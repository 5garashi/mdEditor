/*
  File: tools/graphics/geo-entry.js
  Description: Exposes d3-geo and topojson-client as window.MdGeo for drawing ```geojson and ```topojson maps
  Author: 5garashi.com Design Office
  Created: 2026-10-17
  License: MIT
  SPDX-License-Identifier: MIT
*/
// Build with: npm ci && npm run build (writes wwwroot/lib/geo/geo.min.js).
import { geoArea, geoBounds, geoMercator, geoPath } from "d3-geo";
import { feature, mesh } from "topojson-client";

window.MdGeo = { geoArea, geoBounds, geoMercator, geoPath, feature, mesh };
