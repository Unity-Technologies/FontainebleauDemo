#-
########################################################################
# Copyright (c) 2017 Unity Technologies. All rights reserved.
# NOTICE: All information contained herein is, and remains
#         the property of Unity Technology Aps. and its suppliers,
#         if any.  The intellectual and technical concepts contained
#         herein are proprietary to Unity Technologies Aps. and its
#         suppliers and may be covered by Canadian, U.S. and/or
#         Foreign Patents, patents in process, and are protected
#         by trade secret or copyright law. Dissemination of this
#         information or reproduction of this material is strictly
#         forbidden unless prior written permission is obtained from
#         Unity Technology Aps.
#
########################################################################
#+
"""
@package ui
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnityUI User Interface
@ingroup UnityUtils
"""

import maya.cmds              

from UnityFbxForMaya import (commands)

# ======================================================================'
# User Interface
# ======================================================================'

kMenuName = 'UnityFbxForMaya'
kMenuDivider = 'UnityFbxForMayaDivider'
kMenuLabel = 'Unity'
kMenuInsertAfter = 'exportActiveFileOptions'

def register(pluginFn):
    """
    Register commands for plugin
    @param pluginFn (MFnPlugin): plugin object passed to initializePlugin
    """
    installMenu()
    
    return

def unregister(pluginFn):
    """
    Unregister commands for plugin
    @param pluginFn (MFnPlugin): plugin object passed to uninitializePlugin
    """
    uninstallMenu()
    
    return

def getParentMenu():
    """
    Return the name of the parent menu for the Unity menu
    """
    result = maya.mel.eval('$tempVar = $gMainFileMenu;')
    maya.mel.eval("buildFileMenu")
    return result

def whatsNewVersion():
    """
    Return the name of Maya version to be considered 'new' e.g. "2018"
    """
    return maya.cmds.about(q=True, version=True)

def installMenu():
    """
    install menu into main window 
    @ingroup UnityUI
    """
    parentMenu = getParentMenu()

    maya.cmds.menuItem(kMenuDivider, 
                       divider=True, 
                       longDivider=False, 
                       insertAfter=kMenuInsertAfter, 
                       parent=parentMenu, 
                       version=whatsNewVersion())
    maya.cmds.menuItem(kMenuName, 
                       parent=parentMenu, 
                       insertAfter=kMenuDivider, 
                       image=commands.importCmd.familyIconPath(),
                       subMenu=True, 
                       label=kMenuLabel, 
                       annotation=commands.importCmd.kFamilyLabel, 
                       tearOff=True, 
                       version=whatsNewVersion())

    maya.cmds.menuItem(parent=kMenuName, 
                       label=commands.importCmd.kShortLabel, 
                       annotation=commands.importCmd.kLabel, 
                       command=commands.importCmd.kScriptCommand,
                       image=commands.importCmd.iconPath(),
                       version=whatsNewVersion())
    maya.cmds.menuItem(parent=kMenuName,
                       label=commands.exportCmd.kShortLabel, 
                       annotation=commands.exportCmd.kLabel, 
                       command=commands.exportCmd.kScriptCommand, 
                       image=commands.exportCmd.iconPath(),
                       version=whatsNewVersion())

def uninstallMenu():
    """
    uninstall the UnityFbxForMaya menu from main window
    @ingroup UnityUI
    """
    if maya.cmds.menu(kMenuName, exists=True):     # @UndefinedVariable
        maya.cmds.deleteUI(kMenuDivider, menuItem=True)
        maya.cmds.deleteUI(kMenuName, menuItem=True)
        maya.cmds.deleteUI(kMenuName, menu=True)   # @UndefinedVariable
