#include"API.h"
#include"CAPI.h"

extern CAPI API;

int THUAI3::GetPing()
{
	return API.Ping;
}

void THUAI3::PauseCommunication()
{
	API.PauseUpdate = true;
}

Player THUAI3::GetInfo()
{

	return API.GetInfo();
}


