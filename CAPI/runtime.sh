#!/bin/bash

# args server | playercount | debuglevel | timelimit | token
if [[ $# -le 3 ]]; then
	echo "parameters: {server} {playercount} {debuglevel} {timelimit} [Optional:token]"
	exit 0
fi

echo "server: $1"
echo "playercount: $2"
echo "debuglevel: $3"
echo "timelimit: $4"
if [[ $# -eq 5 ]]; then
    echo "token: $5"
    token="--token $5"
else
    echo "no token"
    token=''
fi

dotnet app/Communication.Agent.dll --server $1 --port 30000 --debugLevel $3 --playercount $2 --timelimit $4 ${token} >agent.log &
for i in `seq 1 $2`
do
    /usr/local/mnt/AI${i} 127.0.0.1 30000 >/dev/null &
done
pid=`ps -ef |grep Agent|grep -v grep|awk 'NR==1{print}'|awk '{print $2}'`
while ps -p $pid >/dev/null
do 
    sleep 1
done
exit 0
