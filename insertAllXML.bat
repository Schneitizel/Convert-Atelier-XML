@echo off
setlocal enabledelayedexpansion
color a

for /r "XML" %%a in (*.txt) do (	
	set "filename=%%~na"
	if not "!filename:~-3!"=="NEW" "%~dp0\ConvertXML.exe" "%%a"
)

pause
exit