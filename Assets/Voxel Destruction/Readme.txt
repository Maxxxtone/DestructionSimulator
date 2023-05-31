--------Voxel Destruction Package--------

Install Unity Jobs and Unity Collections Package

This Package is using BetterStreamingAssets, licenced under MIT
It dosen't work on Android without BetterStreamingAssets, but it is possible to remove it (Visit VoxelObject.cs)

--------Setup--------

NOTE: It is recommended to turn off Vsync

If you want to create a Voxel Object: 
- Model it in Magica Voxel and save the .vox file in the streaming Asset Folder (make sure it only has small letters in the name)
- Create a new GameObject, Add the Voxel Object Component, enter the Path, set the Material to the VoxelModel Material and select Build Model, now there should appear the model in unity
- If the Model appears in a violet Color, the Material Shader is not compatible, visit the section bellow about Render Pipelines
- Now you can change all the values to perfection

Note: 
- The Editor Preview of the Model dosen't have a collider
- Performance is much better on Builds

If you want to make a Object collidable with Voxel Objects:
- The Objects needs to have a rigidbody with a collider
- Add the "Voxel Collider" script to the Object with the Rigidbody
- You can increase or decrese collision scale for more or less destruction

How to improve performance:
- Turn off Vsync
- Split your object into smaller models in MagicaVoxel (every piece then gets drawn separately  which makes everything faster)

--------Render Pipeline--------

For the Voxel Models you just need a Material Shader that supports Vertex Colors, like the Particle Shader

By default the Pacakge is designed for URP (Universal Render Pipeline)

If you are not using URP you will get some warnings about missing components, this is caused by some URP scripts and you can just remove them
These missing components are found on the following Objects and can be removed if you don't want to use URP:
- Directional Light
- Main Camera and Overlay Camera (Childs of the Camera Object)

Built-in render (legacy) Pipeline:
Go to "/Voxel Destruction/Material/Built-In" and assign the Ground Material to ground (in demo scene)
Select every VoxelObject and set the Material Property to the legacy Voxel Material in that Folder

HDRP (High definition Render Pipeline):
If your using HDRP you need to create a new Shader Graph, Create a Vertex Color Node and connect it to color, you can adjust the smoothness to your needs
Create a new Material based on that Shader and set the Material Property on every Voxel Object to the new made Material

--------Errors--------

If you keep getting errors try the following things:
 - Make sure the Collection and Burst package is installed (Package Manager)
 - If your getting Errors about a shader and are not using URP, delete the "/Voxel Destruction/Material/URP" Folder
 - If your getting Allocation Errors on Runtime, switch the Allocator of VoxelObjects to Persistant

Support email: atangameshelp@gmail.com