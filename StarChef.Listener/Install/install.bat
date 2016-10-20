@echo off
echo.
echo Be sure to run this command as Administrator
echo First argument should be service name.
echo Second argument should be service location.
echo.

set serviceName="StarchefMessageListner"
set servicePath=%1
set installUtilPath="C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil"

echo Try to install service "%serviceName%"
%installUtilPath% /serviceName=%serviceName% %servicePath%

for /F "tokens=3 delims=: " %%H in ('sc query %serviceName%^| findstr "STATE"') do (
  if /I "%%H" NEQ "RUNNING" (
	echo Service "%serviceName%" found.
	net start %serviceName%
  )
)