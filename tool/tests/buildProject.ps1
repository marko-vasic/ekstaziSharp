$solutionPath = $args[0]
cmd.exe /c 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat'
cmd.exe /c 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe' $solutionPath /build Debug