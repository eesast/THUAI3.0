#include "API.h"//front change to another //only do simple dishes //kitchen dishes direction wrong //solve:why it wait so long to pick a dish?
#include "Constant.h"
#include "player.h"
#include <iostream>//daojude meixie
#include "OS_related.h"
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Cook;//指定人物天赋。选手代码必须定义此变量，否则报错
Protobuf::DishType object;//当前任务 //maybe several tasks
Protobuf::DishType food[5];//当前所需食材 
int i;
int flag = 1;//用作循环标志
int mapmatrix[50][50];//地图矩阵，由于地图不变，开一个矩阵存储一下本来的地图信息，原始地图信息见表格，左下角（0，0）左右为x，上下为y
static int z = 0;//一个初始的辅助变量，z=0时完成初始化
void showinfo()//输出当前信息
{
	cout << "x:" << PlayerInfo.position.x << "y:" << PlayerInfo.position.y << "direction:" << PlayerInfo.facingDirection << "score:" << PlayerInfo.score << PlayerInfo.dish << endl;
}
void moveto(int a, int b)//去某个地点的函数，内部的逻辑应当是显然的，写的时候注意点，别写到四面都有墙的地方，不然就死循环了。。。
{
	//if (PlayerInfo.position.x > 41 && PlayerInfo.position.x < 42 && PlayerInfo.position.y>19 && PlayerInfo.position.y < 20) { return; }
	if (mapmatrix[a][b] != 0)//该地是提交点产生点等不可以到达的地方，则到达它的旁边，然后调整朝向
	{
		if (mapmatrix[a][b + 1] == 0)//这个位置如果站人了就到不了了，会一直调用moveto函数，且到不了陷入死循环，下一版需要改进一下
		{
			moveto(a, b + 1);
			THUAI3::move(Down, 100);//调整方向
			Sleep(100);
			//showinfo(); Sleep(100);
			//此处应该判断当前位置有没有东西，有的话立刻捡起来用或者扔向远方，防止对捡起目标位置的东西产生影响
			return;
		}
		else if (mapmatrix[a][b - 1] == 0)
		{
			moveto(a, b - 1);
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			return;
		}
		else if (mapmatrix[a + 1][b] == 0)
		{
			moveto(a + 1, b);
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			return;
		}
		else if (mapmatrix[a - 1][b] == 0)
		{
			moveto(a - 1, b);
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			return;
		}
	}
	if (PlayerInfo.position.x > a && PlayerInfo.position.x<(a + 1) && PlayerInfo.position.y>b && PlayerInfo.position.y < (b + 1)) { return; }
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y < b)
	{

		if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右侧不是障碍物，右移一格，然后递归调用moveto
		{
			THUAI3::move(Right, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数，但是moveto函数不用
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//右侧有障碍物且上面没有障碍物，则上移到右侧没有障碍物为止再右移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
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
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y > b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右侧不是障碍物，右移一格，然后递归调用moveto
		{
			THUAI3::move(Right, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//右侧有障碍物且下面没有障碍物，则上移到右侧没有障碍物为止再右移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else//右侧下侧都有障碍物，向上移动到右边没有障碍再右移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y < b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//左侧不是障碍物，左移一格，然后递归调用moveto
		{
			THUAI3::move(Left, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//左侧有障碍物且上面没有障碍物，则上移到左侧没有障碍物为止再左移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else//左侧上侧都有障碍物，向下移动到左边没有障碍再左移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y > b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//左侧不是障碍物，左移一格，然后递归调用moveto
		{
			THUAI3::move(Left, 100);
			Sleep(100);//每次move之后一定要接上等长度的Sleep函数
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//左侧有障碍物且下面没有障碍物，则下移到左侧没有障碍物为止再左移一格，之后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else//左侧下侧都有障碍物，向上移动到左边没有障碍再左移，然后递归
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)PlayerInfo.position.y] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		//地图没有三面环绕着墙的地方，故而两种够用了，这种最终化归到x=a的情况
	}
	//经过以上的四种情况的递归，应当都可以划归到下面五种情况，此时x=a或者y=b
	else if ((int)PlayerInfo.position.x == a && (int)PlayerInfo.position.y < b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] != 0; i++)
			{
				THUAI3::move(Right, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] != 0; i++)
			{
				THUAI3::move(Left, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Up, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x == a && (int)PlayerInfo.position.y > b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] == 0)//下面没有障碍物
		{
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)PlayerInfo.position.y] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] != 0; i++)
			{
				THUAI3::move(Right, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y - 1)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y - 1)] != 0; i++)
			{
				THUAI3::move(Left, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Down, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x < a && (int)PlayerInfo.position.y == b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//右面没有障碍物
		{
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x + 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Right, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
	}
	else if ((int)PlayerInfo.position.x > a && (int)PlayerInfo.position.y == b)
	{
		if (mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] == 0)//左面没有障碍物
		{
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else if (mapmatrix[(int)(PlayerInfo.position.x)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x + 0.5)][(int)(PlayerInfo.position.y + 1)] == 0 && mapmatrix[(int)(PlayerInfo.position.x - 0.5)][(int)(PlayerInfo.position.y + 1)] == 0)//上面没有障碍物
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Up, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
		else
		{
			for (int i = 0; mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y + 0.5)] != 0 || mapmatrix[(int)(PlayerInfo.position.x - 1)][(int)(PlayerInfo.position.y - 0.5)] != 0; i++)
			{
				THUAI3::move(Down, 100);
				Sleep(100);
				//showinfo(); Sleep(100);
			}
			THUAI3::move(Left, 100);
			Sleep(100);
			//showinfo(); Sleep(100);
			moveto(a, b);
		}
	}
	else return;
}
int object_to_food()    //将任务所需的食材存入food数组中
{
	switch (object)
	{
	case 14:
		food[0] = (DishType)2;
		return 1;
	case 15:
		food[0] = (DishType)3;
		food[1] = (DishType)4;
		return 2;
	case 16:
		food[0] = (DishType)3;
		food[1] = (DishType)4;
		food[2] = (DishType)1;
		return 3;
	case 17:
		food[0] = (DishType)5;
		food[1] = (DishType)1;
		return 2;
	case 18:
		food[0] = (DishType)2;
		food[1] = (DishType)6;
		food[2] = (DishType)7;
		return 3;
	case 19:
		food[0] = (DishType)6;
		food[1] = (DishType)8;
		return 2;
	case 20:
		food[0] = (DishType)7;
		food[1] = (DishType)3;
		return 2;
	default:
		food[0] = (DishType)0;
		return 1;
	}
}
//下列moveto（42，40）均为前往食材生成点，moveto（41，18）均为前往灶台
//while循环内为捡相应的食材，put（1，true）为往灶台上放食材，use（0，0，0）为做菜
void play()
{
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
		////showinfo();Sleep(100);
		Sleep(100); 
		THUAI3::move(Up, 1000); Sleep(1000);//Sleep函数，100毫秒内不做操作，防止不满足帧率导致指令堆积起来不操作
		z++;
	}
	flag = 1;                                       //每次都将循环标志置为1
	object = task_list.back();//获取当前任务(这里需要改进，task_list中都是当前可执行的任务，后期需要加入多任务功能）//c++ list jiekou
	//object = (DishType)39;
	object_to_food();//前往指定食材生成点
	cout << "a:" <<object <<"b:"<< food[0] <<"c:"<< food[1];
	if (object == 14)
	{
		moveto(5, 5);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
	}

	else if (object == 15)
	{
		moveto(7, 41);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(25, 5);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(20000);
	}

	else if (object == 16)
	{
		moveto(7, 41);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(25, 5);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(20000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		moveto(42, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(4, 24);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[2])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(42, 28);
		THUAI3::pick(false, Block, 0); Sleep(100);
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(20000);
	}
	else if (object == 17)
	{
	    moveto(4, 24);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(31, 41);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
    }
	else if (object == 18)
	{
	    moveto(5, 5);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(42, 40);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(43, 6);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[2])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(20000);
    }
	else if (object == 19)
	{
	    moveto(42, 40);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(43, 25);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(20000);
    }
	else if (object == 20)
	{
	    moveto(7, 41);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[1])
			{
				flag = 0; break;
			}
		}
		flag = 1;
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(10000);
		THUAI3::pick(false, Block, 0); Sleep(100);
		THUAI3::put(1, 1.57, true); Sleep(100);
		moveto(43, 6);
		while (flag)
		{
			THUAI3::pick(false, Block, 0); Sleep(100);
			if (PlayerInfo.dish == food[0])
			{
				flag = 0; break;
			}
		}
		moveto(41, 28);
		THUAI3::put(1, 1.57, true); Sleep(100);
		THUAI3::use(0, 0, 0); Sleep(100);
		Sleep(15000);
    }
	
	//送菜
	THUAI3::pick(false, Block, 0);Sleep(100);
	if (PlayerInfo.dish != 28 && PlayerInfo.dish != 29)
	{
		moveto(25, 24); //去提交点
		THUAI3::use(0, 0, 0); Sleep(100); THUAI3::use(0, 0, 0); Sleep(100); THUAI3::use(0, 0, 0); Sleep(100);
	}
	else
	{
		THUAI3::put(5, 1.57, true); Sleep(100);
	}
}