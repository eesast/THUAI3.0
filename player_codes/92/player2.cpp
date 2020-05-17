#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
#include<cmath>
#define TRUE 1
#define FALSE 0
#define BR_S_FRTH
using namespace THUAI3;
static bool isstartgame = TRUE;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//指定人物天赋。选手代码必须定义此变量，否则报错
static int glx = 33;
static int gly = 18;
static int istool = 0;//是否有工具以及工具种类
bool isblock(double x,double y)
{
	list<Obj> lt = MapInfo::get_mapcell(x, y);
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		if (itr->objType == ObjType::People)//这里增加了人。
			return TRUE;
		else if(itr->objType == ObjType::Block)
			if (itr->blockType == BlockType::Wall || /*itr->blockType == BlockType::TaskPoint ||*/ itr->blockType == BlockType::FoodPoint|| itr->blockType == BlockType::Table|| itr->blockType == BlockType::Cooker)
			{
				return TRUE;
			}
	return FALSE;
}
void movetob(double x, double y);//先移动y模式
double Abs(double x)
{
	return x < 0 ? -x : x;
}
void gettool()
{
	if (istool)
		return;//有tool就不执行。
	double x = PlayerInfo.position.x;
	double y = PlayerInfo.position.y;
	list<Obj> lt = MapInfo::get_mapcell(x,y);//当前位置
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			pick(TRUE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x-1, y);//左边
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::Left, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x + 1, y);//右边
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::Right, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x, y-1);//下
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::Down, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x , y+1);//上
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::Up, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x - 1, y-1);//左下
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::LeftDown, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x - 1, y+1);//左上
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::LeftUp, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x + 1, y-1);//右下
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::RightDown, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	lt = MapInfo::get_mapcell(x + 1, y+1);//右上
	for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); itr++)
		if (itr->tool >= 1 && itr->tool <= 15)
		{
			move(Protobuf::RightUp, 0);
			Sleep(50);
			pick(FALSE, Tool, itr->tool);//捡起
			istool = int(itr->tool);
			return ;
		}
	return ;
}

