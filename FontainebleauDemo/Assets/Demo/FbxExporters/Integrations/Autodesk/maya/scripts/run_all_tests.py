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
#!/usr/bin/python

import unittest

if __name__ == '__main__':
    testsuite = unittest.TestLoader().discover('.', pattern='*.py')
    unittest.TextTestRunner(verbosity=1).run(testsuite)