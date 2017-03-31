@echo off
echo.
echo Be sure to run this command as Administrator
echo.
net stop StarchefMessageListner
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /uninstall "..\StarChef.Listener.exe"
