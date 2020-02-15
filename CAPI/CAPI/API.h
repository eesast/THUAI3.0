#ifndef API_H
#define API_H

#include"constants.h"
namespace THUAI3
{
	//void Move(Direction direction_t, int duration);
	void Put(int distance, int ThrowDish);
	void Pick();
	void Use(int type, int parameter);
	int GetPing();     // 精确到毫秒的延时
	void PauseCommunication();   // 暂停数据更新
	void ResumeCommunication();
	Player GetInfo();            
} 

#endif