########################################################################
# Copyright (c) 2017 Unity Technologies. All rights reserved.
# NOTICE: All information contained herein is, and remains
#         the property of Unity Technologies Aps. and its suppliers,
#         if any.  The intellectual and technical concepts contained
#         herein are proprietary to Unity Technologies Aps. and its
#         suppliers and may be covered by Canadian, U.S. and/or
#         Foreign Patents, patents in process, and are protected
#         by trade secret or copyright law. Dissemination of this
#         information or reproduction of this material is strictly
#         forbidden unless prior written permission is obtained from
#         Unity Technologies Aps.
#
########################################################################

Automatic Installation
===================

The easiest installation method is to launch Unity and use the
        MenuBar -> Edit -> Project Settings -> Fbx Export -> Install Unity Integration
button.

It will use the version of Maya specified in the "Maya Application" dropdown located above the
button. The dropdown will show all Maya versions located in the default installation location.
To handle non-default installation locations, either select the browse option in the dropdown
and browse to the desired Maya executable location, or set the MAYA_LOCATION environment variable.

Manual Installation
===================

Instructions for installing if you don't use the unity package installer
and your installing in a non-default location.

1. copy UnityFbxForMaya.mod to user folder

    MacOS & Ubuntu: ~/MayaProjects/modules
    Windows:        C:\Users\{USER}\Documents\maya\modules

2. configure path within UnityFbxForMaya.mod to point to integration installation folder

    {UnityProject}/Assets/FbxExporters/Integrations/Autodesk/maya


Running Unit Tests
==================

MacOS

export MAYAPY_PATH=/Applications/Autodesk/maya2017/Maya.app/Contents/bin/mayapy
export MAYA_INTEGRATION_PATH=${UNITY_PROJECT_PATH}/Assets/FbxExporters/Integrations/Autodesk/maya
export PYTHONPATH=${MAYA_INTEGRATION_PATH}/scripts

# run all tests
${MAYAPY_PATH} ${MAYA_INTEGRATION_PATH}/scripts/run_all_tests.py

# run one test
${MAYAPY_PATH} ${MAYA_INTEGRATION_PATH}/scripts/UnityFbxForMaya/commands.py
