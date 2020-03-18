@echo off 
cd ./proto/
start generator.bat
cd ..
md build && cd build
cmake ..
pause
