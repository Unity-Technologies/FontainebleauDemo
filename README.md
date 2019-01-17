# Fontainebleau

<img src = "https://forum.unity.com/proxy.php?image=https%3A%2F%2Fblogs.unity3d.com%2Fwp-content%2Fuploads%2F2018%2F03%2Fimage5-1280x720.png&hash=d4dd82baaada0823f75c693064c8c964" title = "Fontainebleau title screen" alt >

We created the Fontainebleau demo to illustrate the photogrammetry workflow and the use of the LayeredLit shader. 

This technical demo is authored with game development condition in mind: it’s a representative game level and targets the standard PlayStation 4 platform at 1080p @ 30fps. 

The level represents a part of the Fontainebleau forest and uses a limited set of meshes and textures that are reused with different variation with the help of the LayeredLit shader. 

There is a playable first person and third person mode to walk inside the forest. Targeting consoles like XboxOne or PlayStation 4 requires consideration of how to get the most from these platforms.

The demo also supports three different lighting condition to illustrate that correctly authored and de-lighted assets work fine in any lighting condition:

<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/0.PNG" >

- ​    Day lighting
<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/3.PNG" >

- ​    Sunset lighting
<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/5.PNG" >

- ​    Night lighting with lights off
<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/6.PNG" >

- ​    Night lighting with lights on
<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/7.PNG" >

Finally, we included 3 modes to explore the demo:

<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/1.PNG" title = "Fontainebleau menu screen" alt width="200" height="400" >

-   Cinematic mode: select your lighting program, then sit back, relax and enjoy the show,
-   First Person & Third Person Modes: these are very rudimentary exploration modes to let you discover the environment on your own, with bonuses in First Person mode.

<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/11.PNG" >
<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/4.PNG" >

**Why Fontainebleau?**

Fontainebleau is the name of a forest close to the Unity Paris office. The forest is a good subject to speak about photogrammetry. Natural assets are often complex and hard to reproduce realistically. For our artists, it was important to have the subject close to them to go on site and do all the tests needed to analyze the best workflow possible for games.

**The features implemented in this demo are:**

- Deferred rendering
- Layered lit shader
- Volumetric fog
- Tessellation
- Decals
- Planar reflections
- Screen Space Reflections
- Micro shadowing
- Contact shadows (screen-space shadows)
- Post processing
- Local fog density volume
- Fabric Shader graph (3rd character mode)
- Cinemachine

# Setup

- Download **Unity 2018.3.0f2** or a more recent version of 2018.3

- Clone the repository using the tool of your preference (Git, Github Desktop, Sourcetree, ...). 

  | IMPORTANT                                                    |
  | ------------------------------------------------------------ |
  | This project uses Git Large Files Support (LFS). Downloading a zip file using the green button on Github **will not work**. You must clone the project with a version of git that has LFS. You can download Git LFS here: <https://git-lfs.github.com/>. |

- Open the repository folder in Unity. **The first time you open the project Unity will import all the assets, this operation can take more than 1 hour.**

# Exploring the project

- Once the editor is ready, take a look at the menu bar : several menus are added compared to an empty Unity project. The **"Load levels" menu** offers you 2 options :
  - The **Day, Sunset, and Night buttons** are shortcut that will load the main scene plus one of the different lighting scenes used in the project but not the gameplay elements
  - The **Loader** button allows you to load our bootstrap scene. When this scene is loaded you can enter **Play mode** and experience the demo as in a built executable.

# Controls layout

## Keyboard

<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/8.PNG" >

## Gamepad

<img src = "https://github.com/Unity-Technologies/FontainebleauDemo/blob/master/Documentation/Images/9.PNG" >

# Scripts used in this demo
  
- Character controller
- Gameplay ingredients
- Video manager
- Lightmap switching script
- Lightmapped LODs
