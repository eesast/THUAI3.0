#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//指定人物天赋。选手代码必须定义此变量，否则报错

static int lzrtime = 0;//转圈参数
static int lzrhand = 0;//是否提交
static int tasktime = -90000;//任务时间
static int mypicktime;//mypick计时器
static int lzrmypick=0;//是否mypick
static DishType mytask;//现在任务
static int lzrtask = 0;//获取任务参数
static DishType task[20] = { DishEmpty };//任务表
static int aim_x=0, aim_y=0;//目标位置（暂时没用到）
static list<Obj>::iterator obj[3][3];//周围物体
static DishType needs[10] = { DishEmpty };//所需食材
static DishType middle[10] = { DishEmpty };//所需中间产物（被废弃了）
static DishType foods[10] = { DishEmpty };//放进灶台的食物
static int lzrclear = 0;//需要清空灶台
static int last_time = -1000;
static int last_x = 0, last_y = 0, now_x = 1, now_y = 1;

void avoid();

//获取任务
void gettask()
{
	int i = 0;
	list<DishType>::iterator ite;
	for (ite = task_list.begin(); ite != task_list.end(); ite++)
	{
		task[i++] = *ite;
	}
	if (lzrtask == 0)
	{
		for (int i = 9; i >= 0; i--)
		{
			if (task[i] != DishEmpty && task[i] != TomatoFriedEggNoodle && task[i] != BeefNoodle && task[i] != FrenchFries && task[i] != Hamburger &&  task[i] != OverRice && task[i] < 41 && task[i] != Bread && task[i] != Noodle && task[i] != SpicedPot)
			{
				mytask = task[i];
				lzrmypick = 3;
				for (int i = 0; i < 10; i++)
				{
					needs[i] = DishEmpty;
					tasktime = getGameTime();
					lzrclear = 1;
				}
				break;
			}
		}
		lzrtask = 1;
	}
	else if (lzrtask == 1)
	{
		bool lzr = 0;
		for (int i = 0; i < 10; i++)
		{
			if (task[i] == mytask)
			{
				lzr = 1;
			}
		}
		if (lzr == 0 || mytask == DishEmpty)
		{
			lzrtask = 0;
		}
	}
}

//获取地形
list<Obj>::iterator location(int x, int y)
{
	list<Obj> obj;
	obj = MapInfo::get_mapcell(x, y);
	list<Obj>::iterator ite = obj.begin();
	return ite;
}

//获取周围地形
void getobj()
{
	obj[0][0] = location(PlayerInfo.position.x - 1, PlayerInfo.position.y + 1);
	obj[0][1] = location(PlayerInfo.position.x, PlayerInfo.position.y + 1);
	obj[0][2] = location(PlayerInfo.position.x + 1, PlayerInfo.position.y + 1);
	obj[1][0] = location(PlayerInfo.position.x - 1, PlayerInfo.position.y);
	obj[1][1] = location(PlayerInfo.position.x, PlayerInfo.position.y);
	obj[1][2] = location(PlayerInfo.position.x + 1, PlayerInfo.position.y);
	obj[2][0] = location(PlayerInfo.position.x - 1, PlayerInfo.position.y - 1);
	obj[2][1] = location(PlayerInfo.position.x, PlayerInfo.position.y - 1);
	obj[2][1] = location(PlayerInfo.position.x + 1, PlayerInfo.position.y - 1);
}

//编辑所需食材表
void writeneed(DishType a,DishType b,DishType c)
{
	int j = 0;
	for (int i = 0; i < 10; i++)
	{
		switch (j)
		{
		case 0:
			needs[i] = a;
			break;
		case 1:
			needs[i] = b;
			break;
		case 2:
			needs[i] = c;
			break;
		}
		j++;
	}
}

//编辑所说中间产物
void writemiddle(DishType a, DishType b)
{
	int j = 0;
	for (int i = 0; i < 10; i++)
	{
		if (middle[i] == DishEmpty)
		{
			switch (j)
			{
			case 0:
				middle[i] = a;
				break;
			case 1:
				middle[i] = b;
				break;
			}
			j++;
		}
	}
}

//将中间产物所需食材加入食材表（暂时还没考虑）
void middle_need()
{
	return;
}

