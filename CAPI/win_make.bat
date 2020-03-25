@echo off 
cd ../dependency
start generateProto.ps1
cd ../CAPI
md build && cd build
cmake ..
pause
