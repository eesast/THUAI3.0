#ifndef API_H
#define API_H

#include"Constant.h"
#include"structures.h"
#include <string>
namespace THUAI3
{
	void Move(Direction direction_t, int duration=1000);
	void Put(double distance, bool isThrowDish);
	void Pick();
	void Use(int type, int parameter);
	void SpeakToFriend(std::string speakText);
	int GetPing();     // ��ȷ���������ʱ
	void PauseCommunication();   // ��ͣ���ݸ���
	void ResumeCommunication();
	Player GetInfo();            
} 

#endif