void usetool(double x = -1,double y = -1)//使用工具
{
	switch (istool)
	{
	case 1://tigershoes
	case 2://SpeedBuff 
	case 3://StrengthBuff = 3; //加力量，一定时间内增加扔物品的距离
	case 4://TeleScope = 4;    //望远镜，增加视野范围
	case 7://BreastPlate = 7;  //护心镜，防止各种攻击性道具
	case 9://WaveGlueBottle = 9; //滔牌胶水，踩上后会减速，过一段时间自行消失
	//从10开始都是放到锅处
	case 10://LandMine = 10;    //放置地雷
	case 11://TrapTool = 11;    //放置陷阱
	case 12://FlashBomb = 12;   //放置闪光炸弹
		use(1);
		Sleep(50);
		istool = 0; break;//这些都是拿来直接用
	//这些是扔到灶台上的人，因为运动的可能打不中
	case 13:// ThrowHammer = 13; //扔锤子
	case 14:// Bow = 14;         //弓箭
		double x1 = PlayerInfo.position.x;
		double y1 = PlayerInfo.position.y;
		double dis = sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
		double theta = atan((y - y1) / (x - x1));
		use(1, dis, theta);
		Sleep(50);
		istool = 0; break;
	}
}
void movetoa(double x, double y)//基本单位移动1，移动曼哈顿距离。先移动x.
{
	gettool();
	if (istool && istool <= 9)
		usetool();
	if (Abs(x - 33) < 1 && Abs(y - 18) < 1)
	{
		x = 33; y = 19;
	}
	else if (Abs(x - 41) < 1 && Abs(y - 28) < 1)
	{
		x = 41; y = 27;
	}
	double x1 = PlayerInfo.position.x, y1 = PlayerInfo.position.y;
	if (Abs(x - x1) <= 0.5 && Abs(y - y1) <= 0.5)
		return;
	else if (Abs(x - x1) <= 0.5 && Abs(y - y1) > 0.5)
		movetob(x, y);
	else if (x > x1 && y > y1)//右上移动
	{
		if(!isblock(x1+1,y1))
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.x < x1 + 0.8)
			{
				move(Protobuf::Direction::Down, 100);
				Sleep(100);
			}
			movetoa(x , y);
		}
		else if (!isblock(x1, y1+1))
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x, y );
		}
		else
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x, y);
		}
	}
	else if (x > x1 && y < y1)
	{
		if (!isblock(x1 + 1, y1))
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.x < x1 + 0.8)
			{
				move(Protobuf::Direction::Up, 100);
				Sleep(100);
			}
			movetoa(x, y);
		}
		else if (!isblock(x1, y1-1))
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x, y);
		}
		else
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x, y);
		}
	}
	else if (x < x1 && y >y1)
	{
		if (!isblock(x1 - 1, y1))
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed);
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.x > x1 - 0.8)
			{
				move(Protobuf::Direction::Down, 100);
				Sleep(100);
			}
			movetoa(x , y);
		}
		else if (!isblock(x1, y1+1))
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed);
			movetoa(x, y );
		}
		else
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x, y);
		}
	}
	else if (x < x1 && y < y1)
	{
		if (!isblock(x1-1,y1))
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.x > x1 - 0.8)
			{
				move(Protobuf::Direction::Up, 100);
				Sleep(100);
			}
			movetoa(x , y);
		}
		else if (!isblock(x1, y1-1))
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x, y );
		}
		else
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x, y);
		}
	}
}
void movetob(double x, double y)//基本单位移动1，移动曼哈顿距离。
{
	gettool();
	if (istool && istool <= 9)
		usetool();
	double x1 = PlayerInfo.position.x, y1 = PlayerInfo.position.y;
	if (Abs(x - x1) <= 0.5 && Abs(y - y1) <= 0.5)
		return;
	else if (Abs(x - x1) > 0.5 && Abs(y - y1) <= 0.5)
		movetoa(x, y);
	else if (x > x1 && y > y1)//右上移动
	{
		if (!isblock(x1, y1+1))
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.y < y1 + 0.8)
			{
				move(Protobuf::Direction::Left, 100);
				Sleep(100);
			}
			movetob(x, y);
		}
		else if (!isblock(x1 + 1, y1))
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x, y);
		}
		else
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x, y);
		}
	}
	else if (x > x1 && y < y1)//右下
	{
		if (!isblock(x1 , y1-1))
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.y > y1 - 0.8)
			{
				move(Protobuf::Direction::Left, 100);
				Sleep(100);
			}
			movetob(x, y );
		}
		else if (!isblock(x1 + 1, y1))
		{
			move(Protobuf::Direction::Right, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x , y);
		}
		else
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x , y );
		}
	}
	else if (x < x1 && y > y1)//左上
	{
		if (!isblock(x1, y1+1))
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.y < y1 + 0.8)
			{
				move(Protobuf::Direction::Right, 100);
				Sleep(100);
			}
			movetob(x, y );
		}
		else if (!isblock(x1 - 1, y1))
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetob(x , y);
		}
		else
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed);
			movetoa(x , y);
		}
	}
	else if (x < x1 && y < y1)//左下
	{
		if (!isblock(x1, y1-1))
		{
			move(Protobuf::Direction::Down, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			if (PlayerInfo.position.y > y1 - 0.8)
			{
				move(Protobuf::Direction::Right, 100);
				Sleep(100);
			}
			movetob(x, y);
		}
		else if (!isblock(x1 - 1, y1))
		{
			move(Protobuf::Direction::Left, 1000.0 / PlayerInfo.moveSpeed );
			Sleep(1000.0 / PlayerInfo.moveSpeed);
			movetob(x , y);
		}
		else
		{
			move(Protobuf::Direction::Up, 1000.0 / PlayerInfo.moveSpeed);
			Sleep(1000.0 / PlayerInfo.moveSpeed );
			movetoa(x, y);
		}
	}
}
bool searchren(double x, double y)//找某一位置是否有人
{
	list<Obj>lt = MapInfo::get_mapcell(x,y);
	if(lt.begin()->blockType == People)
		return TRUE;
	return FALSE;
}
void submit()
{
	//cout << "hhhhhhhhhhhhh" << endl; /////////////////////////////////////////////////////////////////////////////////////////////////////////
	//double x = PlayerInfo.position.x, y = PlayerInfo.position.y;
	movetoa(26.5,25);
	/////////////////////////cout << "hhhhhhhhhhhhh" << endl; /////////////////////////////////////////////////////////////////////////////////////////////////////////
	move(Protobuf::Direction::Left,0);
	Sleep(60);
	use(0);
	Sleep(60);
	movetoa(37, 23);//交完直接返回两锅中点
	//movetoa(x, y);
}
void play()
{
	int flag = 1;
	if (isstartgame)
	{
		Sleep(1000);
		//movetoa(34, 17.5);
		isstartgame = FALSE;
	}
	movetoa(37, 23);
	while (1)
	{
		if (istool == 13 || istool == 14)//扔锤子等。
		{
			if (searchren(33.5, 19.5))
				usetool(33.5, 19.5);
			else if (searchren(33.5, 17.5))
				usetool(33.5, 17.5);
			else if (searchren(41.5, 27.5))
				usetool(41.5, 27.5);
			else if (searchren(40.5, 28.5))
				usetool(40.5, 28.5);
		}
		gettool();
		if (istool && istool <= 9)
			usetool();
		list<Obj> lt = MapInfo::get_mapcell(33, 18);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (int(itr->dish) <= 26 && int(itr->dish) >= 14)
			{

				glx = 33;
				gly = 18;
				//movetoa(33.5, 19.5);
				//movetoa(33.5, 18.5);
				movetoa(33.5, 20.5);
				if (istool >= 10 && istool <= 12)
					usetool();
				if (!isblock(33.5, 19.5))
				{
					move(Protobuf::Down, 1200.0 / PlayerInfo.moveSpeed);
					Sleep(1200.0 / PlayerInfo.moveSpeed);
				}
				else
				{
					movetoa(34.5, 17);
					move(Protobuf::Left, 1000.0 / PlayerInfo.moveSpeed);
					Sleep(1000.0 / PlayerInfo.moveSpeed);
					move(Protobuf::Direction::Up, 500/PlayerInfo.moveSpeed);	//防止有人占了这个位置。
					Sleep(500/PlayerInfo.moveSpeed);
				}
				goto jiancai;
			}
		}

		lt = MapInfo::get_mapcell(41, 28);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
		{
			if (int(itr->dish) <= 26 && int(itr->dish) >= 14)
			{
				glx = 41;
				gly = 28;
				//movetoa(41, 23);
				//movetoa(41.5, 28.5);
				/*movetoa(41, 23);
				//movetoa(41.5, 28.5);
				movetoa(41.5, 27.5);
				if (!isblock(41.5, 28.5))//防止占用。
					movetob(41.5, 28.5);
				else
				{
					movetoa(40.5, 29.5);
					move(Protobuf::Direction::Right, 0);
					Sleep(60);
				}*/
				movetoa(41, 23);
				if(istool>=10&&istool<=12)
				{
					movetoa(40.5, 26.5);
					usetool();
				}
				//movetoa(41.5, 28.5);
				movetoa(41.5, 26.5);
				if (!isblock(41.5, 27.5))//防止占用。
				{
					move(Protobuf::Up, 1200 / PlayerInfo.moveSpeed);
					Sleep(1200 / PlayerInfo.moveSpeed);
				}
				else
				{
					movetoa(40, 28.5);
					move(Protobuf::Direction::Right, 500 / PlayerInfo.moveSpeed);
					Sleep(500 / PlayerInfo.moveSpeed);
				}
				goto jiancai;
			}
		}
		if (MapInfo::get_mapcell(33, 18).size() >= 3)//之后检查菜是否很多
		{
			glx = 33;
			gly = 18;
			//movetoa(33, 19.5);
			movetoa(33.5, 20.5);//
			if (istool >= 10 && istool <= 12)
				usetool();
			//if (!isblock(33.5, 18.5))
			if(!isblock(33.5,19.5))//
			{
				move(Protobuf::Down, 1200.0 / PlayerInfo.moveSpeed);
				Sleep(1200.0 / PlayerInfo.moveSpeed);
			}
				//movetob(33.5, 18.5);
			else
			{
				movetoa(34.5, 17);
				move(Protobuf::Left, 1000.0 / PlayerInfo.moveSpeed);
				Sleep(1000.0 / PlayerInfo.moveSpeed);
				move(Protobuf::Direction::Up, 500 / PlayerInfo.moveSpeed);	//防止有人占了这个位置。
				Sleep(500 / PlayerInfo.moveSpeed);
			}
			use(0);
			Sleep(60);
			//Sleep(2000);
			goto jiancai;
		}
		if (MapInfo::get_mapcell(41, 28).size() >= 3)//之后检查菜是否很多
		{
			glx = 41;
			gly = 28;
			movetoa(41, 23);
			if (istool >= 10 && istool <= 12)
			{
				movetoa(40.5, 26.5);
				usetool();
			}
			//movetoa(41.5, 28.5);
			movetoa(41.5, 26.5);
			if (!isblock(41.5, 27.5))//防止占用。
			{
				move(Protobuf::Up, 1200 / PlayerInfo.moveSpeed);
				Sleep(1200 / PlayerInfo.moveSpeed);
			}
			else
			{
				movetoa(40, 28.5);
				move(Protobuf::Direction::Right, 500/PlayerInfo.moveSpeed);
				Sleep(500 / PlayerInfo.moveSpeed);
			}
			use(0);
			Sleep(60);
			goto jiancai;
		}
		
	}
