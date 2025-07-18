@echo off
setlocal

set "currentDir=%~dp0"

if exist "%currentDir%cn.etetet.yiuiconditionconfig" (
    rmdir /s /q "%currentDir%cn.etetet.yiuiconditionconfig"
)

set "targetDir=%currentDir%..\..\cn.etetet.yiuiconditionconfig"

if exist "%targetDir%" (
    xcopy "%targetDir%" "%currentDir%cn.etetet.yiuiconditionconfig" /e /i /y
)

if exist "%currentDir%cn.etetet.yiuiconditionconfig\*.meta" (
    del /s /q "%currentDir%cn.etetet.yiuiconditionconfig\*.meta"
)

echo 操作完成
pause