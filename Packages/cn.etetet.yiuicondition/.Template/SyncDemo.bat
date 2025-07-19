@echo off
setlocal

set "currentDir=%~dp0"

if exist "%currentDir%cn.etetet.yiuiconditiondemo" (
    rmdir /s /q "%currentDir%cn.etetet.yiuiconditiondemo"
)

set "targetDir=%currentDir%..\..\cn.etetet.yiuiconditiondemo"

if exist "%targetDir%" (
    xcopy "%targetDir%" "%currentDir%cn.etetet.yiuiconditiondemo" /e /i /y
)

if exist "%currentDir%cn.etetet.yiuiconditiondemo\*.meta" (
    del /s /q "%currentDir%cn.etetet.yiuiconditiondemo\*.meta"
)

echo 操作完成
pause