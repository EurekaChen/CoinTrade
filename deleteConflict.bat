@echo off
dir /b/s | find "��ͻʱ���ļ�����" > temp.dir

for /F "tokens=*" %%A in (temp.dir) do (
del /q "%%A"
)
del /q temp.dir