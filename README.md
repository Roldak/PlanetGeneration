# PlanetGeneration
Procedural generation of planets (or asteroids, etc.) using Unity.

## Techniques used:
* 3D fBm using 3D Perlin noise
* Procedural sphere generation from cube normalization for trivial UV mapping
* Vertex displacement using the 3D fBm
* Numerical computation of vertex position's derivates to compute vertex normals
* Multithreaded mesh generation with dynamic LOD using quadtrees
* GPU-based planet texturing using Unity's surface shaders

## Screenshots:

* Asteroid (25/02/2016):

![Initial working version](ScreenShots/25-02-2016.png)

* Earth-like Planet (27/02/2016):

![Earth-like planet](ScreenShots/27-02-2016.png)

* First-person view (29/02/2016):

![First-person view](ScreenShots/29-02-2016.png)

* Realistic textures (03/03/2016):

![Realistic textures](ScreenShots/03-03-2016.png)

* Aerial view (03/03/2016):

![Aerial view](ScreenShots/03-03-2016-2.png)
