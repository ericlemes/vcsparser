@echo off

set VSWhereLocation=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe

for /f "usebackq tokens=*" %%i in (`"%VSWhereLocation%" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist %InstallDir%\MSBuild\15.0\Bin\MSBuild.exe (
  set MSBuildDir=%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe
)

if exist %InstallDir%\MSBuild\Current\Bin\MSBuild.exe (
  set MSBuildDir=%InstallDir%\MSBuild\Current\Bin\MSBuild.exe
)

exit /b 0