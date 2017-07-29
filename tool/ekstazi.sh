#!/bin/bash

# 1. Retrive neccessary dependencies.
( cd tests
  nuget.exe restore ../ekstaziSharp.sln -packagesdirectory ../packages
)

# 2. Include the following two lines in all csproj files.  Note that
# these lines are also needed in all projects under test
# (https://github.com/Humanizr/Humanizer/issues/537):
# <Reference Include="System.Runtime"/>
# <Reference Include="System.Globalization"/>

# 3. Build the entire project.
msbuild ekstaziSharp.sln

# 4. Run tests.
( cd ekstaziSharpTester/bin/Debug/
  ekstaziSharpTester.exe configs/cSharpTestProject.json
)
