#!/bin/bash
dotnet app/Communication.Agent.dll --server 127.0.0.1:20000 --port 30000 -d 0 --playercount 1 --timelimit 100
/usr/local/AI 127.0.0.1 8888 >> debug.txt 2>&1