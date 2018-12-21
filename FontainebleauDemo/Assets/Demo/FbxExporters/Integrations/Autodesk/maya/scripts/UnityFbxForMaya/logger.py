#-
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
#         forbidden unless prior written permission Fis obtained from
#         Unity Technology Aps.
#
########################################################################
#+
"""
@package logger
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnityLoggerUtils Logger Utilities
@ingroup UnityUtils
"""

from UnityFbxForMaya.debug import debug_info
from UnityFbxForMaya.version import pluginName

import maya.OpenMaya as OpenMaya  # @UnresolvedImport

class LoggerMixin(object):
    """
    The LoggerMissing provides logging methods to classes for that they can report debug messages, 
    info messages, warning messages and error messages to the user.
    @ingroup UnityLoggerUtils
    """
    def __init__(self):
        self.displayDebug("__init__")
    
    def __del__(self):
        self.displayDebug("__del__")
    
    @classmethod
    def displayError(self, msg):
        OpenMaya.MGlobal.displayError('{0}: {1}'.format(pluginName(), msg))
    
    @classmethod
    def displayWarning(self, msg):
        OpenMaya.MGlobal.displayWarning('{0}: {1}'.format(pluginName(), msg))
        
    @classmethod
    def displayInfo(self, msg):
        OpenMaya.MGlobal.displayInfo('{0}: {1}'.format(pluginName(), msg))
        
    @classmethod
    def displayDebug(self, *args, **kwargs):
        debug_info(self, *args, **kwargs)
