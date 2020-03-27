cd /usr/local/CAPI 
cat player.cpp | grep "#define DEVELOPER_ONLY"
if [ $? -ne 0 ];then
	echo "file inclusion error" >> error.txt
	exit 1
fi
mkdir build && cd build && cmake .. && make >error.txt 2>&1
