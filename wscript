#!/usr/bin/env python
#
# Copyright (c) 2010-2011 Trent McPheron <twilightinzero@gmail.com>
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

# For creating a source archive.
APPNAME = 'arbiter'
VERSION = '0.5.3'

# Required waf stuff.
top = '.'
out = 'build'

def configure (ctx):
	# Check for required stuff.
	ctx.load('cs c_config misc')
	args = '--cflags --libs'
	ctx.check_cfg(package = 'glade-sharp-2.0', atleast_version = '2.12',
	               uselib_store = 'GLADE', mandatory = True, args = args)

def build (ctx):
	# Prepare files.
	src_files = ctx.path.ant_glob('src/*.cs')
	data_files = ''
	for n in ctx.path.ant_glob('data/*.glade data/*.png ' +
	                           'data/*.cfg', flat = False):
		data_files += n.abspath() + ' '
	
	# Compile the program.
	ctx.program(
		features = 'cs',
		source   = src_files,
		type     = 'exe',
		gen      = 'arbiter.exe',
		resources = data_files,
		csflags  = '-pkg:glade-sharp-2.0 ' +
		           '/platform:x86 /nologo /optimize- ' +
		           '/codepage:utf8 /main:Arbiter.Arbiter')

def dist (ctx):
	ctx.algo = 'tar.gz'
	ctx.excl = '**/.* **/*~ **/build* **/Packages* **/*.pidb **/*.userprefs'