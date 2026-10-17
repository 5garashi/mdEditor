/*
  File: tools/graphics/three-entry.js
  Description: Exposes the three.js pieces mdEditor uses to show ```stl models as window.MdThree
  Author: 5garashi.com Design Office
  Created: 2026-10-17
  License: MIT
  SPDX-License-Identifier: MIT
*/
// Build with: npm ci && npm run build (writes wwwroot/lib/three/stl-viewer.min.js).
import {
  AmbientLight, Box3, Color, DirectionalLight, Mesh, MeshStandardMaterial, PerspectiveCamera, Scene, Vector3, WebGLRenderer
} from "three";
import { STLLoader } from "three/examples/jsm/loaders/STLLoader.js";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls.js";

window.MdThree = {
  AmbientLight, Box3, Color, DirectionalLight, Mesh, MeshStandardMaterial, PerspectiveCamera, Scene, Vector3, WebGLRenderer,
  STLLoader, OrbitControls
};
