/*
  File: tools/graphics/copy-data.js
  Description: Copies the world outline (world-atlas, from Natural Earth) and the licenses next to the bundles
  Author: 5garashi.com Design Office
  Created: 2026-10-17
  License: MIT
  SPDX-License-Identifier: MIT
*/
const fs = require("fs");
const path = require("path");
const out = path.join(__dirname, "../../wwwroot/lib");
fs.copyFileSync(require.resolve("world-atlas/countries-110m.json"), path.join(out, "geo/countries-110m.json"));
const license = (name) => fs.readFileSync(path.join(__dirname, "node_modules", name, "LICENSE"), "utf8").trim();
fs.writeFileSync(path.join(out, "three/LICENSE"), `three.js 0.186.1 (MIT): the core, STLLoader, and OrbitControls, bundled unmodified by tools/graphics.\n\n${license("three")}\n`);
fs.writeFileSync(path.join(out, "geo/LICENSE"), [
  "geo.min.js bundles d3-geo 3.1.1 and topojson-client 3.1.0 unmodified (tools/graphics).",
  "countries-110m.json is from world-atlas 2.0.2, made from Natural Earth (public domain, https://www.naturalearthdata.com/).",
  "", "== d3-geo (ISC) ==", license("d3-geo"),
  "", "== topojson-client (ISC) ==", license("topojson-client"),
  "", "== world-atlas (ISC) ==", license("world-atlas"), ""].join("\n"));
console.log("data and licenses copied");
