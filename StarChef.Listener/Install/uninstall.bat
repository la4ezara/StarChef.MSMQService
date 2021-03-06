@echo off
echo.
echo Be sure to run this command as Administrator
echo First argument should be service location.
echo.

set serviceName="StarchefMessageListner"
set servicePath=%1
set installUtilPath="C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil"

if EXIST %servicePath% ( 
	echo "Path found %servicePath%."
	for /F "tokens=3 delims=: " %%H in ('sc query %serviceName%^| findstr "STATE"') do (
	  if /I "%%H" EQU "RUNNING" (
		echo Service "%serviceName%" found - try to stop service.
		net stop %serviceName%
	  )
	echo "Uninstall service %serviceName%"
	  %installUtilPath% /serviceName=%serviceName% /u %servicePath%
	)
) ELSE (
	echo "Service path not exist."
)