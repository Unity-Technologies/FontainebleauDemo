# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)

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
