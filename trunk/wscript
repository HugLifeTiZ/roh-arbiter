#!/usr/bin/env python
#
# Copyright (c) 2010 Trent McPheron <twilightinzero@gmail.com>
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

import Options

# For creating a source archive.
APPNAME = 'arbiter'
VERSION = '0.5.3'

# Required waf stuff.
top = '.'
out = 'build'

def configure (conf):
	# Check for required stuff.
	conf.check_tool('cs config_c misc')
	args = '--cflags --libs'
	conf.check_cfg(package = 'glade-sharp-2.0', atleast_version = '2.12',
	               uselib_store = 'GLADE', mandatory = True, args = args)

def build (bld):
	# Prepare files.
	src_files = bld.path.ant_glob('src/*.cs')
	data_files = ''
	for n in bld.path.ant_glob('data/*.glade data/*.png ' +
	                           'data/*.cfg', flat = False):
		data_files += n.abspath() + ' '
	
	# Compile the program.
	src = bld(
		features = 'cs',
		source   = src_files,
		type     = 'exe',
		target   = 'arbiter.exe',
		resources = data_files,
		flags    = '-pkg:glade-sharp-2.0 /platform:x86 /nowarn:0169')

