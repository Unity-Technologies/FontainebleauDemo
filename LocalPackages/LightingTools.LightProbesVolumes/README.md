# LightingTools.LightProbesVolumes
Light probes volumes

Requires Unity 2018.1 or above.

# Setup instructions :

- In your project folder create a "LocalPackages" folder next to your "Assets" folder
- In the LocalPackages folder extract this repo under a "LightingTools.LightProbesVolumes" folder
- In the "Packages" folder open the "manifest.json" in a text editing software
- in "manifest.json" under "dependencies" add the line :
"li.lightingtools.core": "file:../LocalPackages/LightingTools.LightProbesVolumes" (you need to add a "," if this is not the last dependency)
- open the project and profit !

# How to use it :

- In the hierarchy view click : Create / Light / Lightprobe Volume
- Set the size of the box collider to the size of the area you want to place lightprobes in ( if you have a ceiling it is recommended to set the vertical bounds lower to the ceiling, or it will spawn probes on top of it).
- Set the " Light probe volume settings" : 
  - vertical and horizontal spacing ( one probe every X meters )
  - offset from floor is at which vertical distance from the collision you want to spawn the first layer of probes
  - number of layers is the number of probes that will be placed vertically above the hit collider
  - follow floor : when enabled the script performs raycast in order to place lightprobes above existing static geometry that has a collider. When disabled the lightprobes are just placed above the lower face of the volume.
  - fill volume enabled will fill the whole height of the volume instead of just doing X number of layer. When this is enabled the number of layers is ignored.
  - discard inside geometry will test if your probe is inside an object with collisions. This will only work if the top face of your volume is not itself inside an object with collisions. In order to check this enable "draw Debug" and fill the volume : the green cross at the top has to be located in the air and not inside a geometry.
- Click the button !
- When you have several volumes setup in your scene and you want to refresh them all :
- Go to lighting / Refresh lightprobes volumes. This will place again the probes in all the volumes in the scene.
  
# Improvements I would like to do :

- Replace the raycast to colliders by raycast to meshrenderers

# Troubleshoot :

- if the script doesn't place any lightprobe, make sure your geometric is marked as static, and that it has a collider. Using colliders isn't ideal but I haven't found a good solution that would work without them.

# Contributions :

This was originally based on the script shared by ghostmantis games : http://ghostmantis.com/?p=332
