#ifndef NOMINMAX                      
#define NOMINMAX
#endif
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


void THUAI3::Move(Direction direction_t, int duration)
{
    MessageToServer mesC2S;
    mesC2S.set_commandtype(CommandTypeMessage::Move);
    mesC2S.set_moveduration(duration);
    API.SendCommandMessage(&mesC2S);
}

void THUAI3::Put(double distance, bool isThrowDish)
{
    MessageToServer mesC2S;
    mesC2S.set_throwdistance(distance);
    mesC2S.set_isthrowdish (isThrowDish);
    mesC2S.set_commandtype (CommandTypeMessage::Put);
    API.SendCommandMessage(&mesC2S);
}

void THUAI3::Use(int type, int parameter)
{
    MessageToServer mesC2S;
    mesC2S.set_commandtype (CommandTypeMessage::Use);
    mesC2S.set_usetype (type);
    API.SendCommandMessage(&mesC2S);
}

void THUAI3::Pick()
{
    MessageToServer mesC2S;
    mesC2S.set_commandtype (CommandTypeMessage::Pick);
    API.SendCommandMessage(&mesC2S);
}

void THUAI3::SpeakToFriend(string speakText)
{
    MessageToServer mesC2S;
    mesC2S.set_commandtype (CommandTypeMessage::Speak);
    if (speakText.length() > 16)//���Ʒ��͵��ַ�������Ϊ16
        speakText = speakText.substr(0, 15);
    mesC2S.set_speaktext(speakText);
    API.SendCommandMessage(&mesC2S);
}