jiancai:while(1)
	{
		list<Obj> lt = MapInfo::get_mapcell(glx, gly);
		for (list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)//在一个锅处先检查是否有做好的菜
			if (itr->objType==Block && int(itr->dish) <= 26 && int(itr->dish) >= 14)
			{
				pick(FALSE, Block, itr->dish);//捡到菜品
				Sleep(60);
				break;
			}
		if (PlayerInfo.dish != DishEmpty)
			break;
		else//是否没捡起来
		{
			for(list<Obj>::iterator itr = lt.begin(); itr != lt.end(); ++itr)
				if (itr->objType == Block && int(itr->dish) <= 26 && int(itr->dish) >= 14)
					goto jiancai;
		}
		if (MapInfo::get_mapcell(glx, gly).size() >= 3)//之后检查菜是否很多
		{
			use(0,0,0);
			//cout << "okleeeeeeeeeeeeeeeeeeeeeeeeeeeee" << endl;
			Sleep(50);
			return;
		}
		return;
		//if (MapInfo::get_mapcell(75 - PlayerInfo.position.x, 47 - PlayerInfo.position.y).size() > MapInfo::get_mapcell(PlayerInfo.position.x, PlayerInfo.position.y).size())//如果另一个锅的菜更多，就去另一处。
		//{
		//	movetoa(75 - PlayerInfo.position.x, 47 - PlayerInfo.position.y);
		//}
		//cout << "fshdjlkfjhsjdlkcbjskasldcbjshkdalcbvfsejkldcnbsd" << endl;
	}
	//if (int(PlayerInfo.dish) <= 26 && int(PlayerInfo.dish) >= 14)
	for(list<DishType>::iterator itr = task_list.begin();itr!=task_list.end();++itr)
		if (PlayerInfo.dish == *itr)
		{
			submit();//submit函数有返回原来位置的功能。
			flag = 0;
			break;
		}
	if(flag)
		put(10, 0, TRUE);//扔掉没用的。
}
