These are some extensions I did for the Sceelix Procedural Geometry Generator, available for free on Steam.

## Compilation

* Link (or copy) the 'Bin' folder of your Sceelix installation (usu. in <path to Steam Library>\steamapps\common\Sceelix\Bin) into this folder, rename it to "Assemblies". Otherwise you can edit the Reference paths in the project file to point towards the correct location.
* Compile the library using Visual Studio. Tested fine with VS2019 Community Edition.

## Installation

* Link (or copy) the resulting output (Sceelix.Extensions.dll and Sceelix.Extensions.xml) to <your home folder>\Documents\Sceelix\Plugins (the folder next to the default Sceelix project folder)
* having done that, you should see additional nodes in Sceelix under "Other"

## New nodes

### Mesh UV Atlas

This node takes the UV coordinates of the vertices of a given mesh and translates them to a given coordinate space.

Whereas the node "Mesh UV" would always create UV coordinates within the range of (0,0)..(1,1), unless scaled, this node relocates them to a different coordinate space.

This is particularly useful if you have a texture containing a set of different images and the face of the mesh should display only one of them rather than all of them. Most common example would be a texture containing a whole charset and picking out specific letters.

### Unity Entity Group Create

An amalgamation of "Actor Group" and "Unity Entity Create". Groups several actors (including other Unity Entities and Groups) together and decorates them with a meaningful name and attributes as seen in "Unity Entity Create"
