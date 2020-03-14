#include "CAPI.h"
#include "Constant.h"
#include <iostream>
#include <sstream>
#include <mutex>
#include "player.h"
#include "API.h"

using namespace std;

CAPI API;
int frame = 0;
bool GameFinished = false;

void *Ping(void *param)
{
	while (API.IsConnected())
	{
		if (!API.PrintBuffer())
		{
			API.Refresh();
			//printf("Ping : %f ms\n", API.Ping);
			Sleep(200);
		}
	}
	return NULL;
}
int main(int argc, char *argv[])
{
	char *agent_ip = (argv[1]);
	int agent_port = atoi(argv[2]);
	API.Initialize();
	API.ConnectServer(agent_ip, agent_port);
	pthread_t pt;
	pthread_create(&pt, NULL, Ping, NULL);
	pthread_detach(pt);
	string message = "Connected!";
	//��ѭ��ִ����ҳ���
	/*
	while (!GameFinished) 
	{
		int now_frame = frame;
		play();
		while (frame == now_frame);
	}
	getchar();
	API.Quit();
	*/

	// client�����Ҳ���
	while (API.IsConnected())
	{
		if (message == "quit")
		{
			API.Quit();
		}
		else
			API.SendChatMessage(message + " from Agent " + to_string(API.AgentId) + " Player " + to_string(API.PlayerId));
		cin >> message;
	}
	cout << "Disconnected from server.\n";
	getchar();
	return 0;
}
