@echo off
dir /b/s | find "冲突时的文件备份" > temp.dir

for /F "tokens=*" %%A in (temp.dir) do (
del /q "%%A"
)
del /q temp.dir