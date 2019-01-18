# Polybrush 0.9.15

## Features

- New interface style.
- Added example meshes for prefab placement mode.
- ProBuilder compatiblity with 2.9.8+

## Bug Fixes

- Fix icons rendering with a white tint in 2018.1.
- Rebuild normals when additional vertex streams are in use.
- Fix crash when sculpting large objects in 2018.1 and up.
- Placed prefabs now retain their parent connection.

## Changes

- Brush direction default is now "Vertex Normal."

# Polybrush 0.9.14-preview.0

## Bug Fixes

- Compatibility with ProBuilder 2.9.8.
- Fix `Additional Vertex Streams` not working in builds.

## Changes

- Drop support for Unity 5.6 and lower.

# Polybrush 0.9.13-preview.0

## Bug Fixes

- Fix compile error due to `z_ZoomOverride.cs` incorrectly being included in builds.

## Changes

- New example textures.

# Polybrush 0.9.12-preview.0

## Bug Fixes

- Fix compile errors when building to Standalone.
- Fix issue where subdividing in ProBuilder could cause Polybrush to endlessly throw errors.

# Polybrush 0.9.11-preview.0

## Features

- Add Flood paint brush in Vertex Color and Texture painting modes.
- Add utility for baking additional vertex streams back to mesh.
- Set better defaults for Push/Pull and Prefab modes.

## Bug Fixes

- sRGB should be marked false on icons in 5.5 and up.
- Fix deprecated handle warnings in Unity 5.6.
- Updated header backing color to match GUI Color Scheme.
- Fix bug where additional vertex streams would modify static meshes at runtime, causing them to either disappear or be moved.

# Polybrush 0.9.10-preview.0

## Features

- Prefab Brush Mode added (still very early in development).
- Significant performance improvements when working with high vertex count meshes.
- Add a height based blend shader.
- Add texture blend modulate shader.
- Add option to hide vertex dots.

## Bug Fixes

- Fix bug where Polybrush would always instance a new mesh when "Meshes Are Assets" was enabled in ProBuilder.
- Increase text contrast in Polybrush skins.
- Try to crash less when mesh has no normals.
- Fix bug where loading a new scene would prevent brush from updating.
- Fix mismatched text color in command toolbar.
- Fix bug where hovering multiple selected objects would repeatedly instance new brush targets.
- Fix additional space in Color field labels when content is null or empty.
- Fix bug where hovering multiple meshes would sometimes apply the previous mesh vertex positions to the current.
- Fix brush settings anchor setting clipping the top header.
- Fix scene not repainting when applying brush in Unity 5.5.

## Changes

- Store some brush settings per-mode.
- Preferences now stored local to project instead of in Unity Editor preferences.
- Use hinted smooth glyph rasterization instead of OS default for Roboto font.

# Polybrush 0.9.9b2

## Features

- Redesigned interface.
- Add support for "Additional Vertex Streams" workflow.
- Unity 5.5 beta compatibility.
- Add ability to save brush setting modifications to preset.
- Enable multiple axes of mirroring.
- Show color palette as a set of swatches instead of a reorderable list.
- Improve performance when painting larger meshes.
- Handle wireframe and outline disabling/re-enabling correclty in Unity 5.5.
- Clean up various 5.5 incompatibilities.
- Add a question mark icon to header labels with link to documentation page.
- Improve performance by caching mesh values instead of polling UnityEngine.Mesh.
- Improve memory pooling in some performance-critical functions.
- Manual redesigned to better match ProBuilder.
- Mode toolbar now toggles Polybrush on/off when clicking active mode.

## Bug Fixes

- Fix cases where pb_Object optimize could be skipped on undo.
- Fix import settings for icons.
- Don't leak brush editor objects if the brush target has changed.
- Fix serialization warnings on opening editor in Unity 5.4.
- Mark z_Editor brushSettings as HideAndDontSave so that loading new scenes doesn't discard the instanced brush settings, resulting in NullReference errors in the Brush Editor.
- Destroy BrushEditor when z_Editor is done with it so that unity doesn't try to serialize and deserialize the brush editor, resulting in null reference errors.
- Fix texture brush inspector always showing a scroll bar on retina display.
- Fix bug where the texture brush would show a black swatch on the mesh after a script reload.
- Remove shift-q shortcut since it's inconsistent and when it does work interferes with capital Qs.
- Fix bug where OnBrushExit sometimes wouldn't refresh the mesh textures.
- Layer texture blend shaders instead of summing.

## Changes

- "Raise" mode is now "Push Pull."
- Remove vertex billboards from mesh sculpting overlay in favor of just the wireframe.

# Polybrush 0.9.8-preview.0

## Features

- Add option to make brush normal direction sticky to first application.

## Bug Fixes

- Fix Polybrush errors when working with pb 2.6
- Don't specify `isFallback` in CustomEditor implementations since they are not actually fallback editors.
- Don't rely on loaded assembly names matching the expected ProBuilderEditor assembly name when reflecting types and methods.  Fixes null reference when assembly names are changed somewhere in the pipeline.
- Don't bother testing that shader references match when looking for shader metadata since instance ids are so fickle in the first place

# Polybrush 0.9.7-preview.0

## Features

- Significantly improve perfomance in all modes for high vertex count meshes.
- Add "Texture Blend with Vertex Color" shader.
- Add two new default meshes, a smooth and hard icosphere.
- **Texture Mode** backend rewritten entirely to allow for far more complex interactions with shader properties.  See "Writing Texture Shaders" in documentation.
- Add color mask settings to vertex color painter.

