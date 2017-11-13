$output_file="README.log"

# INSTRUCTIONS:

# -Fetching EkstaziSharp dependencies
echo "Fetching EkstaziSharp dependencies"
./tool/tests/nuget.exe restore tool/ekstaziSharp.sln > $output_file

# -Building EkstaziSharp project
echo "Building EkstaziSharp project"
msbuild ./tool/Tester/EkstaziSharp.Tester.csproj /verbosity:quiet > $output_file

$oldDir = pwd
cd ~

# -Setting up project under test
$projectCloned = Test-Path "FluentValidation"
if (-Not $projectCloned) {
    echo "Cloning a test project"
    git clone https://github.com/JeremySkinner/FluentValidation.git
}

cd FluentValidation

# -Building a test project
echo "Building a test project"
msbuild src/FluentValidation.Tests/FluentValidation.Tests.csproj /verbosity:quiet > $output_file

cd $oldDir

# -Running tests using EkstaziSharp
echo "Running tests using EkstaziSharp"
./build/EkstaziSharp.Tester/ekstaziSharpTester.exe `
              --testSource LocalProject `
	      --projectPath ${HOME}\FluentValidation `
	      --solutionPath FluentValidation.sln `
	      --programModules src\FluentValidation.Tests\bin\Debug\FluentValidation.dll `
	      --testModules src\FluentValidation.Tests\bin\Debug\FluentValidation.Tests.dll `
	      --testingFramework XUnit2 `
	      --outputDirectory ${HOME}\FluentValidation `
	      --inputDirectory ${HOME}\FluentValidation `
	      --debug `
	      --noappdomain

# -Check execution logs
echo "Execution log files are located on: ${HOME}\FluentValidation\.ekstaziSharp\ekstaziInformation\executionLogs"
