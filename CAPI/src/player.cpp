#define USER_ONLY
#include "API.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
using namespace THUAI3;
void play()
{
	Sleep(5000);
	std::cout << "Play!" << std::endl;
	print_obj_list();
	/*  玩家在这里写代码  */
}