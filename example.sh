# SCRIPT directory
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly POWERSHELL_EXE="/c/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe"
readonly MSBUILD_EXE="'C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe'"
readonly NUGET_EXE="${SCRIPT_DIR}/tool/tests/nuget.exe"
readonly OUTPUT_FILE="example.log"
readonly EXTRACT_RESULTS_SCRIPT="${SCRIPT_DIR}/tool/tests/extract_results.sh"
readonly TESTER_EXE="${SCRIPT_DIR}/build/EkstaziSharp.Tester/ekstaziSharpTester.exe"
readonly SLN_FILE="${SCRIPT_DIR}/tool/ekstaziSharp.sln"

if [ ! -f "${TESTER_EXE}" ]; then
    # ekstazi# not built so build it
    echo "Building ekstazi#"
    # fetch needed packages
    "${NUGET_EXE}" restore "${SLN_FILE}" > "${OUTPUT_FILE}"
    ${POWERSHELL_EXE} -Command "& {
      & ${MSBUILD_EXE} tool/ekstaziSharp.sln /verbosity:quiet > ${OUTPUT_FILE}
    }"
fi

# Setting up project under test
if [ ! -d ~/FluentValidation ]; then
    echo "Cloning an example project"
    git clone https://github.com/JeremySkinner/FluentValidation.git ~/FluentValidation
fi

echo "Building a project under test"
(
    cd ~/FluentValidation
    git checkout 445f6390f31b4851efb274cef6bddac184c873f7 &> /dev/null
    ${POWERSHELL_EXE} -Command "& {
      & ${MSBUILD_EXE} src/FluentValidation.Tests/FluentValidation.Tests.csproj /verbosity:quiet > ${OUTPUT_FILE}
    }"
)

# -Running tests using ekstazi#
echo "Running tests using ekstazi#"
"${TESTER_EXE}" \
              --testSource LocalProject \
	      --projectPath ~/FluentValidation \
	      --programModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.dll \
	      --testModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.Tests.dll \
	      --testingFramework XUnit2 \
	      --outputDirectory ~/FluentValidation \
	      --inputDirectory ~/FluentValidation \
	      --debug
	      
# Check execution logs
echo "Execution log files are located on: ~/FluentValidation/.ekstaziSharp/ekstaziInformation/executionLogs/"

${EXTRACT_RESULTS_SCRIPT} ~/FluentValidation/.ekstaziSharp/ekstaziInformation/ "${SCRIPT_DIR}/summary.txt"
echo "Summary: ${SCRIPT_DIR}/summary.txt"
