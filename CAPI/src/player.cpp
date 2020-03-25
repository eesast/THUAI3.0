#include "API.h"
#include "player.h"
#include <iostream>
#include "CAPI.h"

using namespace THUAI3;
void play()
{
	Sleep(5000);
	std::cout << "Play!" << std::endl;
	MapInfo::print_obj_list();
	int x, y;
	std::cin >> x >> y;
	MapInfo::print_map(x, y);
	/*  玩家在这里写代码  */
}