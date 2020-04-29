#!/bin/bash
cd /usr/local/CAPI
cp /usr/local/mnt/player.cpp ./src/player.cpp
cat src/player.cpp | grep "#define DEVELOPER_ONLY"
if [ $? -ne 1 ];then
	echo "file inclusion error" >> error.txt
	exit 1
fi
mkdir build
cd build
cmake ..
make >error.txt 2>&1
cp AI /usr/local/mnt/AI${THUAI_CODEROLE}
if [ $? -ne 0 ]
then
	# compile fail
	cp error.txt /usr/local/mnt/error.txt
	curl -X PUT -d '{"compileInfo":"compile error"}' -H 'Content-Type: application/json' -H "Authorization: Bearer ${THUAI_COMPILE_TOKEN}" https://api.eesast.com/v1/codes/${THUAI_CODEID}/compile
else
	curl -X PUT -d '{"compileInfo":"compile success"}' -H 'Content-Type: application/json' -H "Authorization: Bearer ${THUAI_COMPILE_TOKEN}" https://api.eesast.com/v1/codes/${THUAI_CODEID}/compile
fi
exit 0
