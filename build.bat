@echo off

echo ======= Cleanup =======
del stdout.log /f
rd Builds /s /q
rd Temp /s /q

set unity="C:\Program Files\Unity\Hub\Editor\2019.3.0a7\Editor\Unity.exe"
set mypath=%cd%


echo Building: %mypath%
echo ======= Web =======
%unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildWeb

echo ======= Windows =======
%unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildWindows

REM echo ======= Android =======
REM %unity% -quit -batchmode -logFile stdout.log -projectPath %mypath% -executeMethod Builder.BuildAndroid

start .\Builds