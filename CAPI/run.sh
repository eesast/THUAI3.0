docker run -itd --name compile capi_compile
docker cp ./player.cpp capi_compile:/usr/local/CAPI/src/player.cpp
docker exec compile /bin/bash -c 'cd /usr/local/CAPI && mkdir build && cd build && cmake .. && make >error.txt 2>&1'
wait
docker cp compile:/usr/local/CAPI/build/error.txt ./error.txt

# if no compilation error occurs, continue
docker cp compile:/usr/local/CAPI/. ./COMPILED_CAPI
docker run -itd --name run capi_run 
docker cp ./COMPILED_CAPI/. run:/usr/local/CAPI
docker exec run /bin/bash -c '/usr/local/CAPI/build/AI 127.0.0.1 8888'
wait
docker stop compile
docker rm compile
docker stop run
docker rm run