//获取需要的食材
void getneed()
{
	switch (mytask)
	{
	case TomatoFriedEgg:
		writeneed(Protobuf::Tomato, Protobuf::Egg,DishEmpty);
		break;
	case OverRice:
		writeneed(Protobuf::Pork, Protobuf::Pork, Protobuf::Rice);
		break;
	case Barbecue:
		writeneed(Protobuf::Lettuce, Protobuf::Pork, DishEmpty);
		break;
	case CookedRice:
		writeneed(Protobuf::Rice, DishEmpty, DishEmpty);
	case Flour:
		writeneed(Protobuf::Wheat, DishEmpty, DishEmpty);
	case Ketchup:
		writeneed(Protobuf::Tomato, DishEmpty, DishEmpty);
	}
}

//人工智障捡东西
void mypick()
{
	/*if (PlayerInfo.dish != DishEmpty)
	{
		return;
	}
	lzrmypick--;
	mypicktime = getGameTime();
	int u;
	for (u = 0; u < 3; u++)
	{
		if (needs[u] == DishEmpty)
		{
			break;
		}
	}
	for (int i = 0; i < u; i++)
	{
		if (needs[i] == foods[0] || needs[i] == foods[1])
		{
			continue;
		}
		for (int k = 0; k < 3; k++)
		{
			pick(1, Dish, needs[i]);
			cout << "mypick" << endl;
			Sleep(50);
		}
	}*/
	/*for (int i = 0; i < 5; i++)
	{
		if (PlayerInfo.dish == DishEmpty)
		{
			for (int i = 0; needs[i] != DishEmpty; i++)
			{
				if (needs[i] == foods[0] || needs[i] == foods[1])
				{
					continue;
				}
				if (PlayerInfo.dish == DishEmpty)
				{
					for (int lzr = 0; lzr < 3; lzr++)
					{
						pick(1, Dish, needs[i]);
						Sleep(50);
					}
					cout << "mypick" << endl;
				}
				else
				{
					break;
				}
			}
		}
	}*/

}

//捡食物生成点东西
void pickfood()
{
	int lzr = 0;
	if (PlayerInfo.dish == DishEmpty)
	{
		int i = 0;
		while (PlayerInfo.dish == DishEmpty)
		{
			i++;
			pick(0, Block, 0);
			Sleep(50);
			cout << "try to pick" << endl;
			if (i > 10)
			{
				break;
			}
		}
		cout << "pick" << endl;
		cout << PlayerInfo.dish << endl;
		for (int i = 0; needs[i] != DishEmpty; i++)
		{
			if (PlayerInfo.dish == needs[i] && PlayerInfo.dish != foods[0] && PlayerInfo.dish != foods[1])
			{
				lzr = 1;
				break;
			}
		}
		if (lzr)
		{
			return;
		}
		else
		{
			while (PlayerInfo.dish != DishEmpty)
			{
				put(5, 1.57, 1);
				Sleep(50);
				cout << "try to put" << endl;
			}
			cout << "put" << endl;
			cout << PlayerInfo.dish << endl;
		}
	}
	else
		return;
}

//清空灶台
void clear()
{
	int u;
	for (u = 0; u < 3; u++)
	{
		if (foods[u] == DishEmpty)
		{
			break;
		}
	}
	int time = 0;
	while (PlayerInfo.dish != DishEmpty)
	{
		put(10, 1.57, 1);
		Sleep(50);
		cout << "try to put" << endl;
	}
	for (int t = 0; t < u; t++)
	{
		if (foods[t] == DishEmpty)
		{
			break;
		}
		for (int x = 0; PlayerInfo.dish == DishEmpty && x < 50; x++)
		{
			pick(0, Dish, foods[t]);
			Sleep(50);
			cout << "try to pick" << PlayerInfo.dish << endl;
		}
		while (PlayerInfo.dish != DishEmpty)
		{
			put(10, 1.57, 1);
			Sleep(50);
			cout << "try to put" << endl;
		}
	}
		/*while (PlayerInfo.dish == DishEmpty)
		{
			time++;
			for (int s = 0; s < 3; s++)
			{
				pick(0, Dish, foods[s]);
				Sleep(50);
			}
			cout << "try to pick" << PlayerInfo.dish<<endl;
			if (time > 5)
			{
				cout << "cleared" << endl;
				lzrclear = 0;
				for (int i = 0; i < 10; i++)
				{
					foods[i] = DishEmpty;
				}
				return;
			}
		}*/
	cout << "cleared" << endl;
	Sleep(200);
	lzrclear = 0;
	for (int i = 0; i < 10; i++)
	{
		foods[i] = DishEmpty;
	}
}

