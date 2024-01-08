@echo off
setlocal enabledelayedexpansion
color a

for /r "XML" %%a in (*.xml) do (	
	set "filename=%%~na"
	if not "!filename:~-3!"=="NEW" "%~dp0\ConvertXML.exe" "%%a"
)

pause
exit