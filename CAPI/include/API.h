#ifndef API_H
#define API_H

#include "Constant.h"
#include "structures.h"
#include <string>
namespace THUAI3
{
	void move(Direction direction_t, int duration = 1000);
	void put(double distance, bool isThrowDish);
	void pick();
	void use(int type, double parameter1, double parameter2);
	void speakToFriend(std::string speakText);
	int GetPing();			   // 精确到毫秒的延时
	void PauseCommunication(); // 暂停数据更新
	void ResumeCommunication();
	Constant::Player GetInfo();
} // namespace THUAI3

#endif