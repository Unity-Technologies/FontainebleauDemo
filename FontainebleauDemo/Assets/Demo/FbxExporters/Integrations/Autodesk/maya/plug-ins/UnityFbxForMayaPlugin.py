#-
########################################################################
# Copyright (c) 2017 Unity Technologies. All rights reserved.
# NOTICE: All information contained herein is, and remains
#         the property of Unity Technology Aps. and its suppliers,
#         if any.  The intellectual and technical concepts contained
#         herein are proprietary to Unity Technology Aps. and its
#         suppliers and may be covered by Canadian, U.S. and/or
#         Foreign Patents, patents in process, and are protected
#         by trade secret or copyright law. Dissemination of this
#         information or reproduction of this material is strictly
#         forbidden unless prior written permission is obtained from
#         Unity Technology Aps.
#
########################################################################
#+

import sys

import maya.OpenMayaMPx as OpenMayaMPx
import maya.cmds

from UnityFbxForMaya import (version, commands, ui, debug)

kPluginInfo = { 'name': version.pluginName(), 'version': version.versionName(), 'vendor': version.vendorName() }
kVerbose = True
kHeadlessInstall = (maya.cmds.optionVar( exists='UnityFbxForMaya_Headless')
                    and maya.cmds.optionVar(q='UnityFbxForMaya_Headless') == 1)

# initialize the script plug-in
def initializePlugin(mobject):
    pluginFn = OpenMayaMPx.MFnPlugin(mobject, kPluginInfo['vendor'], str(kPluginInfo['version']))
    try:
        if debug.EnableDebugMessages:
            sys.stdout.write('loading %s\n'%kPluginInfo['name'])
        
        commands.register(pluginFn)

        if not kHeadlessInstall:
            ui.register(pluginFn)

    except Exception as e:
        assert isinstance(sys.stderr.write, object)
        sys.stderr.write( "Failed to register plugin: %s" % [kPluginInfo['name'], e] )
        raise

# uninitialize the script plug-in
def uninitializePlugin(mobject):
    pluginFn = OpenMayaMPx.MFnPlugin(mobject)
    try:
        if debug.EnableDebugMessages:
            sys.stdout.write('unloading %s\n'%kPluginInfo['name'])
        
        if not kHeadlessInstall:
            ui.unregister(pluginFn)

        commands.unregister(pluginFn)

    except:
        sys.stderr.write( "Failed to deregister plugin: %s" % kPluginInfo['name'] )
        raise
