#define DEVELOPER_ONLY
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include "API.h"
#include "CAPI.h"
#include <sys/timeb.h>

extern CAPI API;

unsigned long long THUAI3::initGameTime;

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

bool THUAI3::move(Direction direction_t, int duration)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Move);
	mesC2S.set_movedirection(direction_t);
	mesC2S.set_moveduration(duration);
	return API.SendCommandMessage(mesC2S);
}

bool THUAI3::put(double distance, double angle, bool isThrowDish)
{
	MessageToServer mesC2S;
	mesC2S.set_throwdistance(distance);
	mesC2S.set_throwangle(angle);
	mesC2S.set_isthrowdish(isThrowDish);
	mesC2S.set_commandtype(CommandType::Put);
	return API.SendCommandMessage(mesC2S);
}

bool THUAI3::use(int type, double parameter1, double parameter2)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Use);
	mesC2S.set_parameter1(parameter1);
	mesC2S.set_parameter2(parameter2);
	mesC2S.set_usetype(type);
	return API.SendCommandMessage(mesC2S);
}

bool THUAI3::pick(bool isSelfPosition, ObjType pickType, int dishOrToolType)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Pick);
	mesC2S.set_ispickselfposition(isSelfPosition);
	mesC2S.set_picktype(pickType);
	mesC2S.set_pickdishortooltype(dishOrToolType);
	return API.SendCommandMessage(mesC2S);
}

bool THUAI3::speakToFriend(string speakText)
{
	MessageToServer mesC2S;
	mesC2S.set_commandtype(CommandType::Speak);
	if (speakText.length() > 16)
		speakText = speakText.substr(0, 15);
	mesC2S.set_speaktext(speakText);
	return API.SendCommandMessage(mesC2S);
}

void THUAI3::initializeGameTime()
{
	initGameTime = getSystemTime();
}

unsigned long long THUAI3::getGameTime()
{
	return getSystemTime() - initGameTime;
}

void THUAI3::wait()
{
	API.sema.wait();
}