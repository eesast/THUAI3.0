#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//指定人物天赋。选手代码必须定义此变量，否则报错
void play()
{
	char c;
	cin.clear();
	cin.ignore();
	cin >> c;
	switch (c)
	{
	case 'd':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::Right, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'e':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::RightUp, moveDistance / PlayerInfo.moveSpeed * 1000);
	}
	break;
	case 'w':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::Up, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'q':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::LeftUp, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'a':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::Left, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'z':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::LeftDown, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'x':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::Down, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'c':
	{
		int moveDistance = 0;
		std::cout << endl << "Please Input your move distance" << endl;
		cin >> moveDistance;
		move(Protobuf::Direction::RightDown, moveDistance / PlayerInfo.moveSpeed * 1000);
	}break;
	case 'f':
	{
		std::cout << endl << "Please Input 3 parameters : isSelfPosition, pickType, dishOrToolType" << endl;
		bool isSelfPosition = 0;
		cin >> isSelfPosition;
		int pickType = 0;
		cin >> pickType;
		int dishOrToolType = 0;
		cin >> dishOrToolType;
		pick(isSelfPosition, (ObjType)pickType, dishOrToolType);
	}
	break;
	case 'u':
	{
		std::cout << endl << "Please Input 2 parameters : " << endl;
		double param1 = 0;
		cin >> param1;
		double param2 = 0;
		cin >> param2;
		use(1, param1, param2);
	}
	break;
	case 'i': use(0); break;
	case 'r':
	{
		std::cout << endl << "Please Input 2 parameters : " << endl;
		double distance = 0;
		cin >> distance;
		double angle = 0;
		cin >> angle;
		put(distance, angle, true);
		_sleep(26);
		move(Left, 1000);
	}
	break;
	case 't':
	{
		std::cout << endl << "Please Input 2 parameters : " << endl;
		double distance = 0;
		cin >> distance;
		double angle = 0;
		cin >> angle;
		put(distance, angle, false);
	}
	break;
	case ':':
	{
		std::cout << endl << "Please Input your text to speak : " << endl;
		string str;
		cin >> str;
		speakToFriend(str);
	}
	break;
	case 'm':
	{
		std::cout << endl << "Input two interger to print a map cell :" << endl;
		int x, y;
		cin.clear();
		cin.ignore();
		cin >> x >> y;
		list<Obj> l = MapInfo::get_mapcell(PlayerInfo.position.x, PlayerInfo.position.y);
		if (l.empty()) cout << "empty" << endl;
		std::cout << "objs in map[" << PlayerInfo.position.x << "][" << PlayerInfo.position.y << "] :" << endl;
		for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
		{
			std::cout << "\tblocktype : " << i->objType << endl;
		}
	}
	break;
	case 's':
	{
		std::cout << endl << "Self info :" << endl;
		std::cout << "\tposition : " << PlayerInfo.position.x << "," << PlayerInfo.position.y << endl;
		std::cout << "\tdish : " << PlayerInfo.dish << endl;
		std::cout << "\ttool : " << PlayerInfo.tool << endl;
		std::cout << "\trecieveText : " << PlayerInfo.recieveText << endl;
	}

	default:
		break;
	}


	std::cout << "Game Time : " << THUAI3::getGameTime() << endl;
	/*  玩家在这里写代码  */
}