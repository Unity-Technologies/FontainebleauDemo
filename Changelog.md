# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
## Unity 2019.4.36f1 - HDRP 7.7.1

### **Changed**

- Upgraded the project to 2019.4.36f1
- Updated packages

## Unity 2019.4.1f1 - HDRP 7.4.1

### **Fixed**

- Fixed the wind not updating
- Removed dangling meta files

## Unity 2019.3.0f6 - HDRP 7.2.0

### **Changed**

- Use **Physically based sky** (preview) for night lighting setup with clouds rendered on top using a shader graph
- Photogrammetry kit scene : disabled Static batching from some gameobjects to avoid differences in displacement in play mode

### **Fixed**

- Fixed bright lighting when opening night lighting in editor
- Removed legacy UI that wasn't relevant anymore
- Fixed typos in the Discover items

## Unity 2019.3.0f1 - HDRP 7.1.6

### **Added**

- The **Discover window** helps you load the different scene setups of the project and inspect various systems/objects in the editor. It automatically launches on startup or find it in **Help/Discover Fontainebleau**.
- Use **Physically based sky** (preview) for day lighting setup
- Use a **Default Volume Profile** for setting up shared render settings between all scenes

### **Fixed**

- (Shader graph) Fixed issue where one had to manually reimport sub graphs

### **Changed**

- Switched hardcoded game logic to **Gameplay Ingredients** scripts with **state machines, events, logic and actions**
- **Reflection probes** are no longer baked in the editor but **captured at runtime** when the scene is loaded, removing the need to store the cubemap assets in the project and in the build.

### Removed

- Removed **Load Level** menu item, now replaced by the **discover window** (automatically launches on startup or go to Help/Discover Fontainebleau)

### Known issues

- On some platforms the reflection probes get captured before the Light Probe Proxy Volumes are updated leading to **white reflection probes in the night lighting**
- Warning about Detail rendering shader not found (this warning should be removed in the future, it is not valid in this context)



## Unity 2019.1.3f1 - HDRP 5.16.0

### Added
- Shader graph wind **Basic Wind** added to the project
- Added **vertex repulsion** around third person character
- Added **hair shader graph**
- Added **quick search** package

### Fixed
- Fixed shader compilation errors with lens flares
- Fixed custom timeline tracks to work with new post processing

### Changed
- HDRP now uses **embedded** **postprocessing**
- **Volume framework** is no longer experimental
- Ground Mesh is no longer made of Mesh Renderers it is using the **Terrain system**
- Foliage is now using **Basic Wind** instead of the **deprecated wind built in the lit shader**
- Cinematic is now relying on **cinemachine** and virtual cameras
- **Polybrush** tool is now used as a package and no longer in the project
- **FBX exporter** is now used as a package and no longer in the project
- Info panels images are now rendered in **AfterPostprocess pass**
- Night sky stars are now made using **Visual Effect Graph**



## Unity XXXX.X - HDRP X.X.X

- 
