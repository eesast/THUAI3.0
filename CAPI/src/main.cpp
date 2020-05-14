#pragma warning(disable:4711)
#pragma warning(disable:4710)
#pragma warning(disable:5027)
#pragma warning(disable:4623)
#pragma warning(disable:4626)
#pragma warning(disable:4668)
#pragma warning(disable:4100)
#pragma warning(disable:26495)
#pragma warning(disable:26812)

#include "Constant.h"
#include <iostream>
#include <sstream>
#include <mutex>
#include "player.h"
#include "API.h"
#include "OS_related.h"
#include "Sema.h"

using namespace std;

CAPI API;
bool GameRunning = false;

void* Ping(void* param)
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
int main(int argc, char* argv[])
{
	MapInfo::initialize_map();

	char* agent_ip = (argv[1]);
	int agent_port = atoi(argv[2]);
	API.Initialize();
	API.ConnectServer(agent_ip, agent_port);
	// pthread_t pt;
	// pthread_create(&pt, NULL, Ping, NULL);
	// pthread_detach(pt);
	string message = "Connected!";

	DebugFunc = DebugSilently;

	while (!GameRunning)
	{
		API.start_game_sema.wait();
	}
	THUAI3::initializeGameTime();
	while (GameRunning)
	{
		THUAI3::wait();
		play();
	}
	getchar();
	API.Quit();
	cout << "Disconnected from server.\n";
	getchar();
	return 0;
}
