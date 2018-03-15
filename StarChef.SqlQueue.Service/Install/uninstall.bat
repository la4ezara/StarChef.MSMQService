@echo off
echo.
echo Be sure to run this command as Administrator
echo First argument should be service name.
echo Second argument should be service location.
echo.

set serviceName="StarChef.SqlQueue.Service"
set servicePath=%1
set installUtilPath="C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil"

for /F "tokens=3 delims=: " %%H in ('sc query %serviceName%^| findstr "STATE"') do (
  if /I "%%H" EQU "RUNNING" (
	echo Service "%serviceName%" found - try to stop service.
	net stop %serviceName%
  )
  
  %installUtilPath% /serviceName=%serviceName% /u %servicePath%
)

echo Uninstall finish