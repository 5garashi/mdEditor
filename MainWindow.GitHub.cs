/*
  File: MainWindow.GitHub.cs
  Description: The window's side of GitHub conversion and the GitHub view: answering the page, and sending the document to GitHub's Markdown API only after the user has chosen Send
  Author: 5garashi.com Design Office
  Created: 2026-11-08
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MdEditor;

public partial class MainWindow
{
    // GitHub's Markdown API takes no more than this from one request (documented for the raw
    // form; the same limit is kept here), and answers within the 20 seconds of any fetch.
    private const int GitHubMaxBytes = 400 * 1024;
    private const string GitHubMarkdownApi = "https://api.github.com/markdown";

    // The document goes only to api.github.com, and only when the user has pressed Send in
    // the page's question (Identity 13-1 (2) (a), asked every time). No GitHub sign-in is used:
    // GitHub allows 60 requests an hour from one IP address without it. No cookies, no
    // credentials, no redirects, and never an address on this PC or the local network.
    private static readonly HttpClient GitHubHttp = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        Credentials = null,
        PreAuthenticate = false,
        ConnectCallback = ConnectToPublicAddressAsync
    })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private int? _gitHubRemaining;
    private int? _gitHubLimit;
    private DateTimeOffset? _gitHubReset;
    private bool _gitHubBusy;
    // GitHub's last answer and the text it was for: shown again, without asking or sending,
    // while the text is the same.
    private string? _gitHubLastText;
    private string? _gitHubLastHtml;
    private string? _gitHubLastAt;

    // The page sends the editor's text and selection; the answer is what would be rewritten
    // (githubConvertPlan), or the edits for the kinds the user kept checked (githubConvertEdits).
    private void SendGitHubConversion(JsonElement root, bool apply)
    {
        var text = root.GetProperty("text").GetString() ?? string.Empty;
        var from = root.TryGetProperty("from", out var fromValue) ? fromValue.GetInt32() : 0;
        var to = root.TryGetProperty("to", out var toValue) ? toValue.GetInt32() : 0;
        var lineBreaks = root.TryGetProperty("lineBreaks", out var lineBreaksValue) && lineBreaksValue.ValueKind == JsonValueKind.True;
        HashSet<string>? kinds = apply && root.TryGetProperty("kinds", out var kindsValue)
            ? kindsValue.EnumerateArray().Select(kind => kind.GetString() ?? string.Empty).ToHashSet(StringComparer.Ordinal)
            : null;
        GitHubPlan plan;
        try
        {
            plan = GitHubConversion.Plan(text, Math.Clamp(from, 0, text.Length), Math.Clamp(to, 0, text.Length), lineBreaks, kinds);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            SendToPage(new { type = apply ? "githubConvertEdits" : "githubConvertPlan", failed = true });
            return;
        }

        if (apply)
        {
            SendToPage(new
            {
                type = "githubConvertEdits",
                edits = plan.Edits.Select(edit => new { from = edit.From, to = edit.To, insert = edit.Insert }),
                count = plan.Kinds.Sum(kind => kind.Count)
            });
            return;
        }

        SendToPage(new
        {
            type = "githubConvertPlan",
            scope = from < to ? "selection" : "document",
            kinds = plan.Kinds.Select(kind => new { kind = kind.Kind, count = kind.Count, example = kind.Example }),
            skipped = plan.Skipped.Select(kind => new { reason = kind.Kind, count = kind.Count, example = kind.Example })
        });
    }

    // What the question before sending shows: how much would be sent and how many requests
    // GitHub still allows this hour (known after the first one).
    private void SendGitHubViewConfirm(Action<object> send)
    {
        var prepared = GitHubView.Prepare(_text);
        if (_gitHubLastHtml is not null && prepared.Text == _gitHubLastText)
        {
            // Nothing has changed since GitHub's last answer: show it again, sending nothing.
            SendGitHubPreview(send, _gitHubLastHtml, prepared, _gitHubLastAt ?? string.Empty, again: true);
            return;
        }

        var bytes = Encoding.UTF8.GetByteCount(prepared.Text);
        send(new
        {
            type = "githubViewConfirm",
            bytes,
            maxBytes = GitHubMaxBytes,
            remaining = _gitHubRemaining,
            limit = _gitHubLimit,
            reset = _gitHubReset?.ToLocalTime().ToString("HH:mm")
        });
    }

    private async Task ShowGitHubViewAsync(Action<object> send)
    {
        if (_gitHubBusy)
        {
            return;
        }

        _gitHubBusy = true;
        try
        {
            var view = GitHubView.Prepare(_text);
            var prepared = view.Text;
            if (Encoding.UTF8.GetByteCount(prepared) > GitHubMaxBytes)
            {
                send(new { type = "githubViewError", code = "tooLarge", maxBytes = GitHubMaxBytes });
                return;
            }

            string html;
            // For the GUI tests only: a saved answer stands in for GitHub, so the tests send
            // nothing. Documents cannot set environment variables.
            var saved = Environment.GetEnvironmentVariable("MDEDITOR_GITHUB_RESPONSE_FILE");
            if (!string.IsNullOrEmpty(saved))
            {
                html = await File.ReadAllTextAsync(saved, Encoding.UTF8);
                _gitHubLimit = 60;
                _gitHubRemaining = 59;
                _gitHubReset = DateTimeOffset.Now.AddHours(1);
            }
            else
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, GitHubMarkdownApi)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { text = prepared, mode = "gfm" }), Encoding.UTF8, "application/json")
                };
                request.Headers.UserAgent.ParseAdd($"mdEditor/{AppVersion} (+https://github.com/5garashi/mdEditor)");
                request.Headers.Accept.ParseAdd("application/vnd.github+json");
                request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                using var response = await GitHubHttp.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                ReadGitHubQuota(response);
                if (!response.IsSuccessStatusCode)
                {
                    var status = (int)response.StatusCode;
                    send(status is 403 or 429 && _gitHubRemaining == 0
                        ? new { type = "githubViewError", code = "rateLimit", reset = _gitHubReset?.ToLocalTime().ToString("HH:mm"), status }
                        : new { type = "githubViewError", code = "status", reset = (string?)null, status });
                    return;
                }

                if (response.Content.Headers.ContentLength > MaxPageBytes)
                {
                    send(new { type = "githubViewError", code = "status", status = 413 });
                    return;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var limited = new MemoryStream();
                var buffer = new byte[81920];
                int read;
                while ((read = await stream.ReadAsync(buffer)) > 0)
                {
                    if (limited.Length + read > MaxPageBytes)
                    {
                        send(new { type = "githubViewError", code = "status", status = 413 });
                        return;
                    }

                    limited.Write(buffer, 0, read);
                }

                html = Encoding.UTF8.GetString(limited.ToArray());
            }

            _gitHubLastText = prepared;
            _gitHubLastHtml = html;
            _gitHubLastAt = DateTime.Now.ToString("HH:mm");
            SendGitHubPreview(send, html, view, _gitHubLastAt, again: false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            send(new { type = "githubViewError", code = "network" });
        }
        finally
        {
            _gitHubBusy = false;
        }
    }

    // GitHub's answer made ready for the preview each time it is shown (the document folder
    // and the consent for internet images may have changed since).
    private void SendGitHubPreview(Action<object> send, string html,
        (string Text, List<string> Fences, List<(string Tag, int Line)> Blocks) view, string at, bool again)
    {
        send(new
        {
            type = "githubPreview",
            html = _htmlSanitizer.Sanitize(GitHubView.ToPreviewHtml(html, view.Fences, view.Blocks)),
            at,
            again,
            remaining = _gitHubRemaining,
            limit = _gitHubLimit
        });
    }

    private void ReadGitHubQuota(HttpResponseMessage response)
    {
        static int? Number(HttpResponseMessage response, string name) =>
            response.Headers.TryGetValues(name, out var values) && int.TryParse(values.FirstOrDefault(), out var value) ? value : null;

        _gitHubRemaining = Number(response, "x-ratelimit-remaining") ?? _gitHubRemaining;
        _gitHubLimit = Number(response, "x-ratelimit-limit") ?? _gitHubLimit;
        if (Number(response, "x-ratelimit-reset") is { } reset)
        {
            _gitHubReset = DateTimeOffset.FromUnixTimeSeconds(reset);
        }
    }
}
