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
#         forbidden unless prior written permission is obtained from
#         Unity Technologies Aps.
#
########################################################################
#+
"""
@package debug
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnityDebugUtils Debug Utilities
@ingroup UnityUtils
"""
from _collections import defaultdict
import maya.OpenMaya as OpenMaya        # @UnresolvedImport
import maya.cmds              

""" Set this variable to True to display debug messages in the scripting history window.   
@var EnableDebugMessages
@brief Enable the displaying of debug messages through bool variable.  
@ingroup UnityDebugUtils
"""
EnableDebugMessages = True

def debug_info(self, *args, **kwargs):
    """
    Format and print an debug message to stdout.
    @note this is used by LoggerMixin::displayDebug
    @ingroup UnityDebugUtils
    """
    global EnableDebugMessages
    
    if EnableDebugMessages:
        try:
            classname = self.__name__
        except:
            classname = type(self).__name__
            
        prefix= kwargs['prefix'] if kwargs.has_key('prefix') else ''
        tabs= '\t' * int(kwargs['tabs']) if kwargs.has_key('tabs') else ''
        print '{3}{4}{0}.{1} {2}'.format(classname, args[:1], [args[1:]], tabs, prefix) 
    


