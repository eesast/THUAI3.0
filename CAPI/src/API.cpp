#define DEVELOPER_ONLY
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include "API.h"
#include "CAPI.h"

extern CAPI API;

int THUAI3::GetPing()
{
	return API.Ping;
}

void THUAI3::PauseCommunication()
{
	API.PauseUpdate = true;
}

Constant::Player THUAI3::GetInfo()
{
	return API.GetInfo();
}

void THUAI3::move(Direction direction_t, int duration)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Move);
	mesC2S.set_movedirection(direction_t);
	mesC2S.set_moveduration(duration);
	API.SendCommandMessage(&mesC2S);
}

void THUAI3::put(double distance, bool isThrowDish)
{
	MessageToServer mesC2S;
	mesC2S.set_throwdistance(distance);
	mesC2S.set_isthrowdish(isThrowDish);
	mesC2S.set_commandtype(CommandType::Put);
	API.SendCommandMessage(&mesC2S);
}

void THUAI3::use(int type, double parameter1, double parameter2)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Use);
	mesC2S.set_parameter1(parameter1);
	mesC2S.set_parameter2(parameter2);
	mesC2S.set_usetype(type);
	API.SendCommandMessage(&mesC2S);
}

void THUAI3::pick()
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Pick);
	API.SendCommandMessage(&mesC2S);
}

void THUAI3::speakToFriend(string speakText)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Speak);
	if (speakText.length() > 16)
		speakText = speakText.substr(0, 15);
	mesC2S.set_speaktext(speakText);
	API.SendCommandMessage(&mesC2S);
}