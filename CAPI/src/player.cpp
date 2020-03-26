#include "API.h"
#include "player.h"
#include <iostream>
#include "CAPI.h"
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::None;//指定人物天赋。选手代码必须定义此变量，否则报错
void play()
{
	Sleep(5000);
	cout << PlayerInfo.position.x << "   " << PlayerInfo.position.y << endl;
	/*  玩家在这里写代码  */
}