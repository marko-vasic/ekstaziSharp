#!/bin/bash

# SCRIPT directory
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly POWERSHELL_EXE="/cygdrive/c/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe"
readonly MSBUILD_EXE="'C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe'"
readonly NUGET_EXE="${SCRIPT_DIR}/tool/tests/nuget.exe"

# INSTRUCTIONS:

# Fetch EkstaziSharp Dependencies
echo "Fetching EkstaziSharp dependencies"
${NUGET_EXE} restore tool/ekstaziSharp.sln

# Clean EkstaziSharp project
echo "Cleaning EkstaziSharp project"
${POWERSHELL_EXE} -Command "& {
    & ${MSBUILD_EXE} tool/Tester/EkstaziSharp.Tester.csproj /t:Clean /verbosity:quiet
}"

# Build EkstaziSharp project
echo "Building EkstaziSharp project"
${POWERSHELL_EXE} -Command "& {
    & ${MSBUILD_EXE} tool/Tester/EkstaziSharp.Tester.csproj /verbosity:quiet
}"

export fvdir=${HOMEDRIVE}${HOMEPATH}\\FluentValidation

# -Setting up project under test
if [ ! -d $fvdir ]; then
    echo "Cloning a test project"
    git clone https://github.com/JeremySkinner/FluentValidation.git $fvdir
fi

# -Building a test project
echo "Building a test project"
(
    cd $fvdir
    ${POWERSHELL_EXE} -Command "& {
      & ${MSBUILD_EXE} src/FluentValidation.Tests/FluentValidation.Tests.csproj /verbosity:quiet
    }"
)

# -Running tests using EkstaziSharp
echo "Running tests using EkstaziSharp"
pwd
./tool/Tester/bin/Debug/ekstaziSharpTester.exe \
              --testSource LocalProject \
	      --projectPath $fvdir \
	      --solutionPath FluentValidation.sln \
	      --programModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.dll \
	      --testModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.Tests.dll \
	      --testingFramework XUnit2 \
	      --outputDirectory $fvdir\\FluentValidation \
	      --inputDirectory $fvdir\\FluentValidation \
	      --debug \
	      -noappdomain
	      
# -Check execution logs
echo "Execution log files are located on: $fvdir/FluentValidation/.ekstaziSharp/ekstaziInformation/executionLogs/"

tool/tests/extract_results.sh $fvdir\\FluentValidation\\.ekstaziSharp\\ekstaziInformation "summary.txt"
echo "summary file contents: "
cat "summary.txt"
