@echo off 
cd ../dependency
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\generateProto.ps1'"
cd ../CAPI
md build && cd build
cmake ..
pause