//交作业
void hand()
{
	avoid();
	int handtime = 0;
	while (true)
	{
		if (handtime == 0)
		{
			move(Up, 100);
			Sleep(5);
			if (PlayerInfo.position.y > 26.5)
			{
				handtime = 1;
			}
		}
		if (handtime == 1)
		{
			move(Right, 100);
			Sleep(5);
			if (PlayerInfo.position.x > 24.5)
			{
				handtime = 2;
			}
		}
		if (handtime == 2)
		{
			move(Down, 100);
			Sleep(5);
			if (PlayerInfo.position.y < 26.6)
			{
				for (int i = 0; i < 10; i++)
				{
					cout << "handing" << endl;
					use(0, 0, 0);
					Sleep(50);
				}
				if (PlayerInfo.dish == DishEmpty)
				{
					handtime = 3;
				}
				for (int i = 0; i < 10; i++)
				{
					foods[i] = DishEmpty;
				}
			}
		}
		if (handtime == 3)
		{
			move(Left, 100);
			Sleep(5);
			if (PlayerInfo.position.x < 7.6)
			{
				lzrtime = 2;
				lzrhand = 0;
				return;
			}
		}
	}
}

//往锅里扔吃的
void putfood()
{
	for (int i = 0; i < 10; i++)
	{
		if (foods[i] == DishEmpty)
		{
			foods[i] = PlayerInfo.dish;
			Sleep(50);
			break;
		}
	}
	while (PlayerInfo.dish != DishEmpty)
	{
		for (int i = 0; i < 3; i++)
		{
			if (PlayerInfo.dish == needs[i])
			{
				switch (lzrtime)
				{
				case 1:
					put(1, 0, 1);
				}
				Sleep(50);
			}
		}
		cout << "try to put into worker"<<endl;
	}
}

//判断是否开火
bool judge()
{
	if (mytask == 0)
	{
		return 0;
	}
	int k = 0;
	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 3; j++)
		{
			if (needs[i] == foods[j])
			{
				k++;
				break;
			}
		}
	}
	if (k == 3)
	{
		return true;
	}
	else return false;
}

//防撞
void avoid()
{
	if (getGameTime() - last_time > 1000)
	{
		last_x = now_x;
		last_y = now_y;
		now_x = PlayerInfo.position.x;
		now_y = PlayerInfo.position.y;
	}
	if (getGameTime() - last_time > 1000 && last_x == now_x && last_y == now_y)
	{
		cout << "Avoiding" << endl;
		Sleep(200);
		move(Left, 200);
		move(Left, 200);
		move(Left, 200);
		Sleep(200);
		move(Right, 200);
		move(Right, 200);
		move(Right, 200);
		Sleep(200);
		move(Up, 200);
		move(Up, 200);
		move(Up, 200);
		Sleep(200);
		move(Down, 200);
		move(Down, 200);
		move(Down, 200);
		Sleep(200);
	}
	if (getGameTime() - last_time > 1000)
	{
		last_time = getGameTime();
	}
}

