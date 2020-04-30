#ifndef API_H
#define API_H

#include "Constant.h"
#include "structures.h"
#include <string>
namespace THUAI3
{
	extern unsigned long long initGameTime;
	bool move(Direction direction_t, int duration = 1000);
	bool put(double distance, double angle, bool isThrowDish);
	bool pick(bool isSelfPosition, ObjType pickType, int dishOrToolType);
	bool use(int type, double parameter1 = 0, double parameter2 = 0);
	bool speakToFriend(std::string speakText);
	void initializeGameTime();
	unsigned long long getGameTime();
	int GetPing();			   // 精确到毫秒的延时
	void PauseCommunication(); // 暂停数据更新
	void ResumeCommunication();
	Constant::Player GetInfo();
	void wait();
} // namespace THUAI3

#endif