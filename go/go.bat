@ECHO OFF

IF NOT '%1' == '-a' GOTO _REMOVE
go2.exe -a %2 %3
goto EXIT

:_REMOVE
IF NOT '%1' == '-r' GOTO _LIST
go2.exe -r %2
goto EXIT

:_LIST
IF NOT '%1' == '-l' GOTO _JUMP
go2.exe -l
goto EXIT

:_JUMP
IF NOT '%2' == '' goto EXIT
for /f %%i in ('go2.exe %1') do set MOVE_PATH=%%i
pushd .
cd /D %MOVE_PATH%
goto EXIT

:EXIT