//void go(int x, int y)
//{
//	if (((PlayerInfo.position.x - aim_x) > 0.5 || (PlayerInfo.position.x - aim_x) < -0.5) && ((PlayerInfo.position.y - aim_y) > 0.5) || (PlayerInfo.position.y - aim_y) < -0.5)
//	{
//		if ((PlayerInfo.position.x - aim_x) < -0.1)
//		{
//			if ((PlayerInfo.position.y - aim_y) < -0.1)
//			{
//				if (obj[0][2]->objType >= 0 && obj[0][2]->objType <= 5)
//				{
//					if (obj[0][1]->objType >= 0 && obj[0][1]->objType <= 5&& obj[1][2]->objType >= 0 && obj[1][2]->objType <= 5)
//					{
//						cout << '1' << endl;
//						move(LeftDown, 125);
//						Sleep(125);
//						move(RightDown, 125);
//						Sleep(125);
//					}
//					else if ((obj[0][1]->objType >= 0 && obj[0][1]->objType <= 5)|| (obj[0][0]->objType >= 0 && obj[0][0]->objType <= 5))
//					{
//						cout << '2' << endl;
//						move(Right, 125);
//						Sleep(125);
//					}
//					else if ((obj[1][2]->objType >= 0 && obj[1][2]->objType <= 5) || (obj[0][0]->objType >= 0 && obj[0][0]->objType <= 5))
//					{
//						cout << '3' << endl;
//						move(Up, 125);
//						Sleep(125);
//					}
//				}
//				else if ((obj[0][1]->objType >= 0 && obj[0][1]->objType <= 5))
//				{
//					cout << '4' << endl;
//					move(Right, 125);
//					Sleep(125);
//				}
//				else if ((obj[1][2]->objType >= 0 && obj[1][2]->objType <= 5))
//				{
//					cout << '5' << endl;
//					move(Up, 125);
//					Sleep(125);
//				}
//				move(RightUp, 50);
//				cout << "move rightup" << endl;
//				Sleep(50);
//
//			}
//			else if ((PlayerInfo.position.y - aim_y) > 0.1)
//			{
//				cout << "move rightdown" << endl;
//				move(RightDown, 50);
//				Sleep(50);
//			}
//			else
//			{
//				cout << "move right" << endl;
//				move(Right, 50);
//				Sleep(50);
//			}
//		}
//		else if ((PlayerInfo.position.x - aim_x) > 0.1)
//		{
//			if ((PlayerInfo.position.y - aim_y) < -0.1)
//			{
//				cout << "move leftup" << endl;
//				move(LeftUp, 50);
//				Sleep(50);
//			}
//			else if ((PlayerInfo.position.y - aim_y) > 0.1)
//			{
//				cout << "move down" << endl;
//				move(LeftDown, 50);
//				Sleep(50);
//			}
//			else
//			{
//				cout << "move left" << endl;
//				move(Left, 50);
//				Sleep(50);
//			}
//		}
//		if ((PlayerInfo.position.x - aim_x) < -0.1)
//		{
//			if ((PlayerInfo.position.y - aim_y) < -0.1)
//			{
//				move(RightUp, 50);
//				cout << "move rightup" << endl;
//				Sleep(50);
//
//			}
//			else if ((PlayerInfo.position.y - aim_y) > 0.1)
//			{
//				cout << "move rightdown" << endl;
//				move(RightDown, 50);
//				Sleep(50);
//			}
//			else
//			{
//				cout << "move right" << endl;
//				move(Right, 50);
//				Sleep(50);
//			}
//		}
//		else if ((PlayerInfo.position.x - aim_x) > 0.1)
//		{
//			if ((PlayerInfo.position.y - aim_y) < -0.1)
//			{
//				cout << "move leftup" << endl;
//				move(LeftUp, 50);
//				Sleep(50);
//			}
//			else if ((PlayerInfo.position.y - aim_y) > 0.1)
//			{
//				cout << "move down" << endl;
//				move(LeftDown, 50);
//				Sleep(50);
//			}
//			else
//			{
//				cout << "move left" << endl;
//				move(Left, 50);
//				Sleep(50);
//			}
//		}
//	}
//	cout << "move end" << endl;
//	Sleep(1000);
//}

