Vertex Painter for Unity
©2015 Jason Booth
Tested with Unity 5.3.2p2

Example Video:

quick tutorial:
https://www.youtube.com/watch?v=mtbCtwgI440&feature=youtu.be
water and refraction:
https://www.youtube.com/watch?v=V70GQjOH8_Y&feature=youtu.be
distance based UV scaling:
https://www.youtube.com/watch?v=ESfTK4TYFQ4
custom brushes:
https://www.youtube.com/watch?v=UtfpgzvrLUI

Love that I'm giving away this professional quality software instead of selling it on the asset store? You can donate here:
https://pledgie.com/campaigns/31113

Warning: This package can take a long time to import into Unity due to a large number of shader variants; if you do not need the shaders and only want the vertex painter, I suggest removing them from the project before importing. They are fantastic shaders though..

A note on the license:

I have been asked by several people if I mind them reselling parts or all of this
package in the Unity Asset Store. I have amended the license to prohibit this
use case. I have given this package away for free and put considerable work into
it, if you find that your product is enhanced by it’s use, then please point your
users towards my repository. Thank you.

Additional Vertex Streams

	This package allows you to paint information onto the vertices of a mesh in the Unity editor as well as modify any attribute of the mesh. It uses the new 'additionalVertexStream' system of Unity5, which allows you to override per-instance data on meshes without paying the cost of duplicating a full mesh. This makes it ideal for painting vertex information across many instances of a mesh. The tool also allows you to easily bake that information back to mesh assets if you'd prefer to make a modified mesh and store that in disk, instead of with the instance in the scene. 

Features
	The toolset contains several different painting modes as well as a few tools that come in handy when doing this type of work.You can modify the positions, normals, UVs and colors of a mesh, painting colors, values, or direction vectors taken from the stroke. You can bake lighting or ambient occlusion data into the vertices, or bake any of the changes you’ve made into a new mesh asset on disk. 


Precision

	Different channels of mesh data support different levels of precision. For instance, the color channel stores a color in 8888 format, which means you can only store 0 to 1 values with 256 possible values in each component of the color channel. The 4 UV channels, however, provide 4 32-bit floats each. It is important to understand these limitations when working with vertex data. 

Interface


Paint
	This tool allows you to paint information directly onto the vertices Color or UV channels. Data can be viewed as either color information or greyscale information if you are working on individual channels. You can even extend the system with custom brushes that you define for your project, so if you want to paint onto the color and UV channels in one stroke, you can! This is really useful if you want to pack various sub-material values into vertices to save on sampler ops. 

Deform
	This tool allows you to modify the vertices positions, properly recalculating normals and tangents. 

Flow

	This tool allows you to paint directional vectors into the color or UV channels, which can be used to create flowing effects in shaders. Note that the tool currently computes direction with a dot product of the tangent and bitangent of your mesh- this means that if your tangents are not in the expected space, the flow direction may be computed wrong.

Bake
	This tab contains a number of utilities, such as the ability to bake AO and lighting into the mesh data. You can also bake information from a texture into your vertices, bake pivot points into the UV channels of the mesh (allowing you to combine many objects and animate them individually in the shader). Finally, you can save meshes as disk assets from this tab.

Custom
   This tab allows you to work with custom brushes, which can be written to do just about anything you can imagine. An example custom brush is included which paints simplex noise onto a model, but the system allows you to write your own custom brushes, apply arbitrary transformations to the vertex data as you paint, as well as provide an interface for your brush.

Shaders

	Included with the package is a ‘SplatBlend’ shader. The SplatBlend shader closely mirrors Unity’s Standard Shader, but allows you to blend up to 5 different textures together per vertex. The blending is handled via a hight map stored in the alpha channel of the diffuse map, to produce a natural looking blend that allows sand to appear in the cracks of the stone before covering the stone, etc. Values painted into the color channels of the vertices control how much of each texture appears at each vertex. The Specular, Emissive, Metallic, and Normal data is handled in the same manner as with the Standard Shader. 

	Each layer has controls for texture scaling, parallax height, and a contrast which controls the width of the blending area between each layer. 

	The shader also allows you to designate one layer of the shader to flow mode. This distorts the texture along directional vectors painted into the third UV set (The first UV set is used for your UV mapping, and Unity uses the second UV set for enlighten data). This is useful to create flowing water, lava, or other effects. The flow layer contains controls for speed and intensity, allowing you to modify the effect globally across the material. The flow layer can optionally use alpha and refraction to distort the layers below it. The normal map will be used as the refraction direction.  

	There is also an option for Distance UV blending- this is mostly useful for terrain type features. What this does is generate a scaled set of UVs and blend between two samples based on the distance from the camera. 

	The performance of this shader is highly variable based on how many layers you use and which features you enable. Features which are not used in any layers are compiled out; while a feature used on any layer is computed for every layer. In other words, if you don’t use emissive textures on any layer you won’t pay for the feature, but if you use it on one layer an emissive value will be sampled for every layer.

	To understand performance characteristics, here is the number of samples taken for various options on a 5 layer shader with a flow map on one layer:

	Diffuse Only         = 4 texture samples + 2 flow texture samples
	With Normal Map      = 8 texture samples + 4 flow texture samples
	With Specular Map    = 12 texture samples + 4 flow texture samples
	With Parallax        = 16 texture samples + 4 flow texture samples
	With Distance Blend  = 32 texture samples + 4 flow texture samples

	As you can see, the complexity of the shader can grow very fast as features are enabled. 

	Because these features are compiled out when not in use, what the shader compiler is actually doing is compiling a shader for each option and picking the correct shader. This means that importing the shaders actually produces many thousands of shader variants, which makes import time very, very long. Before shipping, you may want to use Unity’s ShaderVariantCollection feature to remove the shaders your game does not use; this will make your app much smaller, as it will remove unused variants.

	Pivot Example:

    The pivot example contains a small tutorial that will show you how to bake the pivot of many objects into the vertices, then combine the object into one object and still rotate them all around their own pivots in the shader. This can be very useful for a number of effects, such as rotating asteroids, objects which shatter into pieces, etc, and is often many times cheaper than animating these effects any other way. 
