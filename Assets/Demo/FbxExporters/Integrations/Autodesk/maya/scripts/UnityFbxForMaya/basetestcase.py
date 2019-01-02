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
@package basetest
@author  Simon Inwood <simon.cf.inwood@gmail.com>
@defgroup UnitTestUtils UnitTest Utilities
@ingroup UnityUnitTests
"""

import os
import unittest
import maya.cmds
import maya.mel

from UnityFbxForMaya.logger import LoggerMixin

try:
    import maya.standalone             
    maya.standalone.initialize()         
    print "UnityFbxForMaya standalone"
except:
    pass

class BaseTestCase(unittest.TestCase, LoggerMixin):
    """
    UnitTest base class. Provides default behaviour for calling all properties
    and mixes in the LoggerMixin class for reporting.
    @ingroup UnityUnitTests
    """
    __properties__ = None
    # define dependencies on other plugin packages
    kMayaPluginDependency = None
    kMayaScriptDependency = None
    
    def projectPath(self, filepath = None):
        if filepath is None:
            filepath = __file__
        
        return os.path.realpath(os.path.join(os.path.dirname(filepath), '../../', 'resources/scenes'))
    
    def pluginsPath(self, filepath = None):
        if filepath is None:
            filepath = __file__
        
        return os.path.realpath(os.path.join(os.path.dirname(filepath), '../../', 'plug-ins'))
    
    def configEnv(self, required_plugins = None):
        self.displayInfo('configEnv {}'.format(required_plugins))
         
        if required_plugins is None :
            required_plugins = [self.pluginsPath()]
            if self.kMayaPluginDependency:
                required_plugins.append(self.kMayaPluginDependency)
                
        if not os.environ.has_key('MAYA_PLUG_IN_PATH'):
            os.environ['MAYA_PLUG_IN_PATH'] = ''
            
        plugin_paths = os.environ['MAYA_PLUG_IN_PATH'].split(':')
        
        for rp in required_plugins:
            if rp not in plugin_paths:
                self.displayDebug('configEnv_', 'appending plugin path', rp, tabs=1) 
                os.environ['MAYA_PLUG_IN_PATH']='{0}:{1}'.format(os.environ['MAYA_PLUG_IN_PATH'],rp)
    
        if self.kMayaScriptDependency:
            self.displayDebug('configEnv_', 'source maya dependency', tabs=1)
            maya.mel.eval('source "{0}.mel";'.format(self.kMayaScriptDependency))  # @UndefinedVariable
        
        
    def displayInfo(self, msg):
        super(BaseTestCase, self).displayInfo('{0}{1}:{2}{3}'.format('<' * 25, self.__class__.__name__, msg, '>' * 25))
        
    # preparing to test
    def setUp(self):
        """ Setting up for the test """
        self.displayDebug('setUp_', 'begin') 
        self.configEnv()

        projectPath = self.projectPath()
        if os.path.exists(projectPath):
            maya.cmds.workspace(self.projectPath(), o=True)     # @UndefinedVariable
            maya.cmds.workspace(dir=self.projectPath())         # @UndefinedVariable
        else:
            self.displayWarning('project path does not existing, skipping workspace setup'.format(projectPath))

        self.displayDebug('setUp_', 'end', tab=1) 

    
    # ending the test
    def tearDown(self):
        """Cleaning up after the test"""
        self.displayDebug('tearDown_', 'begin')
        maya.cmds.file(force=True, new=True) # @UndefinedVariable
        self.displayDebug('tearDown_', 'end', tab=1) 

    # test routine 
    def test_properties(self):
        import inspect
        
        if self.__properties__:
            """Test routine test_properties"""
            self.displayInfo('test_properties')
            obj = self.__properties__()
            
            for pname, prop in inspect.getmembers(obj.__class__, lambda o: isinstance(o, property)):  # @UnusedVariable
                try:
                    getattr(obj,pname)
                except:
                    self.assertTrue(False, 'failed to get property {}'.format(pname))


