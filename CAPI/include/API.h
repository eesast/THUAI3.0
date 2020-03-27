#ifndef API_H
#define API_H
#include "Constant.h"
#include "structures.h"
#include <string>
namespace THUAI3
{
void Move(Direction direction_t, int duration = 1000);
void Put(double distance, bool isThrowDish);
void Pick();
void Use(int type, int parameter);
void SpeakToFriend(std::string speakText);
int GetPing();			   // 精确到毫秒的延时
void PauseCommunication(); // 暂停数据更新
void ResumeCommunication();
Player GetInfo();
} // namespace THUAI3

#endif