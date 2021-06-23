# ProceduralTerrainBlending_2020
Procedural terrain blending project updated to Unity 2020

Unity Project, blending different landmass types seamlessly to create believable "infinite" procedural  terrain.

Based on https://github.com/sotos82/ProceduralTerrainUnity

Terrain height data is created using 4 different "landmass" noise types: plains, hills, mountains, ridged mountains.
Each terrain object has random combination of landmass types, and adjacent terrains are blended seamlessly using bell shape sine curve falloff function. 

Unity version used: 2020.3.3f1

Usage: Open Menu scene, hit play, and input an integer seed for map and click refresh button. Then wait for several seconds for map to be calculated. Once its done, click point on map and press start to launch the terrain scene.
To make  debugging easier, player is placed on scene and is able to move while terrain objects are still created and populated with data. 

I have been using Scrawk's Ceto ocean with this project for easy and great looking solution for water: https://github.com/Scrawk/Ceto 

Disclaimer:
This is a work in progress hobby project, so there are some optimizations to be made, and code is not polished to production quality
