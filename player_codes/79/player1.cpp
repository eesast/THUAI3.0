#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
using namespace THUAI3;
/*******************************队式Runner第一版********************************/
/*****************************第一版实现了什么内容******************************/
/*
moveto函数，基本操作（只有一队游戏时可以成功得到一些分数）
*/
/*****************************需要第二版修改的地方******************************/
/*
1地图问题：麻烦下一版帮我看看地图有没有标注错误，并且把观察到的陷阱和人加入mapmatrix
通信问题：还需要完善通信，确定一下接收到队友的信息应该做什么事以及各个事件的优先级
移动问题：moveto函数没有解决如何避开陷阱和人的算法，原因是利用预先设置的mapmatrix而并非实时利用视野
道具问题：pickanduse函数没有进行实现，也没有在实际算法中使用，下一版需要有道具就捡(不捡改速度的道具)
moveto函数问题1：如果站人了就到不了了，会一直调用moveto函数，陷入死循环，那个人不走我们也走不了
moveto函数问题2：只能解决方形地形，只能过2格及以上的缝隙，这样导致只能改地图矩阵，把所有障碍改成方形
本格道具影响捡生成点或者灶台东西的问题：判断当前位置有没有东西，有的话立刻捡起来用或者扔向远方，还没写
runner做菜的逻辑：runner本应辅助队友，但是当自己收集到的菜够了的时候也可以自己做菜提交，尚未实现
*/
Protobuf::Talent initTalent = Protobuf::Talent::Technician;//指定人物天赋。
int mapmatrix[50][50];//地图矩阵，由于地图不变，开一个矩阵存储一下本来的地图信息，原始地图信息见表格，左下角（0，0）左右为x，上下为y
static int z = 0;//一个初始的辅助变量，z=0时完成初始化
static int flag = 0;
void showinfo()//输出当前信息
{
	cout << "x:" << PlayerInfo.position.x << " y:" << PlayerInfo.position.y << " direction:" << PlayerInfo.facingDirection << " score:" << PlayerInfo.score << " dish:" << PlayerInfo.dish << " tool:" << PlayerInfo.tool << endl;
}
//pickanduse暂未实现，希望后续完善这个函数的定义，在每次move函数之后能补上这个函数
void pickanduse(int ToolType,int a,int b)//我们的策略是有道具立即使用，故设计此pickanduse函数，见到道具捡起来直接用（除了对自己进行速度增益的东西不要用，否则moveto会乱），用不了的撇了
{
	if (ToolType != 5 && ToolType != 8 && ToolType != 13 && ToolType != 14)
	{
		THUAI3::pick(0, Tool, ToolType); Sleep(100);
		THUAI3::use(1, 0, 0);	Sleep(100);
	}
	else if (ToolType == 5)
	{
		MapInfo::get_mapcell(a, b).clear();
		int i = MapInfo::get_mapcell(a, b).front().facingDiretion;
		THUAI3::put(1, i*3.1416/4, 0); Sleep(100);
		THUAI3::pick(0, Tool, ToolType); Sleep(100);
		THUAI3::use(1, 0, 0); Sleep(100);
		THUAI3::pick(1, Tool, ToolType); Sleep(100);
	}
	else if (ToolType == 13 || ToolType == 14)
	{
		THUAI3::pick(0, Tool, ToolType); Sleep(100);
		THUAI3::use(1, 5, 0); Sleep(100);
	}
	/*else if (ToolType == 8)
	{
		if(PlayerInfo.dish != 0)

	}
	*/
}
void seek(int a, int b)//获得周围1格视野
{
	/*if (MapInfo::get_mapcell(a + 1, b + 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a + 1, b + 1).front().trigger == 0)
			mapmatrix[a + 1][b + 1] = 7;//7代表胶水
		else
			mapmatrix[a + 1][b + 1] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a + 1, b + 1).clear();
	if (MapInfo::get_mapcell(a + 1, b + 1).front().objType == 0)//如果周围有人
		mapmatrix[a + 1][b + 1] = 9;//9代表有人
	else if (mapmatrix[a + 1][b + 1] >= 7)//如果这些东西消失了，地图恢复
		mapmatrix[a + 1][b + 1] = 0;

	/*if (MapInfo::get_mapcell(a + 1, b).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a - 1, b).front().trigger == 0)
			mapmatrix[a + 1][b] = 7;//7代表胶水
		else
			mapmatrix[a + 1][b] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a+1, b).clear();
	if (MapInfo::get_mapcell(a + 1, b).front().objType == 0)//如果周围有人
		mapmatrix[a + 1][b] = 9;//9代表有人
	else if (mapmatrix[a + 1][b] >= 7)
		mapmatrix[a + 1][b] = 0;

	/*if (MapInfo::get_mapcell(a + 1, b - 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a + 1, b - 1).front().trigger == 0)
			mapmatrix[a + 1][b - 1] = 7;//7代表胶水
		else
			mapmatrix[a + 1][b - 1] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a+1, b-1).clear();
	if (MapInfo::get_mapcell(a + 1, b - 1).front().objType == 0)//如果周围有人
		mapmatrix[a + 1][b - 1] = 9;//9代表有人
	else if (mapmatrix[a + 1][b - 1] >= 7)
		mapmatrix[a + 1][b - 1] = 0;

	/*if (MapInfo::get_mapcell(a, b - 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a, b - 1).front().trigger == 0)
			mapmatrix[a][b - 1] = 7;//7代表胶水
		else
			mapmatrix[a][b - 1] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a, b-1).clear();
	if (MapInfo::get_mapcell(a, b - 1).front().objType == 0)//如果周围有人
		mapmatrix[a][b - 1] = 9;//9代表有人
	else if (mapmatrix[a][b - 1] >= 7)
		mapmatrix[a][b - 1] = 0;

	/*if (MapInfo::get_mapcell(a - 1, b - 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a - 1, b - 1).front().trigger == 0)
			mapmatrix[a - 1][b - 1] = 7;//7代表胶水
		else
			mapmatrix[a - 1][b - 1] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a-1, b-1).clear();
	if (MapInfo::get_mapcell(a - 1, b - 1).front().objType == 0)//如果周围有人
		mapmatrix[a - 1][b - 1] = 9;//9代表有人
	else if (mapmatrix[a - 1][b - 1] >= 7)
		mapmatrix[a - 1][b - 1] = 0;

	/*if (MapInfo::get_mapcell(a - 1, b).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a - 1, b).front().trigger == 0)
			mapmatrix[a - 1][b] = 7;//7代表胶水
		else
			mapmatrix[a - 1][b] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a-1, b).clear();
	if (MapInfo::get_mapcell(a - 1, b).front().objType == 0)//如果周围有人
		mapmatrix[a - 1][b] = 9;//9代表有人
	else if (mapmatrix[a - 1][b] >= 7)
		mapmatrix[a - 1][b] = 0;

	/*if (MapInfo::get_mapcell(a - 1, b + 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a - 1, b + 1).front().trigger == 0)
			mapmatrix[a - 1][b + 1] = 7;//7代表胶水
		else
			mapmatrix[a - 1][b + 1] = 8;//8代表其他触发器
	else */
	MapInfo::get_mapcell(a-1, b+1).clear();
	if (MapInfo::get_mapcell(a - 1, b + 1).front().objType == 0)//如果周围有人
		mapmatrix[a - 1][b + 1] = 9;//9代表有人
	else if (mapmatrix[a - 1][b + 1] >= 7)
		mapmatrix[a - 1][b + 1] = 0;

	/*if (MapInfo::get_mapcell(a, b + 1).front().objType == 4)//如果周围有触发器
		if (MapInfo::get_mapcell(a, b + 1).front().trigger == 0)
			mapmatrix[a][b + 1] = 7;//7代表胶水
		else
			mapmatrix[a][b + 1] = 8;//8代表其他触发器
	else*/ 
	MapInfo::get_mapcell(a, b+1).clear();
	if (MapInfo::get_mapcell(a, b + 1).front().objType == 0)//如果周围有人
		mapmatrix[a][b + 1] = 9;//9代表有人
	else if (mapmatrix[a][b + 1] >= 7)
		mapmatrix[a][b + 1] = 0;
}
//moveto只能解决方形地形，故而把所有T型，L型的地形还有不规则地形全在mapmatrix矩阵中补成方形
void moveto(int a, int b)//去某个地点的函数，内部的逻辑应当是显然的，写的时候注意点，别写到四面都有墙的地方，不然就死循环了。。。
{
	if (MapInfo::get_mapcell(PlayerInfo.position.x, PlayerInfo.position.y).front().objType == 3)//如果当前地方有道具，捡起来用
		if (MapInfo::get_mapcell(PlayerInfo.position.x, PlayerInfo.position.y).front().tool > 4)
			pickanduse(MapInfo::get_mapcell(PlayerInfo.position.x, PlayerInfo.position.y).front().tool, PlayerInfo.position.x, PlayerInfo.position.y);
	//seek(PlayerInfo.position.x, PlayerInfo.position.y); Sleep(100);
	if (mapmatrix[a][b] != 0)//该地是提交点产生点等不可以到达的地方，则到达它的旁边，然后调整朝向
	{
		if (mapmatrix[a][b + 1] == 0)//这个位置如果站人了就到不了了，会一直调用moveto函数，且到不了陷入死循环，下一版需要改进一下
		{
			moveto(a, b + 1);
			while (PlayerInfo.facingDirection!=Down)
			{
				THUAI3::move(Down, 100);//调整方向
				Sleep(100);
				//showinfo();Sleep(100);
			}
			//此处应该判断当前位置有没有东西，有的话立刻捡起来用或者扔向远方，防止对捡起目标位置的东西产生影响
			return;
		}
		else if (mapmatrix[a][b - 1] == 0)
		{
			moveto(a, b - 1);
			while (PlayerInfo.facingDirection != Up)
			{
				THUAI3::move(Up, 100);//调整方向
				Sleep(100);
				//showinfo();Sleep(100);
			}
			return;
		}
		else if (mapmatrix[a + 1][b] == 0)
		{
			moveto(a + 1, b);
			while (PlayerInfo.facingDirection != Left)
			{
				THUAI3::move(Left, 100);//调整方向
				Sleep(100);
				//showinfo();Sleep(100);
			}
			return;
		}
		else if (mapmatrix[a - 1][b] == 0)
		{
			moveto(a - 1, b);
			while (PlayerInfo.facingDirection != Right)
			{
				THUAI3::move(Right, 100);//调整方向
				Sleep(100);
				//showinfo();Sleep(100);
			}
			return;
		}
	}
	if (PlayerInfo.position.x > a && PlayerInfo.position.x<(a + 1) && PlayerInfo.position.y>b && PlayerInfo.position.y < (b + 1)) { return; }
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y < b)
	{
		
		if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//右侧不是障碍物，右移一格，然后递归调用moveto
		{
			THUAI3::move(Right, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数，但是moveto函数不用
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//右侧有障碍物且上面没有障碍物，则上移到右侧没有障碍物为止再右移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			moveto(a, b);
		}
		else//右侧上侧都有障碍物，向下移动到右边没有障碍再右移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y > b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//右侧不是障碍物，右移一格，然后递归调用moveto
		{
			THUAI3::move(Right, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0&&mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y - 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//右侧有障碍物且下面没有障碍物，则上移到右侧没有障碍物为止再右移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0||mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] != 0|| mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else//右侧下侧都有障碍物，向上移动到右边没有障碍再右移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y < b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//左侧不是障碍物，左移一格，然后递归调用moveto
		{
			THUAI3::move(Left, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//左侧有障碍物且上面没有障碍物，则上移到左侧没有障碍物为止再左移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else//左侧上侧都有障碍物，向下移动到左边没有障碍再左移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y > b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//左侧不是障碍物，左移一格，然后递归调用moveto
		{
			THUAI3::move(Left, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y - 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//左侧有障碍物且下面没有障碍物，则下移到左侧没有障碍物为止再左移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else//左侧下侧都有障碍物，向上移动到左边没有障碍再左移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	//经过以上的四种情况的递归，应当都可以划归到下面五种情况，此时x=a或者y=b
	else if ((int)PlayerInfo.position.x == a && (int)PlayerInfo.position.y < b)
	{
	    if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if(mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] != 0|| mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] != 0|| mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] != 0; i++)
			{
				THUAI3::move(Right, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] != 0; i++)
			{
				THUAI3::move(Left, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x == a && (int)PlayerInfo.position.y > b)
	{
	    if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y - 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//下面没有障碍物
		{
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if(mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//右面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] != 0|| mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y - 1)] != 0|| mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y - 1)] != 0; i++)
			{
				THUAI3::move(Right, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] != 0; i++)
			{
				THUAI3::move(Left, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y == b)
	{
	    if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//右面没有障碍物
		{
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if(mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] != 0|| mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y+0.5)] != 0|| mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y-0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y == b)
	{
	    if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] == 0&& mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] == 0)//左面没有障碍物
		{
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else if(mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x+0.5)][(int)(PlayerInfo.position.y + 1)] == 0&& mapmatrix[(int)(PlayerInfo.position.x-0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y+0.5)] != 0|| mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y-0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo();Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo();Sleep(100);
			moveto(a, b);
		}
	}
	else return;
}
void play()
{
	/*while (TRUE)
	{
		Sleep(1000);
	}*/
	if (z == 0)//初始化，仅当开始时z=0时执行，初始化地图数组，显示当前信息并休息100秒
	{
		for (int i = 0; i < 50; i++)//这个for循环到showinfo之前都是用来初始化地图数组
		{
			for (int j = 0; j < 50; j++)
			{
				mapmatrix[i][j] = 0;
			}
		}
		for (int i = 0; i < 50; i++)
		{
			mapmatrix[i][0] = 5;//5代表高墙
			mapmatrix[i][49] = 5;
		}
		for (int j = 0; j < 50; j++)
		{
			mapmatrix[0][j] = 5;
			mapmatrix[49][j] = 5;
		}
		for (int i = 40; i <= 45; i++)
		{
			mapmatrix[i][12] = 5;
		}
		for (int j = 31; j <= 39; j++)
		{
			mapmatrix[33][j] = 5;
		}
		for (int j = 9; j <= 15; j++)
		{
			mapmatrix[29][j] = 5;
		}
		for (int i = 26; i <= 28; i++)
		{
			for (int j = 18; j <= 20; j++)
			{
				mapmatrix[i][j] = 5;
			}
		}
		for (int j = 15; j <= 18; j++)
		{
			mapmatrix[18][j] = 5;
		}
		for (int i = 12; i <= 17; i++)
		{
			mapmatrix[i][29] = 5;
		}
		for (int i = 7; i <= 10; i++)
		{
			for (int j = 9; j <= 14; j++)
			{
				mapmatrix[i][j] = 5;
			}
		}
		for (int i = 18; i <= 25; i++)
		{
			for (int j = 43; j <= 46; j++)
			{
				mapmatrix[i][j] = 5;
			}
		}
		mapmatrix[25][9] = 5; mapmatrix[25][10] = 5; mapmatrix[24][9] = 5; mapmatrix[24][10] = 5; mapmatrix[26][9] = 5; mapmatrix[26][10] = 5;
		mapmatrix[35][26] = 5; mapmatrix[36][27] = 5; mapmatrix[37][28] = 5; mapmatrix[7][24] = 5;
		for (int i = 35; i <= 38; i++)
		{
			for (int j = 24; j <= 27; j++)
			{
				mapmatrix[i][j] = 5;
			}
		}
		for (int i = 24; i <= 26; i++)
		{
			for (int j = 31; j <= 35; j++)
			{
				mapmatrix[i][j] = 6;
			}
		}
		for (int i = 32; i <= 34; i++)
		{
			for (int j = 17; j <= 19; j++)
			{
				mapmatrix[i][j] = 6;
			}
		}
		mapmatrix[24][38] = 6; mapmatrix[25][39] = 6; mapmatrix[26][39] = 6;
		mapmatrix[7][23] = 6; mapmatrix[8][23] = 6; mapmatrix[9][23] = 6; mapmatrix[9][24] = 6; mapmatrix[9][32] = 6;//6表示桌子
		mapmatrix[16][11] = 6; mapmatrix[16][12] = 6; mapmatrix[17][11] = 6; mapmatrix[17][12] = 6;
		mapmatrix[15][24] = 6; mapmatrix[15][25] = 6; mapmatrix[16][38] = 6; mapmatrix[17][38] = 6; mapmatrix[18][38] = 6;
		mapmatrix[24][39] = 6; mapmatrix[24][31] = 6; mapmatrix[24][32] = 6; mapmatrix[24][33] = 6; mapmatrix[24][34] = 6; mapmatrix[24][35] = 6;
		mapmatrix[25][31] = 6; mapmatrix[26][31] = 6; mapmatrix[26][38] = 6; mapmatrix[27][18] = 6; mapmatrix[27][19] = 6;
		mapmatrix[31][26] = 6; mapmatrix[32][26] = 6; mapmatrix[31][27] = 6; mapmatrix[32][27] = 6;
		mapmatrix[32][17] = 6; mapmatrix[32][18] = 6; mapmatrix[32][19] = 6; mapmatrix[34][18] = 6;
		mapmatrix[40][17] = 6; mapmatrix[41][29] = 6; mapmatrix[42][29] = 6; mapmatrix[42][28] = 6; mapmatrix[45][35] = 6;
		mapmatrix[24][24] = 1; mapmatrix[24][25] = 1; mapmatrix[25][24] = 1; mapmatrix[25][25] = 1;//1代表食物提交点
		mapmatrix[7][41] = 2; mapmatrix[25][5] = 2; mapmatrix[42][40] = 2;; mapmatrix[4][24] = 2; //2代表食物产生点
		mapmatrix[5][5] = 2; mapmatrix[43][6] = 2; mapmatrix[43][25] = 2;; mapmatrix[31][41] = 2; //2代表食物产生点
		mapmatrix[8][24] = 3; mapmatrix[25][38] = 3; mapmatrix[33][18] = 3; mapmatrix[41][28] = 3;//3代表工作台
		mapmatrix[12][19] = 4; mapmatrix[15][32] = 4; mapmatrix[25][13] = 4; mapmatrix[36][35] = 4; mapmatrix[38][24] = 4;//4代表垃圾桶
		//后续还会用7表示各类陷阱，遇到各种情况随时更新地图矩阵并告知队友
		//我们采取只移动而不扔东西的简单策略，故而5和6，即墙壁和桌子等效，都需要绕开走
		//showinfo();Sleep(100);
		flag = 6;
		Sleep(5000);//Sleep函数，100毫秒内不做操作，防止不满足帧率导致指令堆积起来不操作
		z++;
	}
	if (PlayerInfo.recieveText!="")//如果队友发信息，立刻进行处理
	{
        
	}
	//如果当前位置有道具，就立刻使用，用elseif，此处需要完善
	else if (flag == 1)//如果食物产生点有食材就拿，没食材就等，拿到食材更新手中的食材数组
	{
		moveto(7, 41);//去往一个食物产生点
		while (flag)//捡食材
		{
			THUAI3::pick(0, Block, 0);
			Sleep(100);
			if (PlayerInfo.dish != 0) { flag = 2; break; }
		}
	}
	else if (flag == 2)//如果手里拿到了食材，对应动作为去往约定的放置食材点
	{
		moveto(25, 24);//去指定灶台旁边
		THUAI3::put(0, 0, true);//放下食材
		Sleep(100);
		//THUAI3::speakToFriend("get");//通知队友，此处需要完善
		flag = 6;//如果加入做菜，此处改成flag=3;
	}
	//如果食材列表里面满足做菜需求，则取菜做菜，不符合就烧杀劫掠，后续可以加上
	/*else if (flag == 3)
	{
		if ()
		{
			flag = 4;
		}
		else flag = 6;
	}
	else if (flag == 4)//做菜过程中呆在灶台旁边
	{
		THUAI3::use(0, 0, 0);
		Sleep(10000);
		flag = 5;
	}
	else if (flag == 5)//如果做菜快完毕则取菜提交
	{

		flag = 6;
	}*/
	else//以上情况均不满足时，开始进行一套烧杀劫掠的操作，然后回到flag=1的状态，继续收集材料
	{
		moveto(25, 5);//去往一个自己和队友都不用的食物产生点捣乱
		while (flag)//捡食材
		{
			THUAI3::pick(false, Block, 0);Sleep(100);
			//showinfo(); Sleep(100);
			if (PlayerInfo.dish == 4) break;
		}
		//THUAI3::speakToFriend("get");//通知队友，此处需要完善
		moveto(8, 24);
		THUAI3::put(1, 4.71, true);//放下食材
		THUAI3::use(0, 0, 0);
		Sleep(100);
		moveto(5, 5);//去往一个自己和队友都不用的食物产生点捣乱
		while (flag)//捡食材
		{
			THUAI3::pick(false, Block, 0);
			Sleep(100);
			//showinfo(); Sleep(100);
			if (PlayerInfo.dish == 2) break;
		}
		moveto(25, 38);//去其他人的工作台，企图全都搞成黑暗料理
		THUAI3::put(1, 1.57, true);//放下食材
		THUAI3::use(0, 0, 0);
		Sleep(100);
		//moveto(33, 18);(33,18)不能去，moveto会陷入死循环！
		moveto(25, 38);//搞完一通再去其他人的工作台抢菜，如果工作台里啥也没有就走人
		int k1 = 0;
		while (flag&&k1<=20)//捡菜
		{
			THUAI3::pick(false, Block, 0);Sleep(100);
			//showinfo(); Sleep(100);
			k1++;
		}
		if (PlayerInfo.dish != 0 && PlayerInfo.dish != 29 && PlayerInfo.dish != 28)//偷菜成功且不是黑暗料理，就去提交点提交
		{
			moveto(25, 24);
			THUAI3::use(0, 0, 0); Sleep(100);
			//showinfo(); Sleep(100);
		}
		else if (PlayerInfo.dish == 29 || PlayerInfo.dish == 28)
		{
			THUAI3::put(0, 0, true);
			Sleep(100);
		}
		moveto(8, 24);
		k1 = 0;
		while (flag&&k1<=20)//捡菜
		{
			THUAI3::pick(false, Block, 0); 
			Sleep(100);
			//showinfo(); Sleep(100);
			k1++;
		}
		if (PlayerInfo.dish != 0 && PlayerInfo.dish != 29 && PlayerInfo.dish != 28)//偷菜成功且不是黑暗料理，就去提交点提交
		{
			moveto(25, 24);
			THUAI3::use(0, 0, 0); 
			Sleep(100);
		}
		else if (PlayerInfo.dish == 29 || PlayerInfo.dish == 28)
		{
			THUAI3::put(0, 0, true);
			Sleep(100);
		}
		flag = 1;
	}
}