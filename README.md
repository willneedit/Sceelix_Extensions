These are some extensions I did for the Sceelix Procedural Geometry Generator, available for free on Steam.

## Compilation

* If you've installed Sceelix from Steam, compiling it by Visual Studio 2019 or newer or by Visual Studio Code (with C# addon) would just fine.
* If you've installed Sceelix in a non-standard 'well-known' location, link (or copy) the 'Bin' folder of your Sceelix installation (usu. in <path to Steam Library>\steamapps\common\Sceelix\Bin) into this folder, rename it to "Assemblies".
* Otherwise you can edit the Reference paths in the project file to point towards the correct location - look at the .csproj file, line 35f.

## Installation

* Link (or copy) the resulting output (Sceelix.Extensions.dll and Sceelix.Extensions.xml) to <your home folder>\Documents\Sceelix\Plugins (the folder next to the default Sceelix project folder)
* having done that, you should see additional nodes in Sceelix under "Other"

## New nodes

### Mesh UV Atlas

This node takes the UV coordinates of the vertices of a given mesh and translates them to a given coordinate space.

Whereas the node "Mesh UV" would always create UV coordinates within the range of (0,0)..(1,1), unless scaled, this node relocates them to a different coordinate space.

This is particularly useful if you have a texture containing a set of different images and the face of the mesh should display only one of them rather than all of them. Most common example would be a texture containing a whole charset and picking out specific letters.

### Mesh Boolean

Perform boolean operations (Intersection, Union or Difference) of two meshes, an extension of the Boolean operation in Sceelix for 3D meshes, not just coplanar faces. Useful to produce cutouts or combination of meshes.
Input meshes' normals can be flipped as the 'negation' before or after of the intended operation, or to treat the meshes as 'outside' or 'inside'.

### Mesh Tessellation

Slices the faces of a mesh into smaller faces, without changing the general shape. A companion piece for [Mesh Distort](#mesh-distort).

### Mesh Distort

Move the individual vertices randomly in the desired direction, Planes get a 'crumpled' appearance, and solids (like cubes or spheres) get a natural, rock-like appearance, especially with using more than one iterations with tessellations and distortions with varying amounts.

### Unity Entity Group Create

An amalgamation of "Actor Group" and "Unity Entity Create". Groups several actors (including other Unity Entities and Groups) together and decorates them with a meaningful name and attributes as seen in "Unity Entity Create"
