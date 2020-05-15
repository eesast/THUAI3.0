#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include <thread>
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Cook;//指定人物天赋。选手代码必须定义此变量，否则报错

#define TIMOTHY_COMPLETE_NEW_NEW_NEW

#define TIMOTHY_COMPLETE_NEW_NEW

#define TIMOTHY_COMPLETE_NEW

#define TIMOTHY_COMPLETE

bool _first = true;

double Abs(double x)
{
	return x < 0 ? -x : x;
}

double Max(double x, double y)
{
	return x > y ? x : y;
}

void first()				//初次进入，移动到该去的地方
{
	/*DEBUG*/
	

	/*DEBUG*/

	
	double last_x = PlayerInfo.position.x;
	double last_y = PlayerInfo.position.y;
	if (last_x < 25 && last_y < 25 || last_x > 25 && last_y < 25)		//如果出生在左下角或右下角
	{
		do                              //移动到最下边
		{
			last_y = PlayerInfo.position.y;
			THUAI3::move(Down, 1000.0 / PlayerInfo.moveSpeed);
			Sleep(1000.0 / PlayerInfo.moveSpeed);
			if (PlayerInfo.moveSpeed > 1 && Abs(last_y - PlayerInfo.position.y) < 0.5 && PlayerInfo.position.y > 1.8)
			{
				THUAI3::move(Left, 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed);
				double tmy = PlayerInfo.position.y;
				THUAI3::move(Down, 2 * 1000.0 / PlayerInfo.moveSpeed);
				Sleep(2 * 1000.0 / PlayerInfo.moveSpeed);
				if (Abs(tmy - PlayerInfo.position.y) > 0.1)
				{
					THUAI3::move(Right, 1000.0 / PlayerInfo.moveSpeed);
					Sleep(1000.0 / PlayerInfo.moveSpeed);
				}
				continue;
			}
		} while (Abs(last_y - PlayerInfo.position.y) > 0.5 || PlayerInfo.position.y > 1.8);
		do                              //移动到最左边
		{
			////cout << "hahhhhhhhhhhhhha" << endl;
			last_x = PlayerInfo.position.x;
			THUAI3::move(Left, 1000.0 / PlayerInfo.moveSpeed);
			Sleep(1000.0 / PlayerInfo.moveSpeed);
			/////////////////cout << last_x << " " << PlayerInfo.position.x << endl;
			///////////cout << (PlayerInfo.moveSpeed > 1) << (Abs(last_x - PlayerInfo.position.x) < 0.5) << (PlayerInfo.position.x > 1.8) << endl;
			if (PlayerInfo.moveSpeed > 1 && Abs(last_x - PlayerInfo.position.x) < 0.5 && PlayerInfo.position.x > 1.8)
			{
				THUAI3::move(Up, 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed);
				double tmx = PlayerInfo.position.x;
				THUAI3::move(Left, 2 * 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed * 2);
				if (Abs(tmx - PlayerInfo.position.x) > 0.1)
				{
					THUAI3::move(Down, 1000.0 / PlayerInfo.moveSpeed);
					Sleep(1000.0 / PlayerInfo.moveSpeed);
				}
				continue;
			}
		} while (Abs(last_x - PlayerInfo.position.x) > 0.5 || PlayerInfo.position.x > 1.8);
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		double lsty = PlayerInfo.position.y;
		move(Up, 1000.0 / PlayerInfo.moveSpeed);
		Sleep(Max(1000.0 / PlayerInfo.moveSpeed, 50));
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		if (Abs(lsty - PlayerInfo.position.y) < 0.2)	//被人挡上
		{
			move(Right, 1000.0 / PlayerInfo.moveSpeed * 2);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2);
			move(Up, 1000.0 / PlayerInfo.moveSpeed * 2.2);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2.2);
			move(Left, 1000.0 / PlayerInfo.moveSpeed * 2.1);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2.1);
		}
		while (27.5 - PlayerInfo.position.y > 0.01)		//移动到1.5, 27.5
		{
			if (27.5 - PlayerInfo.position.y > PlayerInfo.moveSpeed)
			{
				THUAI3::move(Up, 1000);
				Sleep(1000);
			}
			else
			{
				THUAI3::move(Up, (27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
				Sleep((27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			}
		}
		while (8.5 - PlayerInfo.position.x > 0.01)			//移动到8.5， 27.5
		{
			if (8.5 - PlayerInfo.position.x > PlayerInfo.moveSpeed)
			{
				THUAI3::move(Right, 1000);
				Sleep(1000);
			}
			else
			{
				THUAI3::move(Right, (8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
				Sleep((8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			}
		}
	}

	else if (last_x < 25 && last_y > 25 || last_x > 25 && last_y > 25)				//在左上角或右上角
	{
		do                              //移动到最上边
		{
			last_y = PlayerInfo.position.y;
			THUAI3::move(Up, (1000.0 / PlayerInfo.moveSpeed));
			Sleep((1000.0 / PlayerInfo.moveSpeed));
			if (PlayerInfo.moveSpeed > 1 && Abs(last_y - PlayerInfo.position.y) < 0.5 && PlayerInfo.position.y < 48)
			{
				THUAI3::move(Left, 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed);
				double tmy = PlayerInfo.position.y;
				THUAI3::move(Up, 2 * 1000.0 / PlayerInfo.moveSpeed);
				Sleep(2 * 1000.0 / PlayerInfo.moveSpeed);
				if (Abs(tmy - PlayerInfo.position.y) > 0.1)
				{
					THUAI3::move(Right, 1000.0 / PlayerInfo.moveSpeed);
					Sleep(1000.0 / PlayerInfo.moveSpeed);
				}
				continue;
			}
		} while (Abs(last_y - PlayerInfo.position.y) > 0.5 || PlayerInfo.position.y < 48);
		do                              //移动到最左边
		{
			last_x = PlayerInfo.position.x;
			THUAI3::move(Left, (1000.0 / PlayerInfo.moveSpeed));
			Sleep((1000.0 / PlayerInfo.moveSpeed));
			if (PlayerInfo.moveSpeed > 1 && Abs(last_x - PlayerInfo.position.x) < 0.5 && PlayerInfo.position.x > 1.8)
			{
				THUAI3::move(Down, 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed);
				double tmx = PlayerInfo.position.x;
				THUAI3::move(Left, 2 * 1000.0 / PlayerInfo.moveSpeed);
				Sleep(2 * 1000.0 / PlayerInfo.moveSpeed);
				if (Abs(tmx - PlayerInfo.position.x) > 0.1)
				{
					THUAI3::move(Up, 1000.0 / PlayerInfo.moveSpeed);
					Sleep(1000.0 / PlayerInfo.moveSpeed);
				}
				continue;
			}
		} while (Abs(last_x - PlayerInfo.position.x) > 0.5 || PlayerInfo.position.x > 1.8);

		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		double lsty = PlayerInfo.position.y;
		move(Down, 1000.0 / PlayerInfo.moveSpeed);
		Sleep(1000.0 / PlayerInfo.moveSpeed);
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		if (Abs(lsty - PlayerInfo.position.y) < 0.2)	//被人挡上
		{
			move(Right, 1000.0 / PlayerInfo.moveSpeed * 2);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2);
			move(Down, 1000.0 / PlayerInfo.moveSpeed * 2.2);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2.2);
			move(Left, 1000.0 / PlayerInfo.moveSpeed * 2.1);
			Sleep(1000.0 / PlayerInfo.moveSpeed * 2.1);
		}

		while (PlayerInfo.position.y - 27.5 > 0.01)		//移动到1.5, 27.5
		{
			if (PlayerInfo.position.y - 27.5 > PlayerInfo.moveSpeed)
			{
				THUAI3::move(Down, 1000);
				Sleep(1000);
			}
			else
			{
				THUAI3::move(Down, (PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
				Sleep((PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
			}
		}
		while (8.5 - PlayerInfo.position.x > 0.01)			//移动到8.5， 27.5
		{
			if (8.5 - PlayerInfo.position.x > PlayerInfo.moveSpeed)
			{
				THUAI3::move(Right, 1000);
				Sleep(1000);
			}
			else
			{
				THUAI3::move(Right, (8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
				Sleep((8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			}
		}

		/*do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Right, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
			Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
		} while (PlayerInfo.position.x < 24.3);*/



	}

	/*DEBUUG*/
	

	/*DEBUG*/
}



void Shangjiao(int pos)
{
	if (pos == 1)
	{
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Right, (11.501 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((11.501 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x < 11.4);
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Down, (PlayerInfo.position.y - 23.3) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.y - 23.3) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.y > 23.5);
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Right, (24.51 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((24.51 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x < 24.3);

		
		while (Abs(PlayerInfo.moveSpeed) < 0.1)
		{
			Sleep(50);
		}

		move(Up, 50);

		Sleep(55);
		THUAI3::use(0, 0, 0);
		Sleep(55);
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 10.4) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.x - 10.4) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x > 10.6);
		
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Up, (27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.y < 27.3);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 10.4) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.x - 10.4) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x > 10.6);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 8.5) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.x - 8.5) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x > 8.7);

	}
	else
	{
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Down, (PlayerInfo.position.y - 36.4) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.y - 36.4) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.y > 36.6);
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Right, (28.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((28.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x < 28.3);
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Down, (PlayerInfo.position.y - 25.5) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.y - 25.5) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.y > 25.7);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 26.4) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.x - 26.4) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.x > 26.6);


		/*do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.x > 22.6);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Down, (PlayerInfo.position.y - 25.4) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.y - 25.4) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.y > 25.5);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Right, 1000.0 / PlayerInfo.moveSpeed);
			Sleep(1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.x < 23.2);

		move(Right, 0);*/
		Sleep(55);
		while (Abs(PlayerInfo.moveSpeed) < 0.1)
		{
			Sleep(50);
		}
		THUAI3::use(0, 0, 0);
		Sleep(50);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Right, (28.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((28.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x < 28.3);

		do
		{

			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Up, (36.7 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((36.7 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));

		} while (PlayerInfo.position.y < 36.5);

		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.x > 22.7);

		do
		{

			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Up, (38.49 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((38.49 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));

		} while (PlayerInfo.position.y < 38.27);

		/*do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			move(Left, 500.0 / PlayerInfo.moveSpeed);
			Sleep(500.0 / PlayerInfo.moveSpeed);
		} while (PlayerInfo.position.x > 22.8);
		do
		{
			
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Up, (38.49 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
			Sleep((38.49 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
			
		} while (PlayerInfo.position.y < 38.27);*/


		bool hasmove = false;
		Sleep(55);
		while (PlayerInfo.position.y > 38.5)
		{

			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Down, (PlayerInfo.position.y - 38.3) * 1000.0 / PlayerInfo.moveSpeed);
			Sleep((PlayerInfo.position.y - 38.3) * 1000.0 / PlayerInfo.moveSpeed);
			hasmove = true;
		}
		if (hasmove)
		{
			THUAI3::move(Up, 0);
			Sleep(55);
		}

	}
	
}

//void PrintPos()
//{
//	while (1)
//	{
//		cout << "pos: " << PlayerInfo.position.x << " " << PlayerInfo.position.y << endl;
//		Sleep(600);
//	}
//}


void play()
{



	//cout << "begin!" << endl;
	//std::thread prt(PrintPos);			/*debug!!!!!!!!!!!!!!!!!!!!!!!*/
	if (_first)				//开局移动到好的位置
	{




		Sleep(1000);
		_first = false;
		first();
	}

	int countleft;
	DishType whatleft;
	list<Obj> lt;
	int count, countorg, where;
	int countup;
	DishType whatup;
	DishType what;
	int cut1, cut2;
	
cooker1:

	//cout << "begin!" << endl;

	lt = MapInfo::get_mapcell(8, 24);		//查锅
	//cout << "begin!" << endl;
	count = 0;
	countorg = 0;
	where = -1;
	what = DishEmpty;
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		//cout << "end!" << endl;
		
		if (itr->objType == ObjType::Block)
		{
			if (int(itr->dish) >= int(Wheat) && int(itr->dish) <= int(Ketchup))
			{
				if (what == DishEmpty) what = itr->dish;
				where = 0;
				++count;
				++countorg;
			}
			if (int(itr->dish) >= int(CookedRice) && int(itr->dish) <= int(SpicedPot6))	//菜熟了
			{
				do
				{
					while (Abs(PlayerInfo.moveSpeed) < 0.1)
					{
						Sleep(50);
					}
					THUAI3::move(Down, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
					Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
				} while (PlayerInfo.position.y > 25.6);
			repick:
				pick(false, Block, itr->dish);
				Sleep(52);
				if (int(PlayerInfo.dish) >= int(CookedRice) && int(PlayerInfo.dish) <= int(SpicedPot6))
				{
					Shangjiao(1);
					goto cooker2;
				}
				if (PlayerInfo.dish == DarkDish) put(4, 3.14, true);
				if (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(55);
					goto repick;
				}
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				do
				{
					while (Abs(PlayerInfo.moveSpeed) < 0.1)
					{
						Sleep(50);
					}
					move(Up, (27.5 - PlayerInfo.position.y) * 1000 / PlayerInfo.moveSpeed);
					Sleep((27.5 - PlayerInfo.position.y) * 1000 / PlayerInfo.moveSpeed);
				} while (PlayerInfo.position.y < 27.4);
				goto cooker2;
			}
		}
		if (itr->objType == ObjType::Dish)
		{
			if (what == DishEmpty) what = itr->dish;
			where = 0;
			++count;
			++countorg;
		}
	}

	//cout << "end333!" << endl;

	lt = MapInfo::get_mapcell(8, 25);
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		if (itr->objType == ObjType::Dish)
		{
			if (what == DishEmpty) what = itr->dish;
			if (where == -1) where = 2;
			++count;
		}
	}

	

	//cout << "end555!" << endl;

	if (countorg >= 2 || countorg >= 1 && PlayerInfo.dish != DishEmpty)	//开始做饭
	{
		
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Down, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
			Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
		} while (PlayerInfo.position.y > 25.6);
		while (Abs(PlayerInfo.moveSpeed) < 0.1)
		{
			Sleep(50);
		}
		THUAI3::put(1, 5, true);
		Sleep(250);

		recheck1:
		lt = MapInfo::get_mapcell(8, 25);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->objType == Dish)
			{
				pick(true, Dish, itr->dish);
				Sleep(55);
				put(1, 5, true);
				Sleep(55);
				goto recheck1;
			}
		}

	
		lt = MapInfo::get_mapcell(8, 24);
		cut1 = 0, cut2 = 0;
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->dish != DishEmpty)
			{

				++cut1;
			}
		}

		THUAI3::use(0, 0, 0);
		Sleep(60);
		lt = MapInfo::get_mapcell(8, 24);

		
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->objType == ObjType::Dish)
			{

				++cut2;
			}
		}

		//等着
		if (cut2 < cut1)
		{

			wait:
			THUAI3::move(Down, 0);
			Sleep(52);
			int cnt = 0;

			while (cnt < 1205)
			{
				bool iscooking = false;
				list<Obj>lt = MapInfo::get_mapcell(8, 24);
				for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
				{
					
					if (itr->objType == ObjType::Block)
					{
						
						if (int(itr->dish) >= int(CookedRice) && int(itr->dish) <= int(SpicedPot6))
						{
							while (Abs(PlayerInfo.moveSpeed) < 0.1)
							{
								Sleep(50);
							}
							pick(false, Block, itr->dish);
							Sleep(55);
							if (int(PlayerInfo.dish) >= int(CookedRice) && int(PlayerInfo.dish) <= int(SpicedPot6))
							{
								Shangjiao(1);
								goto cooker2;
							}
						}
						else if (int(itr->dish) >= int(Flour) && int(itr->dish) <= int(Ketchup))
						{
							pick(false, Block, itr->dish);
							Sleep(55);
						}
						if (itr->dish == DishType::CookingDish) { iscooking = true; };
					}
					
				}
				++cnt;
				Sleep(51);
				if (cnt > 5 && iscooking == false) break;
			}

			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Up, (27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
				Sleep((27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			} while (PlayerInfo.position.y < 27.3);
		}
		else
		{
			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Up, (27.5 - PlayerInfo.position.y) * 1000.0 / PlayerInfo.moveSpeed);
				Sleep((27.5 - PlayerInfo.position.y) * 1000.0 / PlayerInfo.moveSpeed);
			} while (PlayerInfo.position.y < 27.4);
		}
	}

	else if (count != 0 && PlayerInfo.dish == DishEmpty)
	{
		if (where == 0 || where == 2)
		{
			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				THUAI3::move(Down, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
				Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
			} while (PlayerInfo.position.y > 25.6);
			if (where == 2) pick(true, Dish, what);
			else
			{
				pick(false, Block, what);
				if (PlayerInfo.dish == DishEmpty) { Sleep(55); pick(false, Dish, what); }
				Sleep(55);
			}

			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Up, (27.5 - PlayerInfo.position.y) * 1000.0 / PlayerInfo.moveSpeed);
				Sleep((27.5 - PlayerInfo.position.y) * 1000.0 / PlayerInfo.moveSpeed);
			} while (PlayerInfo.position.y < 27.4);
		}
	}

	Sleep(50);
	countleft = 0;	//检查左边
	whatleft = DishEmpty;
	lt = MapInfo::get_mapcell(7, 24);
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		if (itr->objType == ObjType::Dish)
		{
			if (whatleft == DishEmpty) whatleft = itr->dish;
			++countleft;
		}
	}

	if (whatleft != DishEmpty)
	{
		do
		{
			while (PlayerInfo.moveSpeed < 0.2)
			{
				Sleep(50);
			}
			move(Left, (PlayerInfo.position.x - 7.3) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.x - 7.3) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x > 7.5);


		do
		{
			while (PlayerInfo.moveSpeed < 0.2)
			{
				Sleep(50);
			}
			move(Down, (PlayerInfo.position.y - 24.5) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.y - 24.5) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.y > 24.7);

		move(Right, 50);
		Sleep(60);
		if (PlayerInfo.dish == DishEmpty)
		{
			put(1, 0, true);
			Sleep(53);
			pick(true, Dish, whatleft);
			Sleep(53);
		}
		bool okcook = false;
		
		{
			list<Obj> lst = MapInfo::get_mapcell(7, 24);
			list<Obj>::iterator itr;
			for (itr = lst.begin(); itr != lst.end(); ++itr)
			{
				if (itr->objType == ObjType::Dish)
				{
					put(1, 0, true);
					Sleep(53);
					okcook = true;
					pick(1, Dish, itr->dish);
					Sleep(53);
				}
			}

		}

		do
		{
			while (PlayerInfo.moveSpeed < 0.2) Sleep(50);
			move(Up, (27.5 - PlayerInfo.position.y) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((27.5 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.y < 27.3);

		do
		{
			while (PlayerInfo.moveSpeed < 0.2) Sleep(50);
			move(Right, (8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.x < 8.3);

		if (okcook) goto cooker1;
		
	}

	countup = 0;
	lt = MapInfo::get_mapcell(8, 25);
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		if (itr->objType == ObjType::Dish)
		{
			++countup;
			break;
		}
	}
	if (countup != 0)
	{
		do
		{
			while (PlayerInfo.moveSpeed < 0.1) Sleep(50);
			move(Down, (PlayerInfo.position.y - 25.5)* (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.y - 25.5)* (1000.0 / PlayerInfo.moveSpeed));
		} while (PlayerInfo.position.y > 25.7);

		lt = MapInfo::get_mapcell(8, 25);
		put(1, 5, true);
		Sleep(53);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->objType == ObjType::Dish)
			{
				pick(true, Dish, itr->dish);
				Sleep(53);
				put(1, 5, true);
				Sleep(53);
			}
		}
		Sleep(53);
		use(0, 0, 0);
		Sleep(53);
		goto wait;
	}




cooker2:

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Up, (30.5 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((30.5 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y < 30.3);
	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Left, (PlayerInfo.position.x - 7.5) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.x - 7.5)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.x > 7.7);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Up, (40.51 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((40.51 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y < 40.3);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Right, (22.5 - PlayerInfo.position.x)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((22.5 - PlayerInfo.position.x)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.x < 22.3);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Down, (PlayerInfo.position.y - 38.49)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.y - 38.49)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y > 38.5);



	bool hasmove = false;
	Sleep(53);
	while (PlayerInfo.position.y > 38.5)
	{
		while (Abs(PlayerInfo.moveSpeed) < 0.1)
		{
			Sleep(50);
		}
		THUAI3::move(Down, (PlayerInfo.position.y - 38.3) * 1000.0 / PlayerInfo.moveSpeed);
		Sleep((PlayerInfo.position.y - 38.3) * 1000.0 / PlayerInfo.moveSpeed);
		hasmove = true;
	}
	if (hasmove)
	{
		THUAI3::move(Up, 0);
		Sleep(53);
	}

	lt = MapInfo::get_mapcell(25, 38);		//查锅
	//cout << "begin!" << endl;
	count = 0;
	countorg = 0;
	where = -1;
	what = DishEmpty;
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		//cout << "end!" << endl;
		if (itr->objType == ObjType::Block)
		{
			if (int(itr->dish) >= int(Wheat) && int(itr->dish) <= int(Ketchup))
			{
				if (what == DishEmpty) what = itr->dish;
				where = 0;
				++count;
				++countorg;
			}
			if (int(itr->dish) >= int(CookedRice) && int(itr->dish) <= int(SpicedPot6))	//菜熟了
			{
				do
				{
					while (Abs(PlayerInfo.moveSpeed) < 0.1)
					{
						Sleep(50);
					}
					THUAI3::move(Right, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
					Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
				} while (PlayerInfo.position.x < 24.3);
			repick2:
				pick(false, Block, itr->dish);
				Sleep(55);
				if (int(PlayerInfo.dish) >= int(CookedRice) && int(PlayerInfo.dish) <= int(SpicedPot6))
				{
					Shangjiao(2);
					goto end;
				}
				if (PlayerInfo.dish == DarkDish) put(6, 1.57, true);
				if (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(53);
					goto repick2;
				}

				do
				{
					while (Abs(PlayerInfo.moveSpeed) < 0.1)
					{
						Sleep(50);
					}
					move(Left, (PlayerInfo.position.x - 22.5) * 1000 / PlayerInfo.moveSpeed);
					Sleep((PlayerInfo.position.x - 22.5) * 1000 / PlayerInfo.moveSpeed);
				} while (PlayerInfo.position.x > 22.6);
				goto end;
			}
		}
		if (itr->objType == ObjType::Dish)
		{
				if (what == DishEmpty) what = itr->dish;
				where = 0;
				++count;
				++countorg;
		}
	}

	//cout << "end333!" << endl;

	lt = MapInfo::get_mapcell(24, 38);
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
	{
		if (itr->objType == ObjType::Dish)
		{
			if (what == DishEmpty) what = itr->dish;
			if (where == -1) where = 2;
			++count;
		}
	}

	//cout << "end555!" << endl;

	if (countorg >= 2 || countorg >= 1 && PlayerInfo.dish != DishEmpty)	//开始做饭
	{
		do
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Right, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
			Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
		} while (PlayerInfo.position.x < 24.3);
		while (Abs(PlayerInfo.moveSpeed) < 0.1)
		{
			Sleep(50);
		}
		THUAI3::put(1, 0, true);
		Sleep(250);
		lt = MapInfo::get_mapcell(25, 38);
		int cut1 = 0, cut2 = 0;
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->dish != DishEmpty)
			{

				++cut1;
			}
		}

		THUAI3::use(0, 0, 0);
		Sleep(55);
		lt = MapInfo::get_mapcell(25, 38);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (itr->objType == ObjType::Dish)
			{

				++cut2;
			}
		}

		//等着
		if (cut2 < cut1)
		{
			THUAI3::move(Right, 0);
			Sleep(52);
			int cnt = 0;

			while (cnt < 1205)
			{
				list<Obj>lt = MapInfo::get_mapcell(25, 38);
				bool iscooking = false;
				for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
				{
					if (itr->objType == ObjType::Block)
					{
						if (int(itr->dish) >= int(CookedRice) && int(itr->dish) <= int(SpicedPot6))
						{
							while (Abs(PlayerInfo.moveSpeed) < 0.1)
							{
								Sleep(50);
							}
							pick(false, Block, itr->dish);
							Sleep(53);
							if (int(PlayerInfo.dish) >= int(CookedRice) && int(PlayerInfo.dish) <= int(SpicedPot6))
							{
								Shangjiao(2);
								goto end;	///////////////////////////////////////////////////////
							}
							else if (int(itr->dish) >= int(Flour) && int(itr->dish) <= int(Ketchup))
							{
								pick(false, Block, itr->dish);
								Sleep(55);
							}
						}
						if (itr->dish == DishType::CookingDish) { iscooking = true; };
					}
					
				}

				++cnt;
				Sleep(53);
				if (cnt > 20 && iscooking == false) break;
			}

			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Left, (PlayerInfo.position.x - 22.5) * (1000.0 / PlayerInfo.moveSpeed) * 0.5);
				Sleep((PlayerInfo.position.x - 22.5) * (1000.0 / PlayerInfo.moveSpeed) * 0.5);
			} while (PlayerInfo.position.x > 22.8);

		}
		else
		{
			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Left, (PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
				Sleep((PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
			} while (PlayerInfo.position.x > 22.6);
		}
	}

	else if (count != 0 && PlayerInfo.dish == DishEmpty)
	{
		if (where == 0 || where == 2)
		{
			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				THUAI3::move(Right, (1000.0 / PlayerInfo.moveSpeed) * 0.5);
				Sleep((1000.0 / PlayerInfo.moveSpeed) * 0.5);
			} while (PlayerInfo.position.x < 24.4);
			if (where == 2) pick(true, Dish, what);
			else
			{
				pick(false, Block, what);
				if (PlayerInfo.dish == DishEmpty)
				{
					Sleep(53);
					pick(false, Dish, what);
				}
				Sleep(53);
			}

			do
			{
				while (Abs(PlayerInfo.moveSpeed) < 0.1)
				{
					Sleep(50);
				}
				move(Left, (PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
				Sleep((PlayerInfo.position.x - 22.5) * 1000.0 / PlayerInfo.moveSpeed);
			} while (PlayerInfo.position.x > 22.6);
		}
	}



end:

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Up, (40.4 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((40.4 - PlayerInfo.position.y)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y < 40.2);
	
	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Left, (PlayerInfo.position.x - 8.5)* (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.x - 8.5)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.x > 8.7);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Down, (PlayerInfo.position.y - 39.5) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.y - 39.5) * (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y > 39.7);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Left, (PlayerInfo.position.x - 7.5) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.x - 7.5) * (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.x > 7.7);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Down, (PlayerInfo.position.y - 30.5) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.y - 30.5) * (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y > 30.7);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Right, (8.5 - PlayerInfo.position.x) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((8.5 - PlayerInfo.position.x)* (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.x < 8.3);

	do
	{
		while (PlayerInfo.moveSpeed < 0.1)
		{
			Sleep(50);
		}
		move(Down, (PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
		Sleep((PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
	} while (PlayerInfo.position.y > 27.7);

	/*do
	{
		if ((PlayerInfo.position.y - 27.5) > PlayerInfo.moveSpeed)
		{

			THUAI3::move(Down, 1000);
			Sleep(1000);
		}
		else
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Down, (PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.y - 27.5) * (1000.0 / PlayerInfo.moveSpeed));
		}
	} while (PlayerInfo.position.y > 27.6);

	do
	{
		if ((PlayerInfo.position.x - 8.5) > PlayerInfo.moveSpeed)
		{
			THUAI3::move(Left, 1000);
			Sleep(1000);
		}
		else
		{
			while (Abs(PlayerInfo.moveSpeed) < 0.1)
			{
				Sleep(50);
			}
			THUAI3::move(Left, (PlayerInfo.position.x - 8.5) * (1000.0 / PlayerInfo.moveSpeed));
			Sleep((PlayerInfo.position.x - 8.5) * (1000.0 / PlayerInfo.moveSpeed));
		}
	} while (PlayerInfo.position.x > 8.6);*/

}





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

	cout << "Game Time : " << THUAI3::getGameTime() << endl;
	/*  玩家在这里写代码  */

//char c;
	//cin.clear();
	//cin.ignore();
	//cin >> c;
	//switch (c)
	//{
	//case 'd':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::Right, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'e':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::RightUp, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}
	//break;
	//case 'w':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::Up, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'q':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::LeftUp, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'a':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::Left, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'z':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::LeftDown, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'x':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::Down, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'c':
	//{
	//	int moveDistance = 0;
	//	std::cout << endl << "Please Input your move distance" << endl;
	//	cin >> moveDistance;
	//	move(Protobuf::Direction::RightDown, moveDistance / PlayerInfo.moveSpeed * 1000);
	//}break;
	//case 'f':
	//{
	//	std::cout << endl << "Please Input 3 parameters : isSelfPosition, pickType, dishOrToolType" << endl;
	//	bool isSelfPosition = 0;
	//	cin >> isSelfPosition;
	//	int pickType = 0;
	//	cin >> pickType;
	//	int dishOrToolType = 0;
	//	cin >> dishOrToolType;
	//	pick(isSelfPosition, (ObjType)pickType, dishOrToolType);
	//}
	//break;
	//case 'u':
	//{
	//	std::cout << endl << "Please Input 2 parameters : " << endl;
	//	double param1 = 0;
	//	cin >> param1;
	//	double param2 = 0;
	//	cin >> param2;
	//	use(1, param1, param2);
	//}
	//break;
	//case 'i': use(0); break;
	//case 'r':
	//{
	//	std::cout << endl << "Please Input 2 parameters : " << endl;
	//	double distance = 0;
	//	cin >> distance;
	//	double angle = 0;
	//	cin >> angle;
	//	put(distance, angle, true);
	//	_sleep(26);
	//	move(Left, 1000);
	//}
	//break;
	//case 't':
	//{
	//	std::cout << endl << "Please Input 2 parameters : " << endl;
	//	double distance = 0;
	//	cin >> distance;
	//	double angle = 0;
	//	cin >> angle;
	//	put(distance, angle, false);
	//}
	//break;
	//case ':':
	//{
	//	std::cout << endl << "Please Input your text to speak : " << endl;
	//	string str;
	//	cin >> str;
	//	speakToFriend(str);
	//}
	//break;
	//case 'm':
	//{
	//	std::cout << endl << "Input two interger to print a map cell :" << endl;
	//	int x, y;
	//	cin.clear();
	//	cin.ignore();
	//	cin >> x >> y;
	//	list<Obj> l = MapInfo::get_mapcell(x, y);
	//	std::cout << "objs in map[" << x << "][" << y << "] :" << endl;
	//	for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
	//	{
	//		std::cout << "\tblocktype : " << i->objType << endl;
	//	}
	//}
	//break;
	//case 's':
	//{
	//	std::cout << endl << "Self info :" << endl;
	//	std::cout << "\tposition : " << PlayerInfo.position.x << "," << PlayerInfo.position.y << endl;
	//	std::cout << "\tdish : " << PlayerInfo.dish << endl;
	//	std::cout << "\ttool : " << PlayerInfo.tool << endl;
	//	std::cout << "\trecieveText : " << PlayerInfo.recieveText << endl;
	//}

	//default:
	//	break;
	//}


	//std::cout << "Game Time : " << THUAI3::getGameTime() << endl;
	///*  玩家在这里写代码  */

