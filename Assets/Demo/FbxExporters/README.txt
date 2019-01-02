FBX Exporter Package
====================

Copyright (c) 2017 Unity Technologies. All rights reserved.

See LICENSE.txt file for full license information.

VERSION: 1.0.0b1

Requirements
------------

The Unity Integration for Maya is designed to work with Maya 2017 or later.

Please note that MayaLT is not supported at this time.

Installing Unity Integration for Maya
-------------------------------------

The easiest way to install the Unity integration For Maya is from the Fbx Export Settings in Unity.

        MenuBar -> Edit -> Project Settings -> Fbx Export -> Install Unity Integration

It will use the version of Maya specified in the "Maya Application" dropdown located above the
button. The dropdown will show all Maya versions located in the default installation location.
To handle non-default installation locations, either select the browse option in the dropdown
and browse to the desired Maya executable location, or set the MAYA_LOCATION environment variable.

Alternately, you can install the package and integrations from the command-line
using a script, an example of which can be found in the scripts folder of the 
Maya integration. The version for OSX is called install_maya_plugin.sh.