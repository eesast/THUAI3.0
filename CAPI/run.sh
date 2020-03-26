cat player.cpp | grep "#define DEVELOPER_ONLY"
if [ $? -ne 0 ];then
	echo "file inclusion error" >> error.txt
	exit 1
fi
docker pull veritaria/thuai_capi_compile
docker pull veritaria/thuai_capi_run
docker run -itd --name compile veritaria/thuai_capi_compile
docker cp ./player.cpp capi_compile:/usr/local/CAPI/src/player.cpp
docker exec compile /bin/bash -c 'cd /usr/local/CAPI && mkdir build && cd build && cmake .. && make >error.txt 2>&1' && wait
if [ $? -ne 0 ];then 
	docker cp compile:/usr/local/CAPI/build/error.txt ./error.txt
	echo "compilation error"
	docker stop compile
	docker rm compile
	# errors are stored in error.txt
else
	# if no compilation error occurs, continue
	echo "compilation success"
	docker cp compile:/usr/local/CAPI/build/AI ./
	docker cp compile:/usr/local/CAPI/build/proto/lib/libprotos.so ./
	cp ./AI /home/backup/name  # save executable file
	docker run -itd --name run veritaria/thuai_capi_run 
	docker cp ./AI run:/usr/local/
	docker cp ./libprotos.so run:/usr/lib/
	docker exec run /bin/bash -c '/usr/local/CAPI/build/AI 127.0.0.1 8888'
	wait
	docker stop compile
	docker rm compile
	docker stop run
	docker rm run
fi