## Bug Fixes

- Improve performance of shared edge triangle lookups, fixing lengthy lags when mousing over high vertex count meshes in paint and texture modes.
- When blending multiple brushes in texture mode use the max weight instead of summing.
- Add information about setting shader paths to Hidden in Shaderforge instructions.
- In vertex sculpting modes iterate per-common index instead of per-vertex, improving brush application performance and minimizing chances of splitting a common vertex by accident.
- Minor cleanup of enum types (make sure they're namespaced & remove unused includes).
- Fix bug where script reloads would null-ify texture mode brush color and not properly reset it.
- Fix errors in brush event logic that caused OnBrushEnter/Move/Exit to be called at incorrect times with invalid targets, resulting in crashes when editing prefabs with multiple meshes with different shaders
- Fix undo throwing errors when splat_cache is null.
- Clamp radius max value to min+.001 to avoid crashes when scrolling radius shortcut with equal min/max values.
- Fix uv3/4 not applying or applying to incorrect mesh channels in ProBuilder (requires update to ProBuilder 2.5.1).
- Fix null errors when sculpting pb_Object meshes caused by FinalizeAndResetHovering iterating the same object multiple times.

## Changes

- Set default brush settings radius max to 5.
- When blending multiple brush effects use lighten blending mode instead of additive.
- Shorten ShaderForge source file suffix to "\_SfSrc" (but keep compatibility for older "\_SfTexBlendSrc").

# Polybrush 0.9.6b0

## Bug Fixes

- fc2de1a e0528a8 Fix lag when selecting or hovering an object with large vertex count.
- 76908b2 Increase the min threshold for vertex weight to be considered for movement in raise/lower mode.
- 390f758 Fix out of bounds errors in overlay renderer when mesh vertex count is greater than ushort.max / 4.
- 8b3cb9c Fix typo that caused pb_Objects not to update vertex caches on z_EditableObject Apply calls.

# Polybrush 0.9.5-preview.0

## Bug Fixes

- 114ccc2 Show checkbox in z_Editor context menu for current floating state.
- f44d4d6 When applying mesh values to `pb_Object` also include vec4 uv3/4 attributes.
- c3d3f7a Fix ambiguous method error in z_EditableObject when modifying a ProBuilder mesh.

# Polybrush 0.9.4-preview.0

## Features

- 6eb69c2 Add new "Diffuse Vertex Color" and "TriPlanar Blend Legacy" shaders.

## Bug Fixes

- e775a03 Fix Unity crash when selection contains non-mesh objects.

# Polybrush 0.9.3-preview.0

## Features
- 04b73e8 Add option to ignore unselected GameObjects when a brush tool is in use.
- 04b73e8 Improve detection of meshes in selected children.
- 04b73e8 Add button to clear Polybrush preferences in Settings.

## Bug Fixes
- 3a9533e Fix mirrored brush applying only one brush in texture and vertex paint modes.

# Polybrush 0.9.2-preview.0

## Features

- 5610d2a Instead of a single readme, use a static site generator to build documentation.

## Bug Fixes

- 67a22df Fix triplanar blend shader stretching on some poles.  Add more detailed information on using vertex color and texture blend shader materials
- 3a97e2c When checking mouse picks for selection include children of selected gameObjects as valid targets (fixes issue where a selected model root would not register for brush.
- d0d5527 Fix null reference errors when brush mirroring is enabled.
- 47a2700 Don't throw null ref when weights cache in overlay doesn't match new set.
- 0a2cf5c Add docs for brush settings, interface, and general settings.
- ee601f1 Add misc and troubleshooting section to docs.
- 393fc47 Add warnings that Polybrush does not work on Unity terrain objects

## Changes

- Migrate project to Github.  For access to the latest development builds of Polybrush please email contact@procore3d.com with your invoice number and request Git access (this will be automated in the future).

# Polybrush 0.9.1-preview.1+r4073

## Bug Fixes

- Register children of current selection as valid mesh editables.  Fixes potential confusion when a model prefab is selected at it's root with children.

# Polybrush 0.9.1-preview.0+r4048

## Features

- Texture Blend Mode now supports any combination `UV0, UV2, UV3, UV4, Color, Tangent` mesh attributes, as set by the shader using the syntax `define Z_MESH_ATTRIBUTES UV0 ...`.
- Improve the `Unlit Texture Blend` example shader.
- Add option to automatically rebuild collision meshes.
- Split Direction *Normal* into *Brush Normal* and *Vertex Normal*.
- Improve the behavior of Smooth Mode.
- Remove relax option from Smooth Mode (what was once Normal w/ Relax on or off becomes Vertex Normal and Brush Normal, respectively).
- Add option to keep the brush focused on the first mesh hit when dragging.
- When dragging, always restrict brush application to the current selection.

## Bug Fixes

- Fix issue where Texture Paint Mode would sometimes apply values to the incorrect channel after switching between two different materials with the same shader.
- Make sure that the Texture Paint mode always has valid splat-weights to work with (usually causing "black face errors").
- Fix warnings when shaderforge isn't installed, and update version number.
- Fix instances where setting ProBuilder edit level would not correctly unset Polybrush tool (and vice-versa).

## Changes

- Standard Vertex Color and Standard Texture Blend shaders now default to metallic / roughness workflow.

# Polybrush 0.9.0-preview.0+r3975

Initial Release.
