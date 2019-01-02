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
"""
@package version
@brief version file for UnityFbxForMaya
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnityFbxForMayaPluginVersion Plugin Version
@ingroup UnityFbxForMayaPlugin
"""
VERSION = '1.0.0b1'

def pluginPrefix():
    """
    Return prefix to use for commands and Maya Object names
    @ingroup UnityFbxForMayaPluginVersion
    """
    return 'unity'

def versionName():
    """
    Return version string for the UnityFbxForMaya plugin
    @ingroup UnityFbxForMayaPluginVersion
    """
    return VERSION

def pluginName():
    """
    Return name of UnityFbxForMaya plugin
    @ingroup UnityFbxForMayaPluginVersion
    """
    return 'UnityFbxForMaya'

def vendorName():
    """
    Return vendor name of UnityFbxForMaya plugin
    @ingroup UnityFbxForMayaPluginVersion
    """
    return 'Unity Technology Aps.'
