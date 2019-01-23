@echo off

del stdout.log /f
rd Builds /s /q
set unity="C:\Program Files\Unity\Hub\Editor\2019.1.0a9\Editor\Unity.exe"
set mypath=%cd%
@echo %mypath%

rem Uncomment to build spesific flavours
%unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildWindows
rem %unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildAndroid
%unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildWeb

