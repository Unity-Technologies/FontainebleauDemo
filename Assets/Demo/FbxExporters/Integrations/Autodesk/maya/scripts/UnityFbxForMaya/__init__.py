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
Maya Unity Integration
@package UnityFbxForMaya
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnityFbxForMayaPlugin Unity Plugin

@brief

@details

@defgroup UnityUtils Utilities
@defgroup UnityCommands Commands
@defgroup UnityUI User Interface
@defgroup UnityUnitTests Unit Tests
"""

# list of public modules for this package
__all__ = ["commands", "ui"]

try:             
    import maya.standalone             
    maya.standalone.initialize()         
    print "Unity standalone"
except: 
    pass

