#define _CRT_SECURE_NO_WARNINGS
#include"CAPI.h"
#include"constants.h"
#include<iostream>
#include<sstream>
#include<mutex>
#include"player.h"
#include"API.h"

using namespace std;

CAPI API;
int frame = 0;
bool GameFinished = false;

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
int main()
{
	API.Initialize();
	API.ConnectServer("192.168.2.154", 30000);
	pthread_t pt;
	pthread_create(&pt, NULL, Ping, NULL);
	pthread_detach(pt);
	string message = "Connected!";
	//ËÀÑ­»·Ö´ÐÐÍæ¼Ò³ÌÐò
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

	// clientÁÄÌìÊÒ²âÊÔ
	while (API.IsConnected()) 
	{
		if (message == "quit")
		{
			API.Quit();
		}
		else API.SendChatMessage(message + " from Agent " + to_string(API.AgentId) + " Player " + to_string(API.PlayerId));
		cin >> message;
	}
	cout << "Disconnected from server.\n";
	getchar();
	return 0;
}