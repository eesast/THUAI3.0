#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
#include <thread>
#include <chrono>
#include <cmath>

using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//指定人物天赋。选手代码必须定义此变量，否则报错

void MoveTo(float x, float y) {
	if (x >= PlayerInfo.position.x && y >= PlayerInfo.position.y) {	//食物产生点在右上
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Right, 200 * (x - PlayerInfo.position.x));
			Sleep(200 * (x - PlayerInfo.position.x) + 1);
			move(Up, 200 * (y - PlayerInfo.position.y));
			Sleep(200 * (y - PlayerInfo.position.y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (x - PlayerInfo.position.x > 1 && y - PlayerInfo.position.y > 1) { //被堵住
			move(Left, 400);
			Sleep(401);
			move(Up, 200);
			Sleep(201);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Right, 200 * (x - PlayerInfo.position.x));
				Sleep(200 * (x - PlayerInfo.position.x) + 1);
				move(Up, 200 * (y - PlayerInfo.position.y));
				Sleep(200 * (y - PlayerInfo.position.y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		THUAI3::pick(0, Dish,0);
		move(Right, 0);
		THUAI3::pick(0, Dish, 0);
	}
	else if (x >= PlayerInfo.position.x) {	//食物产生点在右下
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Right, 200 * (x - PlayerInfo.position.x));
			Sleep(200 * (x - PlayerInfo.position.x) + 1);
			move(Down, 200 * (PlayerInfo.position.y - y));
			Sleep(200 * (PlayerInfo.position.y - y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (x - PlayerInfo.position.x > 1 && PlayerInfo.position.y - y > 1) { //被堵住
			move(Left, 400);
			Sleep(401);
			move(Down, 400);
			Sleep(401);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Right, 200 * (x - PlayerInfo.position.x));
				Sleep(200 * (x - PlayerInfo.position.x) + 1);
				move(Down, 200 * (PlayerInfo.position.y - y));
				Sleep(200 * (PlayerInfo.position.y - y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		THUAI3::pick(0, Dish, 0);
		move(Right, 0);
		THUAI3::pick(0, Dish, 0);
	}
	else if (y >= PlayerInfo.position.y) {	//食物产生点在左上
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Left, 200 * (PlayerInfo.position.x - x));
			Sleep(200 * (PlayerInfo.position.x - x) + 1);
			move(Up, 200 * (y - PlayerInfo.position.y));
			Sleep(200 * (y - PlayerInfo.position.y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (PlayerInfo.position.x - x > 1 && y - PlayerInfo.position.y > 1) { //被堵住
			move(Right, 800);
			Sleep(801);
			move(Up, 600);
			Sleep(601);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Left, 200 * (PlayerInfo.position.x - x));
				Sleep(200 * (PlayerInfo.position.x - x) + 1);
				move(Up, 200 * (y - PlayerInfo.position.y));
				Sleep(200 * (y - PlayerInfo.position.y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		THUAI3::pick(0, Dish, 0);
		move(Left, 0);
		THUAI3::pick(0, Dish, 0);
	}
	else {	//食物产生点在左下
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Left, 200 * (PlayerInfo.position.x - x));
			Sleep(200 * (PlayerInfo.position.x - x) + 1);
			move(Down, 200 * (PlayerInfo.position.y - y));
			Sleep(200 * (PlayerInfo.position.y - y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (PlayerInfo.position.x - x > 1 && PlayerInfo.position.y - y > 1) { //被堵住
			move(Right, 400);
			Sleep(801);
			move(Down, 600);
			Sleep(601);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Left, 200 * (PlayerInfo.position.x - x));
				Sleep(200 * (PlayerInfo.position.x - x) + 1);
				move(Down, 200 * (PlayerInfo.position.y - y));
				Sleep(200 * (PlayerInfo.position.y - y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		THUAI3::pick(0, Dish, 0);
		move(Left, 0);
		THUAI3::pick(0, Dish, 0);
	}
}
void MoveToFood() {
	if (PlayerInfo.position.y - PlayerInfo.position.x / 2 > 15.25 && PlayerInfo.position.x < 25) {
		MoveTo(7.5, 41.5);
		MoveTo(42.5, 40.5);
		MoveTo(25.5, 5.5);
	}
	else if (17 * PlayerInfo.position.x + 35 * PlayerInfo.position.y > 1383) {
		MoveTo(42.5, 40.5);
		MoveTo(7.5, 41.5);
		MoveTo(25.5, 5.5);
	}
	else {
		MoveTo(25.5, 5.5);
		MoveTo(42.5, 40.5);
		MoveTo(7.5, 41.5);
	}
}
void MoveTo1(float x, float y) {
	if (x >= PlayerInfo.position.x && y >= PlayerInfo.position.y) {	//食物产生点在右上
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Right, 200 * (x - PlayerInfo.position.x));
			Sleep(200 * (x - PlayerInfo.position.x) + 1);
			move(Up, 200 * (y - PlayerInfo.position.y));
			Sleep(200 * (y - PlayerInfo.position.y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (x - PlayerInfo.position.x > 1 && y - PlayerInfo.position.y > 1) { //被堵住
			move(Left, 400);
			Sleep(401);
			move(Up, 200);
			Sleep(201);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Right, 200 * (x - PlayerInfo.position.x));
				Sleep(200 * (x - PlayerInfo.position.x) + 1);
				move(Up, 200 * (y - PlayerInfo.position.y));
				Sleep(200 * (y - PlayerInfo.position.y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		use(0, 0, 0);
		move(Right, 0);
		use(0,0,0);
	}
	else if (x >= PlayerInfo.position.x) {	//食物产生点在右下
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Right, 200 * (x - PlayerInfo.position.x));
			Sleep(200 * (x - PlayerInfo.position.x) + 1);
			move(Down, 200 * (PlayerInfo.position.y - y));
			Sleep(200 * (PlayerInfo.position.y - y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (x - PlayerInfo.position.x > 1 && PlayerInfo.position.y - y > 1) { //被堵住
			move(Left, 400);
			Sleep(401);
			move(Down, 400);
			Sleep(401);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Right, 200 * (x - PlayerInfo.position.x));
				Sleep(200 * (x - PlayerInfo.position.x) + 1);
				move(Down, 200 * (PlayerInfo.position.y - y));
				Sleep(200 * (PlayerInfo.position.y - y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		use(0, 0, 0);
		move(Right, 0);
		use(0, 0, 0);
	}
	else if (y >= PlayerInfo.position.y) {	//食物产生点在左上
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Left, 200 * (PlayerInfo.position.x - x));
			Sleep(200 * (PlayerInfo.position.x - x) + 1);
			move(Up, 200 * (y - PlayerInfo.position.y));
			Sleep(200 * (y - PlayerInfo.position.y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (PlayerInfo.position.x - x > 1 && y - PlayerInfo.position.y > 1) { //被堵住
			move(Right, 800);
			Sleep(801);
			move(Up, 600);
			Sleep(601);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Left, 200 * (PlayerInfo.position.x - x));
				Sleep(200 * (PlayerInfo.position.x - x) + 1);
				move(Up, 200 * (y - PlayerInfo.position.y));
				Sleep(200 * (y - PlayerInfo.position.y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		use(0, 0, 0);
		move(Left, 0);
		use(0, 0, 0);
	}
	else {	//食物产生点在左下
		while (1) {
			int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
			move(Left, 200 * (PlayerInfo.position.x - x));
			Sleep(200 * (PlayerInfo.position.x - x) + 1);
			move(Down, 200 * (PlayerInfo.position.y - y));
			Sleep(200 * (PlayerInfo.position.y - y) + 1);
			if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
		}
		while (PlayerInfo.position.x - x > 1 && PlayerInfo.position.y - y > 1) { //被堵住
			move(Right, 400);
			Sleep(801);
			move(Down, 600);
			Sleep(601);
			while (1) {
				int x0 = PlayerInfo.position.x, y0 = PlayerInfo.position.y;
				move(Left, 200 * (PlayerInfo.position.x - x));
				Sleep(200 * (PlayerInfo.position.x - x) + 1);
				move(Down, 200 * (PlayerInfo.position.y - y));
				Sleep(200 * (PlayerInfo.position.y - y) + 1);
				if (x0 == PlayerInfo.position.x && y0 == PlayerInfo.position.y) break;	//直到移不动
			}
		}
		use(0, 0, 0);
		move(Left, 0);
		use(0, 0, 0);
	}
}
void MoveToCook() {
		MoveTo1(33.5, 18.5);
		MoveTo1(41.5, 28.5);
		MoveTo1(25.5, 36.5);
		MoveTo1(8.5, 24.5);
}
void MoveToTask() {
	while (PlayerInfo.position.x > 26.5 || PlayerInfo.position.x < 23.5 || PlayerInfo.position.y>26.5 || PlayerInfo.position.y < 23.5) {	//若不在任务点附近
		int flaghorizontal = 0;
		int flagvertical = 0;
		if (PlayerInfo.position.x > 26.5)
			flaghorizontal = 1;
		if (PlayerInfo.position.y > 26.5)
			flagvertical = 1;
		int fuzzyposition = 0;
		fuzzyposition = 10 * flaghorizontal + flagvertical;		//大致所在位置

		double nowpositionx = PlayerInfo.position.x;
		double nowpositiony = PlayerInfo.position.y;
		Direction tempmovedirection;
		switch (fuzzyposition) {
		case(0):tempmovedirection = RightUp; break;
		case(1):tempmovedirection = RightDown; break;
		case(10):tempmovedirection = LeftUp; break;
		case(11):tempmovedirection = LeftDown; break;
		}
		move(tempmovedirection, 100);	//斜着向中心运动

		if (abs(nowpositionx - PlayerInfo.position.x) < 1e-6) {	//若被挡住则竖直后退
			if (flagvertical == 1)
				THUAI3::move(Up, 100);
			else
				THUAI3::move(Down, 100);
		}
	}
	use(0, 0, 0);	//提交菜品
}

void DecideToDo() {
	int a, b = 0;
	for (auto it = task_list.begin(); it != task_list.end(); it++) {
		a += 1;
		if ((PlayerInfo.dish >= 1 && PlayerInfo.dish <= 25) && PlayerInfo.dish == *it) {
			MoveToCook();
			break;
		}
		else if (PlayerInfo.dish >= 26 && PlayerInfo.dish <= 50 && PlayerInfo.dish == *it) {
			MoveToTask();
			break;
		}
		else if (PlayerInfo.dish == 0) {
			MoveToFood();
			break;
		}
		else b += 1;
	}
	if (a == b) put(100,3.14, 1);
}


void play()
{
	char c;
	cin.clear();
	cin.ignore();
	cin >> c;
	switch (c)
	{
	case 'd':move(Protobuf::Direction::Right, 1000); break;
	case 'e':move(Protobuf::Direction::RightUp, 1000); break;
	case 'w':move(Protobuf::Direction::Up, 1000); break;
	case 'q':move(Protobuf::Direction::LeftUp, 1000); break;
	case 'a':move(Protobuf::Direction::Left, 1000); break;
	case 'z':move(Protobuf::Direction::LeftDown, 1000); break;
	case 'x':move(Protobuf::Direction::Down, 1000); break;
	case 'c':move(Protobuf::Direction::RightDown, 1000); break;

	default:
		break;
	}
	cout << "Input two interger to print a map cell :" << endl;
	int x, y;
	cin.clear();
	cin.ignore();
	cin >> x >> y;
	list<Obj> l = MapInfo::get_mapcell(x, y);
	cout << "objs in map[" << x << "][" << y << "] :" << endl;
	for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
	{
		cout << "\tblocktype : " << i->blockType << endl;
	}

	Sleep(1000);
	DecideToDo();
	MoveToFood();
	MoveToCook();
	MoveToTask();
}