#!/bin/bash
cd /usr/local/CAPI
cat src/player.cpp | grep "#define DEVELOPER_ONLY"
if [ $? -ne 1 ];then
	echo "file inclusion error" >> error.txt
	exit 1
fi
mkdir build
cd build
cmake ..
make >error.txt 2>&1
