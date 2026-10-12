/*
ファイル名 / File: promotion/player.js
概要 / Description: Keyboard-operable controls for the silent demonstration
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.1.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
*/
(() => {
  const video = document.querySelector('video');
  const controls = document.querySelector('.player-controls');
  if (!video || !controls) return;
  const en = document.documentElement.lang === 'en';
  const play = controls.querySelector('[data-play]');
  const stop = controls.querySelector('[data-stop]');
  const fullscreen = controls.querySelector('[data-fullscreen]');
  const stage = document.querySelector('.video-stage');
  const seek = controls.querySelector('input');
  const clock = controls.querySelector('output');
  const error = document.querySelector('.player-error');
  const time = value => `${String(Math.floor(value / 60)).padStart(2, '0')}:${String(Math.floor(value % 60)).padStart(2, '0')}`;
  function update() {
    const duration = Number.isFinite(video.duration) ? video.duration : 40;
    play.textContent = video.paused ? (en ? 'Play' : '再生') : (en ? 'Pause' : '一時停止');
    seek.max = duration;
    if (document.activeElement !== seek) seek.value = video.currentTime;
    seek.setAttribute('aria-valuetext', `${time(video.currentTime)} / ${time(duration)}`);
    clock.textContent = `${time(video.currentTime)} / ${time(duration)}`;
  }
  play.addEventListener('click', async () => {
    if (!video.paused) { video.pause(); return; }
    try { await video.play(); error.hidden = true; }
    catch { error.hidden = false; }
  });
  stop.addEventListener('click', () => { video.pause(); video.currentTime = 0; update(); });
  fullscreen.hidden = !document.fullscreenEnabled;
  fullscreen.addEventListener('click', async () => {
    try {
      if (document.fullscreenElement) await document.exitFullscreen();
      else await stage.requestFullscreen();
    } catch {
      error.textContent = en ? 'Full screen is unavailable. Download the video below and open it in a video player.' : '全画面表示できませんでした。下から動画を保存し、動画プレーヤーで開いてください。';
      error.hidden = false;
    }
  });
  document.addEventListener('fullscreenchange', () => {
    fullscreen.textContent = document.fullscreenElement ? (en ? 'Exit full screen' : '全画面を閉じる') : (en ? 'Full screen' : '全画面表示');
  });
  seek.addEventListener('input', () => {
    const requested = Number(seek.value);
    video.currentTime = requested;
    seek.setAttribute('aria-valuetext', `${time(requested)} / ${time(Number.isFinite(video.duration) ? video.duration : 40)}`);
  });
  seek.addEventListener('blur', update);
  for (const event of ['loadedmetadata','play','pause','timeupdate','seeked','ended']) video.addEventListener(event, update);
  video.addEventListener('error', () => { error.hidden = false; });
  video.controls = false;
  controls.hidden = false;
  update();
})();