void play()
{
	avoid();
	if (PlayerInfo.position.x > 45 && PlayerInfo.position.y < 4)
	{
		cout << "escaping" << endl;
		move(Left, 2000);
		Sleep(1000);
		lzrtime = 19;
	}
	else if (PlayerInfo.position.x > 45)
	{
		cout << "escaping" << endl;
		move(Down, 5000);
		Sleep(1000);
	}
	if (PlayerInfo.position.x < 3 && PlayerInfo.position.y>45)
	{
		cout << "escaping" << endl;
		move(Down, 5000);
		Sleep(1000);
		lzrtime = 0;
	}
	//getobj();
	/*char c;
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
		cout << "\tblocktype : " << i->objType << endl;
	}

	cout << "Game Time : " << THUAI3::getGameTime() << endl;*/
	/*getobj();
	if (getGameTime() == 0)
	{
		cout << "game start" << endl;
		Sleep(1000);
		aim_x = 10, aim_y = 10;
	}
	go(aim_x, aim_y);*/
	/*for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 3; j++)
		{
			if (i == 1 && j == 1)
			{
				continue;
			}
			if (obj[i][j]->objType == People)
			{
				avoid();
			}
		}
	}*/

	if (lzrtime == 0)
	{
		move(Up, 100);
		Sleep(5);
		if (PlayerInfo.position.y > 24)
		{
			lzrtime = 250;
		}
	}
	if (lzrtime == 250)
	{
		move(Right, 100);
		Sleep(5);
		if (PlayerInfo.position.x > 3.4)
		{
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 260;
		}
	}
	if (lzrtime == 260)
	{
		move(Up, 100);
		Sleep(5);
		if (PlayerInfo.position.y > 25.5)
		{
			lzrtime = 300;
		}
	}
	if (lzrtime == 300)
	{
		move(Right, 100);
		Sleep(5);
		if (PlayerInfo.position.x > 6.5)
		{
			lzrtime = 400;
		}
	}
	if (lzrtime == 400)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 24.6)
		{
			lzrtime = 1;
		}
	}
	//到达第一个工作台
	if (lzrtime == 1)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 7.4)
		{
			cout << "score:" << PlayerInfo.score;
			if (lzrclear)
			{
				clear();
			}
			if (PlayerInfo.dish != DishEmpty)
			{
				putfood();
			}
			if (judge())
			{
				for (int i = 0; i < 3; i++)
				{
					use(0, 0, 0);
					cout << "try to use" << endl;
					Sleep(50);
				}
				switch (mytask)
				{
				case TomatoFriedEgg:
					cout << "cooking" << endl;
					Sleep(21000);
					for (int i = 0; i < 5; i++)
					{
						pick(0, Block, 0);
						Sleep(100);
					}
					cout << "pick food" << PlayerInfo.dish << endl;
					if (PlayerInfo.dish == TomatoFriedEgg)
					{
						lzrhand = 1;
					}
					else
					{
						while (PlayerInfo.dish != DishEmpty)
						{
							put(5, 3.14, 1);
							Sleep(50);
						}
						for (int i = 0; i < 10; i++)
						{
							foods[i] = DishEmpty;
						}
					}
					break;
				case OverRice:
					cout << "cooking" << endl;
					Sleep(21000);
					for (int i = 0; i < 5; i++)
					{
						pick(0, Block, 0);
						Sleep(100);
					}
					cout << "pick food" << PlayerInfo.dish << endl;
					if (PlayerInfo.dish == OverRice)
					{
						lzrhand = 1;
					}
					else
					{
						while (PlayerInfo.dish != DishEmpty)
						{
							put(5, 3.14, 1);
							Sleep(50);
						}
						for (int i = 0; i < 10; i++)
						{
							foods[i] = DishEmpty;
						}
					}
					break;
				case Barbecue:
					cout << "cooking" << endl;
					Sleep(21000);
					for (int i = 0; i < 5; i++)
					{
						pick(0, Block, 0);
						Sleep(100);
					}
					cout << "pick food" << PlayerInfo.dish << endl;
					if (PlayerInfo.dish == Barbecue)
					{
						lzrhand = 1;
					}
					else
					{
						while (PlayerInfo.dish != DishEmpty)
						{
							put(5, 3.14, 1);
							Sleep(50);
						}
						for (int i = 0; i < 10; i++)
						{
							foods[i] = DishEmpty;
						}
					}
					break;
				case CookedRice:
					cout << "cooking" << endl;
					Sleep(11000);
					for (int i = 0; i < 5; i++)
					{
						pick(0, Block, 0);
						Sleep(100);
					}
					cout << "pick food" << PlayerInfo.dish << endl;
					if (PlayerInfo.dish == CookedRice)
					{
						lzrhand = 1;
					}
					else
					{
						while (PlayerInfo.dish != DishEmpty)
						{
							put(5, 3.14, 1);
							Sleep(50);
						}
						for (int i = 0; i < 10; i++)
						{
							foods[i] = DishEmpty;
						}
					}
					break;
				}
			}
			cout << "lzrhand:" << lzrhand;
			if (lzrhand)
			{
				cout << "go to hand" << endl;
				hand();
			}
			lzrtime = 2;
		}
	}

	//到达第一个食物生成点
	if (lzrtime == 2)
	{
		move(Up, 1000);
		Sleep(50);
		if (PlayerInfo.position.y > 40.4)
		{
			//mypick();
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 3;
		}
	}
	if (lzrtime == 3)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 20.5)
		{
			lzrtime = 4;
		}
	}
	if (lzrtime == 4)
	{
		move(Down, 100);
		Sleep(5);
		if (PlayerInfo.position.y < 38.5)
		{
			lzrtime = 5;
		}
	}

	//到达第二个工作台
	if (lzrtime == 5)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 24.4)
		{
			lzrtime = 6;
		}
	}
	if (lzrtime == 6)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 37.5)
		{
			lzrtime = 7;
		}
	}
	if (lzrtime == 7)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 31)
		{
			lzrtime = 2090;
		}
	}
	if (lzrtime == 2090)
	{
		move(Up, 100);
		Sleep(5);
		if (PlayerInfo.position.y > 40.4)
		{
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 200;
		}
	}
	//到达第二个食物生成点
	if (lzrtime == 200)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 41.4)
		{
			pickfood();
			//mypick();
			lzrtime = 10;
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
		}
	}
	if (lzrtime == 10)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 30.6)
		{
			lzrtime = 11;
		}
	}
	if (lzrtime == 11)
	{
		move(Left, 1000);
		Sleep(50);
		if (PlayerInfo.position.x < 40.5)
		{
			lzrtime = 12;
		}
	}
	if (lzrtime == 12)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 28.4)
		{
			lzrtime = 13;
		}
	}

	//到达第三个工作台
	if (lzrtime == 13)
	{
		move(Right, 1000);
		Sleep(50);
		if (PlayerInfo.position.x > 40.4)
		{
			lzrtime = 3000;
		}
	}
	if (lzrtime == 3000)
	{
		move(Down, 100);
		Sleep(5);
		if (PlayerInfo.position.y<26)
		{
			lzrtime = 3050;
		}
	}
	if (lzrtime == 3050)
	{
		move(Right, 100);
		Sleep(5);
		if (PlayerInfo.position.x > 42.4)
		{
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 14;
		}
	}
	if (lzrtime == 14)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 16)
		{
			lzrtime = 15;
		}
	}
	if (lzrtime == 15)
	{
		move(Left, 1000);
		Sleep(50);
		if (PlayerInfo.position.x < 39.5)
		{
			lzrtime = 16;
		}
	}
	if (lzrtime == 16)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 7)
		{
			lzrtime = 3060;
		}
	}
	if (lzrtime == 3060)
	{
		move(Right, 100);
		Sleep(5);
		if (PlayerInfo.position.x > 42.4)
		{
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 3070;
		}
	}
	if (lzrtime == 3070)
	{
		move(Down, 100);
		Sleep(5);
		if (PlayerInfo.position.y <6)
		{
			lzrtime = 17;
		}
	}
	//到达第三个食物生成点
	if (lzrtime == 17)
	{
		move(Left, 1000);
		Sleep(50);
		if (PlayerInfo.position.x < 26.6)
		{
			//mypick();
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 18;
		}
	}
	if (lzrtime == 18)
	{
		move(Down, 1000);
		Sleep(50);
		if (PlayerInfo.position.y < 4.5)
		{
			lzrtime = 4000;
		}
	}
	if (lzrtime == 4000)
	{
		move(Left, 100);
		Sleep(5);
		if (PlayerInfo.position.x < 6)
		{
			lzrtime = 4010;
		}
	}
	if (lzrtime == 4010)
	{
		move(Up, 100);
		Sleep(5);
		if (PlayerInfo.position.y > 4.4)
		{
			pickfood();
			cout << PlayerInfo.dish << endl;
			cout << mytask << endl;
			cout << needs[0] << needs[1] << needs[2] << endl;
			cout << "foods:" << endl;
			cout << foods[0] << foods[1] << foods[2] << endl;
			lzrtime = 19;
		}
	}
	if (lzrtime == 19)
	{
		move(Left, 1000);
		Sleep(50);
		if (PlayerInfo.position.x < 3.6)
		{
			lzrtime = 0;
		}
	}
	gettask();
	getneed();
	getobj();
	if (location(PlayerInfo.position.x, PlayerInfo.position.y)->objType == Dish && (getGameTime() - mypicktime) > 2000 && lzrmypick)
	{
		mypick();
	}
	//system("cls");
	//cout << PlayerInfo.dish << endl;
	/*  玩家在这里写代码  */
}

