# Fontainebleau

<img src = "https://forum.unity.com/proxy.php?image=https%3A%2F%2Fblogs.unity3d.com%2Fwp-content%2Fuploads%2F2018%2F03%2Fimage5-1280x720.png&hash=d4dd82baaada0823f75c693064c8c964" title = "Fontainebleau title screen" alt >

We created the Fontainebleau demo to illustrate the photogrammetry workflow and the use of the LayeredLit shader. This technical demo is authored with game development condition in mind: it’s a representative game level and targets the standard PlayStation 4 platform at 1080p @ 30fps. The level represents a part of the Fontainebleau forest and uses a limited set of meshes and textures that are reused with different variation with the help of the LayeredLit shader. There is a playable first person and third person mode to walk inside the forest. Targeting consoles like XboxOne or PlayStation 4 requires consideration of how to get the most from these platforms.
The demo also supports three different lighting condition to illustrate that correctly authored and de-lighted asset work fine in any lighting condition:
    Day lighting
    Sunset lighting
    Night lighting with lights off and on

Finally, we included 3 modes to explore the demo:
  Cinematic mode: select your lighting program, then sit back, relax and enjoy the show,
  First Person & Third Person Modes: these are very rudimentary exploration modes to let you discover the environment on your own, with bonuses in First Person mode.

Why Fontainebleau?

Fontainebleau is the name of a forest close to the Unity Paris office. The forest is a good subject to speak about photogrammetry. Natural assets are often complex and hard to reproduce realistically. For our artists, it was important to have the subject close to them to do all the tests needed to analyze the best workflow possible for games.

# The features implemented in this demo are:
* Layer lit 
* Decals
* Planar refelction
* SSR
* Micro shadowing
* Contact shadow
* Cinemachine
* Post process
* Volumetric
* Width desity volume
* Fabric shader graph (3rd character mode)
* Deferred rendering

