#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//指定人物天赋。选手代码必须定义此变量，否则报错
double CountDistance(double x1, double x2, double y1, double y2);
void moveToPosition(int x, int y);
void CrossBarrier(int index);
void FacePosition(double x, double y);
int countAngle(double x1, double x2, double y1, double y2);
void pick_dish(int location);
bool CheckTasklist();

void play()
{
	string state = "SearchFood";
	while (true)
	{
		if (state == "SearchFood") {
            while (PlayerInfo.dish == 0) {
                pick_dish(1);
                if (PlayerInfo.dish == 0) {
                    pick_dish(2);
                }
                if (PlayerInfo.dish == 0) {
                    pick_dish(3);
                }
            }
			/*moveToPosition(8, 42);
			pick(false, Block, 0);
			while (PlayerInfo.dish == 0) {
				Sleep(1000);
				pick(false, Block, 0);
			}*/
			state == "CookFood";
		}

		if (state == "CookFood") {
			moveToPosition(9, 25);
			FacePosition(9, 25);
			use(0, 0, 0);
			pick(false, Block, 0);
			while (PlayerInfo.dish == 0) {
				Sleep(1000);
				pick(false, Block, 0);
			}
			state = "SubmitFood";
		}

		if (state == "SubmitFood")
		{
			moveToPosition(24, 25);
			FacePosition(25, 25);

			while (CheckTasklist() == false)
			{
				Sleep(1000);
			}
			use(0, 0, 0);
			state == "SearchFood";
		}
	}
	
	
	std::cout << "(" << PlayerInfo.position.x << "," << PlayerInfo.position.y << ")" << endl;
}

double CountDistance(double x1, double x2, double y1, double y2) {
	return (x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2);
}

void moveToPosition(int x, int y) {
	double t1;
	int judge = 0;
	
	while (CountDistance(PlayerInfo.position.x, x, PlayerInfo.position.y, y) > 2) {
		judge = 0;
		if (abs(PlayerInfo.position.x - x) > 1) {
			if (PlayerInfo.position.x > x)
			{
				t1 = PlayerInfo.position.x;
				move(Protobuf::Direction::Left, 100);				//向左移动一格
				Sleep(100);
				if (PlayerInfo.position.x == t1)
				{
					judge = 1;
				}
			}
			else {
				t1 = PlayerInfo.position.x;
				move(Protobuf::Direction::Right, 100);
				Sleep(100);
				if (PlayerInfo.position.x == t1)
				{
					judge = 1;
				}
			}
		}
		else if (abs(PlayerInfo.position.y - y) > 1) {
			if (PlayerInfo.position.y > y)
			{
				t1 = PlayerInfo.position.y;
				move(Protobuf::Direction::Down, 100);
				Sleep(100);
				if (PlayerInfo.position.y == t1)
				{
					judge = 2;
				}
			}
			else {
				t1 = PlayerInfo.position.y;
				move(Protobuf::Direction::Up, 100);
				Sleep(100);
				if (PlayerInfo.position.y == t1)
				{
					judge = 2;
				}
			}
		}


		
		if (judge == 1)
		{
			CrossBarrier(1);
		}
		else if (judge == 2) {
			CrossBarrier(2);
		}
		std::cout << "(" << PlayerInfo.position.x << "," << PlayerInfo.position.y << ")" << judge << endl;

	}
}


void CrossBarrier(int index) {
	if (index == 1) {
		move(Protobuf::Direction::Up, 500);
		Sleep(500);
		move(Protobuf::Direction::Right, 500);
		Sleep(500);
		std::cout << "1" << endl;
	}
	else {
		move(Protobuf::Direction::Right, 500);
		Sleep(500);

		move(Protobuf::Direction::Up, 500);
		Sleep(500);

		std::cout << "2" << endl;

	}
}

void FacePosition(double x, double y) {
	int angle = countAngle(PlayerInfo.position.x, x, PlayerInfo.position.y, y);
	switch (angle) {
		case 1: move(Protobuf::Direction::Up, 0);
		case 2:	move(Protobuf::Direction::Down, 0);
		case 3:	move(Protobuf::Direction::Left, 0);
		case 4:	move(Protobuf::Direction::Right, 0);
		default: move(Protobuf::Direction::Down, 0);
	}
		
}

int countAngle(double x1, double x2, double y1, double y2) {
	if ((y2 - y1 > x2 - x1) && (y2 - y1 > -(x2 - x1)))
	{
		return 1;
	}
	else if ((y2 - y1 < x2 - x1) && (y2 - y1 < -(x2 - x1))) {
		return 2;
	}
	else if ((y2 - y1 > x2 - x1) && (y2 - y1 < -(x2 - x1))) {
		return 3;
	}
	else if ((y2 - y1 < x2 - x1) && (y2 - y1 > -(x2 - x1))) {
		return 4;
	}
}

bool CheckTasklist() {
	for (auto i = task_list.begin(); i != task_list.end(); i++) {
		if (PlayerInfo.dish == *i)
		{
			return true;
		}
	}
	return false;
}


void pick_dish(int location)
{
    int x_t = 26;
    int y_t = 6;
    if (location == 1)
    {
        x_t = 26;
        y_t = 6;
    }
    else if (location == 2)
    {
        x_t = 8;
        y_t = 42;
    }
    else if (location == 3)
    {
        x_t = 43;
        y_t = 42;
    }

    moveToPosition(x_t, y_t);
    FacePosition(x_t, y_t);

    if (abs(PlayerInfo.position.x - x_t) < 1 && abs(PlayerInfo.position.y - y_t) < 1)
    {
        pick(false, Block, 0);
    }
}
