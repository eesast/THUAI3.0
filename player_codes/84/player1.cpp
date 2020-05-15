#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
#include <cmath>
#include <time.h>
#include <thread>
#include <string>
using namespace THUAI3;

int themap[50][50] = {
	{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 6, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 5, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 6, 6, 6, 6, 6, 0, 0, 0, 6, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 2, 0, 0, 0, 5, 5, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 5, 5, 5, 5, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 6, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5},
	{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}
};
struct DishProperty
{
	int Score;
	int CookTime;
	int TaskTime;
};
const static std::map<int, DishProperty> DishInfo =
{
{int(Protobuf::Wheat), {0,0,0} },
{int(Protobuf::Rice), {0,0,0} },
{int(Protobuf::Tomato), {0,0,0} },
{int(Protobuf::Egg), {0,0,0} },
{int(Protobuf::Beef), {20,0,0} },
{int(Protobuf::Pork), {20,0,0} },
{int(Protobuf::Potato), {20,0,0} },
{int(Protobuf::Lettuce), {20,0,0} },
{int(Protobuf::Flour), {0,10000,0} },
{int(Protobuf::Noodle), {20,10000,0} },
{int(Protobuf::Bread), {0,10000,0} },
{int(Protobuf::Ketchup), {0,10000,0} },
{int(Protobuf::CookedRice), {20,10000,40000} },
{int(Protobuf::TomatoFriedEgg), {40,20000,60000} },
{int(Protobuf::TomatoFriedEggNoodle), {85,20000,120000} },
{int(Protobuf::BeefNoodle), {60,20000,120000} },
{int(Protobuf::OverRice), {60,20000,90000} },
{int(Protobuf::Barbecue), {40,20000,60000} },
{int(Protobuf::FrenchFries), {45,15000,75000} },
{int(Protobuf::Hamburger), {80,20000,120000} },
{int(Protobuf::SpicedPot), {0,60000,300000} },
{int(Protobuf::DarkDish), {-10,5000,300000} },
{int(Protobuf::OverCookedDish), {-10,5000,300000} },
{int(Protobuf::CookingDish), {-10,5000,300000} },
};
const static std::unordered_map<int, std::list<Protobuf::DishType>> CookingTable =
{
{int(Protobuf::Flour), {Protobuf::Wheat} },
{int(Protobuf::Noodle), {Protobuf::Flour} },
{int(Protobuf::Bread), {Protobuf::Egg,Protobuf::Flour} },
{int(Protobuf::CookedRice), {Protobuf::Rice} },
{int(Protobuf::Ketchup), {Protobuf::Tomato} },
{int(Protobuf::TomatoFriedEgg), {Protobuf::Tomato,Protobuf::Egg} },
{int(Protobuf::TomatoFriedEggNoodle), {Protobuf::Noodle,Protobuf::TomatoFriedEgg} },
{int(Protobuf::BeefNoodle), {Protobuf::Beef,Protobuf::Noodle} },
{int(Protobuf::OverRice), {Protobuf::Rice,Protobuf::Pork,Protobuf::Potato} },
{int(Protobuf::Barbecue), {Protobuf::Pork,Protobuf::Lettuce} },
{int(Protobuf::FrenchFries), {Protobuf::Potato,Protobuf::Ketchup} },
{int(Protobuf::Hamburger), {Protobuf::Beef,Protobuf::Lettuce,Protobuf::Bread} },
};

struct my
{
	int CP[2] = { 0 };	//��⿵ص�
	int mark_state = 0;
	int Another_state = 0;
	int aim;	//Ŀ��ֵ
	int attr;	//aim������
	int CookingDish[2] = { 0 };	//��⿲�Ʒ
	int StartTime[2] = { 0 };
	int AnotherHand = 0;
	char ToSend[16] = { 0 };
	long long LastSend = 0;
	int ComState = 1;
	int Force = 0;
}My;

int StorePlace[10][4] = { {15, 1, 1},{35, 1, 1},{48, 17, 1},{48, 48, 1},{17, 48, 1}, {1, 35, 1},{36, 48, 1}, {43, 11, 0},{8, 15, 0} };	//���꣬�Ƿ���ã�����ʱ��
const int CookPlace[4][2] = { {8, 24},{25, 38},{41, 28},{33, 18} };
const int FoodPlace[8][2] = { {5, 5},{43, 6},{4, 24},{25, 5},{43, 25},{7, 41}, {31, 41}, {42, 40} };
int SP[10][10] = { 0 };	//�����ʳ��
int Time0[10] = { 0 };
int Time1[4] = { 0 };	//Cook
int Time2[8] = { 0 };
int Time3[4] = { 0 };
int pre[5][2] = { 0 };
int TaskTime[9][6] = { 0 };	//��һ����ʾ�ظ�����
int mark[22][4] = { 0 };	//�����ԣ�ƫ��ֵ����������ʣ��ʱ�䣬ע���ȱ��Ʒ
const int step = 10000;
char Normal[1000][16];
char Abnormal[1000][16];
char NormalSend[1000][16];
char AbnormalSend[1000][16];
int AllFood[23] = { 0 };
int Dicision[3][10] = { 0 };
int N = -1, AN = -1, NS = 0, AS = 0;
/**************************************/
Protobuf::Talent initTalent = Protobuf::Talent::Runner;//ָ�������츳��ѡ�ִ�����붨��˱��������򱨴�
int gotoxy(double, double, int type = 0);
int gotoxy0(double, double, int Clear = 0);
int gotoxy1(int, int, int NG = 0);
int gotoxy2(double, double);
int Check0();
int Check1();
enum Protobuf::Direction random(int a);
void Time(int _time);
int Check(int a = 0);
void ClearP();
void ClearD();
int ClearTable(int n = 0, int* able = 0);
void Insert(int dish, int score);
void _move(Direction direction_t, int duration);
void _put(double distance, double angle, bool isThrowDish);
void _use(int type, double parameter1, double parameter2);
void _pick(bool isSelfPosition, ObjType _pickType, int dishOrToolType);
void _speakToFriend(string speakText);
void AutoPick(int type = 0, int prior = 0);
void Dicide();
void FlushTask();
void FlushFood();
void _FlushTask();
void _GetMessage();
void _FlushMark();
void FlushPlace();
void ClearCook(int i = 0);
void Order(int n);
int State0();
int State1();
int State2();
int State3();
int FindDish(int dish);
int SearchNear0(int type = 0);
int SearchNear1();
int SearchNear2();
int SearchNear3();
int CalNum(int place);
void CookPut(int num);
void CookPick(int num);
void FlushSend();
void FlushAim();
void FlushHand();
void WriteHead();
void ZTH(int n);
int Read(int letter);
void Read101(int letter);
void Read102(int letter);
void Read110(int letter);
void Read112(int letter);
void Read103(int letter);
void C0();
void C1();
void C2();
void C3();
void C4();
void C5();
void C6();
void C7();
void C8();
void C9();
void C10();
void C11();
void C12();
void C13();
void C14();
void C15();
void C16();
void C17();
void C18();
void C19();
void C20();
void C21();
void C22();
void D0(int flag = 0);
void D1(int flag = 0);
void D2(int flag = 0);
void D3(int flag = 0);
void D4(int flag = 0);
void D5(int flag = 0);
void D6(int flag = 0);
void D7(int flag = 0);
void D8(int flag = 0);
void D9(int flag = 0);
void D10(int flag = 0);
void D11(int flag = 0);
void D12(int flag = 0);
void D13(int flag = 0);
void D14(int flag = 0);
void D15(int flag = 0);
void D16(int flag = 0);
void D17(int flag = 0);
void D18(int flag = 0);
void D19(int flag = 0);
void D20(int flag = 0);
void D21(int flag = 0);
void D22(int flag = 0);

void (*C[23])() = { C0,C1,C2,C3,C4,C5,C6,C7,C8,C9,C10,C11,C12,C13, C14, C15, C16, C17, C18, C19, C20, C21, C22 };
void (*D[23])(int flag) = { D0,D1,D2,D3,D4,D5,D6,D7,D8,D9,D10,D11,D12,D13, D14, D15, D16, D17, D18, D19, D20, D21, D22 };

//0ͨ�����⣬ͨ�ż�����
void play()
{
	//static pthread_t thrd1, thrd2, thrd3;
	std::thread thrd1(_FlushTask);
	std::thread thrd2(_GetMessage);
	std::thread thrd3(_FlushMark);
	static int flag = 1;
	if (flag)
	{
		std::cout << endl;
		flag = 0;
		/*
		while (pthread_create(&thrd1, NULL, _FlushTask, NULL))
		{
			std::cout << "Error!" << endl;
			Sleep(500);
		}
		while (pthread_create(&thrd2, NULL, _GetMessage, NULL))
		{
			std::cout << "Error!" << endl;
			Sleep(500);
		}
		while (pthread_create(&thrd3, NULL, _FlushMark, NULL))
		{
			std::cout << "Error!" << endl;
			Sleep(500);
		}
		*/
		My.ToSend[0] = 100;
		My.ToSend[1] = 100;
		My.ToSend[2] = 0;
		FlushSend();
		//getchar();
		themap[5][5] = 2;
		themap[43][6] = 2;
		themap[4][24] = 2;
		themap[25][5] = 2;
		themap[43][25] = 2;
		themap[7][41] = 2;
		themap[31][41] = 2;
		themap[42][40] = 2;
	}
	static long long start = getGameTime();
	while (1)
	{
		if (My.mark_state == 0)
			while (State0());
		if (My.mark_state == 1)
			while (State1());
		if (My.mark_state == 3)
			while (State3());
	}
}

int State0()
{
	cout << "State0!" << endl;
	int state;
	static int trytime = 0;
	if (My.Force == 1)
	{
		cout << "Force111!" << endl;
		My.Force = 0;
		return 0;
	}
	if (PlayerInfo.dish)
	{
		My.aim = SearchNear0();
		My.attr = 0;
		while (1)
		{
			if (!PlayerInfo.dish)
				return 0;
			if (My.Force == 1)
			{
				cout << "Force111!" << endl;
				My.Force = 0;
				return 0;
			}
			state = gotoxy2(StorePlace[My.aim][0] + 0.5, StorePlace[My.aim][1] + 0.5);
			//printf("%d", PlayerInfo.dish);
			//std::cout << "##\n";
			//std::cout << My.aim << endl;
			//std::cout << StorePlace[My.aim][0] + 0.5 << ',' << StorePlace[My.aim][1] + 0.5 << endl;
			//getchar();
			if (state == 1)
			{
				trytime = 0;
				_put(0, 0, true);
				//Sleep(10000);
				//FlushPlace();
				//getchar();
				return 1;
			}
			else if (state < 0)
			{
				cout << "Unexpected!" << endl;
				trytime++;
				gotoxy0(0, 0, 2);
				if (trytime >= 3)
				{
					trytime = 0;
					Time0[My.aim] = getGameTime();
					return 0;
				}
				else
					return -1;
			}
		}
	}
	else
	{
		My.aim = SearchNear2();
		My.attr = 2;
		while (1)
		{
			if (PlayerInfo.dish)
				return 0;
			if (My.Force == 1)
			{
				My.Force = 0;
				return 0;
			}
			state = gotoxy1(FoodPlace[My.aim][0], FoodPlace[My.aim][1]);
			//std::cout << My.aim;
			//std::cout << "$$\n";
			//getchar();
			if (state == 1)
			{
				trytime = 0;
				_pick(false, Block, 0);
				//getchar();
				return 1;
			}
			else if (state < 0)
			{
				trytime++;
				gotoxy0(0, 0, 2);
				if (trytime >= 3)
				{
					trytime = 0;
					Time2[My.aim] = getGameTime();
					return 0;
				}
				else
					return -1;
			}
		}
	}
}

int State1()
{
	int end, i;
	int state;
	cout << "State1!!!" << endl;
	static int trytime = 0;
	if (My.Force == 1)
	{
		cout << "Force";
		My.Force = 0;
		return 0;
	}
	for (end = 0;; end++)
	{
		if (Dicision[0][end] == -1)
			break;
	}
	if (end == 0)
	{
		My.mark_state = 0;
		return 0;
	}
	if (My.CookingDish[0])
	{
		cout << "Cooking";
		if (getGameTime() - My.StartTime[0] < DishInfo.find(My.CookingDish[0])->second.CookTime + 100)
			Sleep(10);
		else
		{
			My.CookingDish[0] = 0;
			My.StartTime[0] = 0;
			My.CP[0] = 0;
			CookPick(My.aim);
		}
		return 1;
	}
	for (i = end - 1; i != 0; i--)
	{
		if (FindDish(Dicision[0][i]) != -1)
			break;
	}
	if (PlayerInfo.dish == Dicision[0][i])
	{
		if (i == end - 1)
		{
			cout << "Finished";
			ClearD();
			My.mark_state = 0;
			return 0;
		}
		My.attr = 1;
		My.aim = SearchNear1();
		if (My.aim == -1)
		{
			Sleep(50);
			My.aim = 0;
			return 0;
		}
		state = gotoxy1(CookPlace[My.aim][0], CookPlace[My.aim][1]);
		if (state == 1)
		{
			cout << "Start";
			trytime = 0;
			int able[1] = { Dicision[0][i] };
			if (ClearTable(1, able) == 0)
			{
				My.mark_state = 0;
				ClearCook();
				return 0;
			}
			CookPut(My.aim);
			_use(0, 0, 0);
			My.CP[0] = My.aim;
			My.CookingDish[0] = Dicision[0][i + 1];
			My.StartTime[0] = getGameTime();
			return 1;
		}
		else if (state < 0)
		{
			trytime++;
			if (trytime >= 3)
			{
				cout << "Fuck";
				trytime = 0;
				Time2[My.aim] = getGameTime();
				return 0;
			}
			else
			{
				My.mark_state = 0;
				return -1;
			}
		}
		else
			return 0;
	}
	else if (PlayerInfo.dish)
	{
		return State0();
	}
	else
	{
		cout << "is Here?";
		int place = FindDish(Dicision[0][i]);
		if (place == 100)
			return 1;
		else if (place == -1)
		{
			cout << "Amazing!";
			My.mark_state = 0;
			return State0();
		}
		else
		{
			state = gotoxy2(StorePlace[place][0] + 0.5, StorePlace[place][1] + 0.5);
			cout << "State:" << state << endl;
			if (state < 0)
			{
				trytime++;
				gotoxy0(0, 0, 2);
				if (trytime >= 3)
				{
					trytime = 0;
					return 0;
				}
				return -1;
			}
			else if (state == 0)
				return 0;
			else if (state == 1)
			{
				cout << PlayerInfo.position.x << ',' << PlayerInfo.position.y << endl;
				cout << place << endl << endl;
				cout << (StorePlace[place][0] + 0.5) << ',' << (StorePlace[place][1] + 0.5) << endl;
				_pick(true, Dish, Dicision[0][i]);
				if (PlayerInfo.dish != Dicision[0][i])
					return 0;
				else
					return 1;
			}
			else
				return 0;
		}
	}
}

int State2()
{
	cout << "State2!" << endl;
	if (My.Force == 1)
	{
		cout << "Force22222222222222";
		My.Force = 0;
		return 0;
	}
	return 0;
}

int State3()
{
	static int trytime = 0;
	int state;
	int place;
	cout << "State3!State3!State3!State3!State3!State3!State3!State3!" << endl;
	if (My.Force == 1)
	{
		cout << "Force33333333333333";
		My.Force = 0;
		return 0;
	}
	cout << "Check33333" << endl;
	cout << "Dicision[0][0]!!!" << Dicision[0][0] << endl;
	int temp = Dicision[0][0];
	if (!temp)
	{
		Sleep(5);
		return 0;
	}
	place = FindDish(Dicision[0][0]);
	if (place == -1)
	{
		cout << "Here?Kidding?" << endl;
		return 0;
	}
	else if (PlayerInfo.dish != Dicision[0][0])
	{
		cout << "Here?ahhhh?" << endl;
		if (place == 100)
		{
			cout << "WEIIIIIIIIIIIIIIII!" << endl;
			return 0;
		}
		state = gotoxy2(StorePlace[place][0] + 0.5, StorePlace[place][1] + 0.5);
		if (state < 0)
		{
			trytime++;
			gotoxy0(0, 0, 2);
			if (trytime >= 3)
			{
				trytime = 0;
				return 0;
			}
			return -1;
		}
		else
			return 1;
	}
	else
	{
		cout << "Goinggggggg!" << endl;
		int a[4][2] = { {25,24},{25,25},{26,24},{26,25} };
		int place = SearchNear3();
		state = gotoxy1(a[place][0], a[place][1]);
		if (state < 0)
		{
			trytime++;
			gotoxy0(0, 0, 2);
			if (trytime >= 3)
			{
				Time3[place] = getGameTime();
				trytime = 0;
				return 0;
			}
			return -1;
		}
		else if (state == 1)
		{
			_use(0, 0, 0);
			ClearD();
			return 2;
		}
		else
			return 1;
	}
}

int gotoxy0(const double x, const double y, int Clear)
{
	//clock_t start, end;
	double a, b;//Ŀǰλ��
	static double j = -1, k = -1;//�Ƚ��Ƿ�ס
	int dist = 500 / PlayerInfo.moveSpeed;//�ƶ�ʱ��
	static int trytime = 0;//���Դ���
	static int _trytime = 0;
	double judge = 1;//ֱ����б��ѡ���ж�������
	static int count = 0;
	static Direction dir;
	static Direction last_dir;
	static Direction aim;
	static int int_aim;
	static int int_dir;
	static double i = 1;
	int special = 0;
	static int times = 0;
	static bool first = true;
	//int limittime = 6;
	//static int fail_time = 0;
	//static int i_count = 0;
	//static int trytime_count = 0;
	static int dir_time = 0;
	static bool stuck_flag = false;
	static int state;
	if (Clear == 1)
		goto _Clear;
	if (Clear == 2)
		goto REALRAN;
	dir = PlayerInfo.facingDirection;
	a = PlayerInfo.position.x;
	b = PlayerInfo.position.y;
	if (abs(a - x) < 1 && abs(b - y) < 1)
	{
	_Clear:
		i = 1;
		j = k = -1;
		//i_count = 0;
		//trytime_count = 0;
		//fail_time = 0;
		trytime = 0;
		_trytime = 0;
		count = 0;
		stuck_flag = false;
		times = 0;
		first = true;
		dir_time = 0;
		//std::cout << "Success!" << endl;
		return 1;
	}
	//getchar();
	//std::cout << "(a,b): " << a << ',' << b << endl;
	//std::cout << "Stuck_flag is :" << stuck_flag << endl;
	if (((abs(a - j) > 0.0001) || (abs(b - k) > 0.0001)) && !stuck_flag)//û����ס
	{
		//std::cout << "Mei Bei Ka!" << endl;
		j = a;
		k = b;
		//getchar();
		if (a < x && abs(b - y) < judge)//��
		{
			dir = Right;
			_move(Right, dist);
		}
		else if (a > x && abs(b - y) < judge)//��
		{
			dir = Left;
			_move(Left, dist);
		}
		else if (abs(a - x) < judge && b < y)//��
		{
			dir = Up;
			_move(Up, dist);
		}
		else if (abs(a - x) < judge && b > y)//shang
		{
			dir = Down;
			_move(Down, dist);
		}
		else if (a < x && b < y)//��������
		{
			dir = RightUp;
			_move(RightUp, dist);
		}
		else if (a < x && b > y)//����
		{
			dir = RightDown;
			_move(RightDown, dist);
		}
		else if (a > x && b < y)//����
		{
			dir = LeftUp;
			_move(LeftUp, dist);
		}
		else if (a > x && b > y)//����
		{
			dir = LeftDown;
			_move(LeftDown, dist);
		}
		Time(dist);
		a = PlayerInfo.position.x;
		b = PlayerInfo.position.y;
		if (count >= 4)
		{
			//std::cout << "Reset!" << endl;
			//getchar();
			i = 1;
			//fail_time = 0;
			//i_count = 0;
			//trytime_count = 0;
			trytime = 0;
			times = 0;
			stuck_flag = false;
		}
		else if (count < 4 && ((abs(a - j) > 0.0001) || (abs(b - k) > 0.0001)))
			count++;
		return 0;
	}
	else //����ס
	{
		//std::cout << "Bei Ka Le!" << endl;
		if (0)
		{
		Ran:
			//std::cout << "Randoming!" << endl;
			//getchar();
			stuck_flag = false;
			first = true;
			dir_time = 0;
			if (!special && trytime <= 4)
			{
				//std::cout << "Clear Error!" << endl;
				if (count >= 0)
					count--;
				else
					count = -1;
				trytime++;
				if (trytime == 1)
				{
					if (PlayerInfo.position.x > x)
					{
						_move(Left, 50);
						Time(50);
					}
					else
					{
						_move(Right, 50);
						Time(50);
					}
				}
				//std::cout << "Trytime: " << trytime << endl;
				j = k = -1;
				return -1;
			}
			else
			{
			REALRAN:
				std::cout << "Real Random!" << endl;
				//std::cout << "Trytime, to be cleared: " << trytime << endl;
				//std::cout << "i = " << i << endl;
				//if (trytime >= 5)
				//{
					//std::cout << "Trytime == 5! What did you do? No!!!!!!!!!!" << endl;
					//getchar();
				//}
				//getchar();
				//getchar();
				trytime = 0;
				count = 0;
				int temp = rand() % 8;
				_move(random(temp), dist * i);
				Time(dist * i);
				if (rand() % 2)
					_move(random((temp + 2) % 8), dist * i);
				else
					_move(random((temp - 2) % 8), dist * i);
				Time(dist * i);
				//std::cout << "Direction Temp:" << (temp % 8) << endl;
				//getchar();
				if (i < 2)
					i += 0.5;
				else if (i == 2 || i == 3)
					i++;
				return 0;
			}
		}
		//dist = 50 * (trytime_count + 1);
		/*
		if (!stuck_flag)
		{
			check if real stuck;
			if real
				count = 0
				_use dir, stuck to decide last_dir
			if not real
				return 0
		}
		*/
		if (!stuck_flag)
		{
			//std::cout << "Try Set True!" << endl;
			if (!(state = Check()))
			{
				Time(50);
				if (_trytime >= 1)
				{
					_trytime = 0;
					//std::cout << "Force!" << endl;
					//std::cout << "Trytime: " << trytime << endl;
					goto Ran;
				}
				_trytime++;
				return 0;
			}
			_trytime = 0;
			//std::cout << "True Already!" << endl;
			stuck_flag = true;
			count = 0;
			bool _Right = (x > a);//Ŀ������
			bool _Up = (y > b);//Ŀ������
			if (_Right)
			{
				if (!(state % 2))
				{
					if (_Up)
						aim = Up;
					else
						aim = Down;
					last_dir = Right;
				}
				else
				{
					aim = Right;
					if (_Up)
					{
						if (!((state >> 1) % 2))
							last_dir = Up;
						else if (!((state >> 3) % 2))
							last_dir = Down;
						else
						{
							//std::cout << "$$$$$";
							//getchar();
							special = 1;
							goto Ran;
						}
					}
					else
					{
						if (!((state >> 3) % 2))
							last_dir = Down;
						else if (!((state >> 1) % 2))
							last_dir = Up;
						else
						{
							special = 1;
							//std::cout << "$$$$$";
							//getchar();
							goto Ran;
						}
					}
				}
			}
			else
			{
				if (!((state >> 2) % 2))
				{
					if (_Up)
						aim = Up;
					else
						aim = Down;
					last_dir = Left;
				}
				else
				{
					aim = Left;
					if (_Up)
					{
						if (!((state >> 1) % 2))
							last_dir = Up;
						else if (!((state >> 3) % 2))
							last_dir = Down;
						else
						{
							special = 1;
							//std::cout << "$$$$$";
							//getchar();
							goto Ran;
						}
					}
					else
					{
						if (!((state >> 3) % 2))
							last_dir = Down;
						else if (!((state >> 1) % 2))
							last_dir = Up;
						else
						{
							special = 1;
							//std::cout << "$$$$$";
							//getchar();
							goto Ran;
						}
					}
				}
			}
			int_dir = pow(2, (int)last_dir / 2);
			int_aim = pow(2, (int)aim / 2);
			return 0;
		}
		/*
else if (stuck_flag)
{
	check
	while last_dir available
		go until dir(aim) available
	except not available
		only one direction
		go until dir_last available
		go untile aim available
		go a little
		flag = false
		return 0
}
*/
		else if (stuck_flag)
		{
			if (first)
			{
				j = PlayerInfo.position.x;
				k = PlayerInfo.position.y;
				//std::cout << "aim = " << aim << endl;
				//std::cout << "last_dir = " << last_dir << endl;
				if (!(state = Check()))
				{
					Time(50);
					if (!(state = Check()))
						goto Ran;
				}
				first = false;
			}
			if (int_aim & ~state)
			{
				//std::cout << "State:" << state << endl;
				_move(aim, dist);
				Time(dist);
				if (count >= 2)
					count -= 2;
				else
					count = 0;
				dir_time = 0;
				if (aim == 0 || aim == 4)
				{
					if (abs(PlayerInfo.position.x - j) > 1.5)
					{
						//std::cout << "Init:" << PlayerInfo.position.x << ',' << j << endl;
						//getchar();
						stuck_flag = false;
						first = true;
						return 0;
					}
				}
				else
				{
					if (abs(PlayerInfo.position.y - k) > 1.5)
					{
						//std::cout << "Init:" << PlayerInfo.position.y << ',' << k << endl;
						//getchar();
						stuck_flag = false;
						first = true;
						return 0;
					}
				}
				state = Check();
				if (trytime >= 4)
				{
					special = 1;
					goto Ran;
				}
				trytime++;
				return 0;
			}
		Back:
			/*
			if (special)
			{
				special = 0;
				std::cout << "Special:" << int_dir << ',' << state << ',' << (int_dir & ~state) << endl;
				getchar();
			}
			*/
			if (int_dir & ~state)
			{
				//std::cout << "To last_dir, " << last_dir << '!' << endl;
				if (count <= 4)
					count++;
				if (dir_time >= 20)
				{
					special = 1;
					goto Ran;
				}
				dir_time++;
				_move(last_dir, dist);
				//else
				//	i = 1;
				Time(dist);
				//std::cout << "last_dir, aim: " << last_dir << ',' << aim << endl;
				//trytime = 0;
				//getchar();
				state = Check();
				return 0;
			}
			//std::cout << "WHY HERE? State:" << state << endl;
			if (trytime >= 4)
			{
				//std::cout << "OnlyUsual" << endl;
				special = 1;
				goto Ran;
			}
			//std::cout << "Just Check!";
			//getchar();
			trytime++;
			int temp;
			if (int_dir <= 3)
				temp = int_dir * 4;
			else
				temp = int_dir / 4;
			//std::cout << temp << ',' << int_dir << ',' << int_aim << ',' << state << endl;
			if (temp = (~(int_dir | int_aim | temp) & ~state & 15))
			{
				count = 0;
				//std::cout << "Before:" << temp << endl;
				if (temp == 1)
					temp = 0;
				else if (temp == 8)
					temp = 6;
				//std::cout << "After:" << temp << endl;
				_move((Direction)temp, 2 * dist);
				Time(2 * dist);
				//std::cout << state << endl;
				//std::cout << "~Aim! Direction:" << Direction(temp);
				//getchar();
				//special = 1;
				state = Check();
				goto Back;
				//return 0;
			}
			else
				goto Ran;
		}
		else
		{
			//std::cout << "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" << endl;
			goto Ran;
		}
	}
}

int Check0()
{
	double xx = PlayerInfo.position.x - 0.5;
	double yy = PlayerInfo.position.y - 0.5;
	bool flag_x = false, flag_y = false;
	bool flag_Right = false, flag_Left = false, flag_Up = false, flag_Down = false;
	int x = round(xx);
	int y = round(yy);
	double e = 0.00000000001;//�ֱ��
	if (abs(x - xx) <= e)
	{
		flag_x = true;
		x = round(xx);
	}
	if (xx > x)
		flag_Right = true;
	else if (xx < x)
		flag_Left = true;
	if (abs(y - yy) <= e)
	{
		flag_y = true;
		y = round(yy);
	}
	if (yy > y)
		flag_Up = true;
	else if (yy < y)
		flag_Down = true;
	/*
	if (xx == x)
		flag_x = true;
	if (yy == y)
		flag_y = true;
	*/
	//std::cout << flag_x << ',' << flag_y << endl;
	//printf("All flag: %d,%d,%d,%d\n", flag_Right, flag_Up, flag_Left, flag_Down);
	//std::cout << "Checking Position:" << x << ',' << y << endl;
	//std::cout << "Player Position:";
	//printf("%.14f, %.14f\n", PlayerInfo.position.x, PlayerInfo.position.y);
	int count = 0;
	static std::list<Obj>::iterator plist;
	static std::list<Obj> obj_list;
label1:
	if (!flag_x)
		goto label2;
	obj_list = MapInfo::get_mapcell(x + 1, y);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count++;
				goto label3;
			}
		}
	}
	if (flag_Up)
		obj_list = MapInfo::get_mapcell(x + 1, y + 1);
	else if (flag_Down)
		obj_list = MapInfo::get_mapcell(x + 1, y - 1);
	else
		goto label3;
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count++;
				goto label3;
			}
		}
	}
label3:
	obj_list = MapInfo::get_mapcell(x - 1, y);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 4;
				goto label2;
			}
		}
	}
	if (flag_Up)
		obj_list = MapInfo::get_mapcell(x - 1, y + 1);
	else if (flag_Down)
		obj_list = MapInfo::get_mapcell(x - 1, y - 1);
	else
		goto label2;
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 4;
				goto label2;
			}
		}
	}
label2:
	if (!flag_y)
		goto label;
	obj_list = MapInfo::get_mapcell(x, y + 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 2;
				goto label4;
			}
		}
	}
	if (flag_Right)
		obj_list = MapInfo::get_mapcell(x + 1, y + 1);
	else if (flag_Left)
		obj_list = MapInfo::get_mapcell(x - 1, y + 1);
	else
		goto label4;
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 2;
				goto label4;
			}
		}
	}
label4:
	obj_list = MapInfo::get_mapcell(x, y - 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 8;
				goto label;
			}
		}
	}
	if (flag_Right)
		obj_list = MapInfo::get_mapcell(x + 1, y - 1);
	else if (flag_Left)
		obj_list = MapInfo::get_mapcell(x - 1, y - 1);
	else
		goto label;
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 1 || plist->objType == 0)
			{
				count += 8;
				goto label;
			}
		}
	}
label:
	//std::cout << "Count: " << count << endl;
	return count;
}

int Check1()
{
	double xx = PlayerInfo.position.x - 0.5;
	double yy = PlayerInfo.position.y - 0.5;
	int x = round(xx);
	int y = round(yy);
	//int flag_R = 0, flag_U = 0;
	//std::cout << "Check1: " << endl;
	//std::cout << "Checking Position:" << x << ',' << y << endl;
	//std::cout << "Player Position:";
	//printf("%.14f, %.14f\n", PlayerInfo.position.x, PlayerInfo.position.y);
	/*
	if (xx > x)
		flag_R = 1;
	else if (xx < x)
		flag_R = -1;
	if (yy > y)
		flag_U = 1;
	else if (yy < y)
		flag_U = -1;
	*/
	int count = 0;
	static std::list<Obj>::iterator plist;
	static std::list<Obj> obj_list;
	obj_list = MapInfo::get_mapcell(x + 1, y);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 1;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x - 1, y);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 16;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x, y + 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 4;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x, y - 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 64;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x + 1, y + 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 2;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x - 1, y + 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 8;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x - 1, y - 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 32;
				break;
			}
		}
	}
	obj_list = MapInfo::get_mapcell(x + 1, y - 1);
	if (obj_list.size())
	{
		for (plist = obj_list.begin(); plist != obj_list.end(); plist++)
		{
			if (plist->objType == 0)
			{
				count |= 128;
				break;
			}
		}
	}
	//flag[0] << "Count: " << count << endl;
	return count;
}

enum Protobuf::Direction random(int a)
{
	switch (a)
	{
	case 0:
		return Right; break;
	case 1:
		return RightUp; break;
	case 2:
		return Up; break;
	case 3:
		return LeftUp; break;
	case 4:
		return Left; break;
	case 5:
		return LeftDown; break;
	case 6:
		return Down; break;
	case 7:
		return RightDown; break;
	}
}

void Time(int _time)
{
	/*
	clock_t start, end;
	start = getGameTime();
	while (end = getGameTime())
		if ((end - start) >= _time)
			break;
			*/
	Sleep(_time);
}

int Check(int a)
{
	switch (a)
	{
	case 0: return Check0();
	case 1: return Check1();
	}
	//std::cout << "Wrong!!!!!" << endl;
	getchar();
	return 0;
}

int gotoxy(const double x, const double y, int type)
{
	switch (type)
	{
	case 0:
		return gotoxy0(x, y);
	case 1:
		return gotoxy1(round(x), round(y));
	}
}

int gotoxy1(const int x, const int y, const int NG)
{
	bool flag[4] = { true, true, true, true };
	int decision;
	int i, value;
	static int success = 0;
	double nowx = PlayerInfo.position.x;
	double nowy = PlayerInfo.position.y;
	double aimx;
	double aimy;
	double min = 1000;
	double temp;
	static int forbid = NG;
	static int trytime = 0;
	if (themap[x + 1][y] || x == 49 || forbid % 2)
		flag[0] = false;
	if (themap[x][y + 1] || y == 49 || (forbid >> 1) % 2)
		flag[1] = false;
	if (themap[x - 1][y] || x == 0 || (forbid >> 2) % 2)
		flag[2] = false;
	if (themap[x][y - 1] || y == 0 || (forbid >> 3) % 2)
		flag[3] = false;
	if (!(flag[0] || flag[1] || flag[2] || flag[3]))
	{
		std::cout << "All around is people!" << endl;
		forbid = 0;
		trytime = 0;
		success = 0;
		return -2;
	}
	if (flag[0])
	{
		temp = sqrt(pow(abs(nowx - 1.5 - x), 2) + pow(abs(nowy - 0.5 - y), 2));
		if (temp < min)
		{
			min = temp;
			i = 0;
			aimx = x + 1.5;
			aimy = y + 0.5;
		}
	}
	if (flag[1])
	{
		temp = sqrt(pow(abs(nowx - 0.5 - x), 2) + pow(abs(nowy - 1.5 - y), 2));
		if (temp < min)
		{
			min = temp;
			i = 1;
			aimx = x + 0.5;
			aimy = y + 1.5;
		}
	}
	if (flag[2])
	{
		temp = sqrt(pow(abs(nowx + 0.5 - x), 2) + pow(abs(nowy - 0.5 - y), 2));
		if (temp < min)
		{
			min = temp;
			i = 2;
			aimx = x - 0.5;
			aimy = y + 0.5;
		}
	}
	if (flag[3])
	{
		temp = sqrt(pow(abs(nowx - 0.5 - x), 2) + pow(abs(nowy + 0.5 - y), 2));
		if (temp < min)
		{
			min = temp;
			i = 3;
			aimx = x + 0.5;
			aimy = y - 0.5;
		}
	}
	value = gotoxy(aimx, aimy);
	if (value == 1)
	{
		Time(50);
		int dist;
		nowx = PlayerInfo.position.x;
		nowy = PlayerInfo.position.y;
		if ((nowx - aimx) >= 0.3)
		{
			dist = (nowx - aimx) * 1000 / PlayerInfo.moveSpeed;
			_move(Left, dist);
			if (dist >= 50)
				Time(dist);
			else
				Time(50);
		}
		else if ((nowx - aimx) <= -0.3)
		{
			dist = (aimx - nowx) * 1000 / PlayerInfo.moveSpeed;
			_move(Right, dist);
			if (dist >= 50)
				Time(dist);
			else
				Time(50);
		}
		if ((nowy - aimy) >= 0.3)
		{
			dist = (nowy - aimy) * 1000 / PlayerInfo.moveSpeed;
			_move(Down, dist);
			if (dist >= 50)
				Time(dist);
			else
				Time(50);
		}
		else if ((nowy - aimy) <= -0.3)
		{
			dist = (aimy - nowy) * 1000 / PlayerInfo.moveSpeed;
			_move(Up, dist);
			if (dist >= 50)
				Time(dist);
			else
				Time(50);
		}
		switch (i)
		{
		case 0: _move(Left, 50); break;
		case 1: _move(Down, 50); break;
		case 2: _move(Right, 50); break;
		case 3: _move(Up, 50); break;
		}
		Time(50);
		nowx = PlayerInfo.position.x;
		nowy = PlayerInfo.position.y;
		/*
		switch (i)
		{
		case 0: PlayerInfo.facingDirection = Left; break;
		case 1: PlayerInfo.facingDirection = Down; break;
		case 2: PlayerInfo.facingDirection = Right; break;
		case 3: PlayerInfo.facingDirection = Up; break;
		}
		*/
		//std::cout << "i = "<< i << endl;
		//std::cout << PlayerInfo.facingDirection << endl;
		//std::cout << PlayerInfo.position.x << ',' << PlayerInfo.position.y << endl;
		//getchar();
		if (abs(nowx - aimx) < 0.5 && abs(nowy - aimy) < 0.5)
		{
			trytime = 0;
			forbid = 0;
			success++;
			if (success >= 3)
			{
				gotoxy0(0, 0, 2);
			}
			return 1;
		}
		else
		{
			if (PlayerInfo.facingDirection % 2)
				return 0;
			else
			{
				int face_num = round(pow(2.0, PlayerInfo.facingDirection));
				int temp = Check1();
				if (temp & face_num)
				{
					trytime = 0;
					switch (face_num)
					{
					case 1: forbid |= 4; break;
					case 4: forbid |= 8; break;
					case 16: forbid |= 1; break;
					case 64: forbid |= 2; break;
					}
					return -1;
				}
				if (face_num == 1)
					face_num |= 0x82;
				else
					face_num |= face_num >> 1 + face_num << 1;
				if (temp & face_num)
				{
					if (!trytime)
					{
						trytime++;
						return 0;
					}
					else
					{
						//std::cout << "We try!" << endl;
						trytime = 0;
						switch (face_num)
						{
						case 1: forbid |= 4; break;
						case 4: forbid |= 8; break;
						case 16: forbid |= 1; break;
						case 64: forbid |= 2; break;
						}
						//getchar();
						success = 0;
						return -1;
					}
				}
			}

		}
	}
	else
	{
		success = 0;
		return value;
	}
}

int gotoxy2(const double x, const double y)
{
	static int success = 0;
	int value = gotoxy0(x, y);
	if (value != 1)
	{
		success = 0;
		return value;
	}
	else
	{
		Time(50);
		double dx = PlayerInfo.position.x - x;
		double dy = PlayerInfo.position.y - y;
		int dist[2] = { 0 };
		if (abs(dx) > 0.3)
		{
			dist[0] = abs(dx) * 1000 / PlayerInfo.moveSpeed;
			if (dx > 0)
			{
				_move(Left, dist[0]);
				Time(dist[0]);
			}
			else
			{
				_move(Right, dist[0]);
				Time(dist[0]);
			}
		}
		if (abs(dy) > 0.3)
		{
			dist[1] = abs(dy) * 1000 / PlayerInfo.moveSpeed;
			if (dy > 0)
			{
				_move(Down, dist[1]);
				Time(dist[1]);
			}
			else
			{
				_move(Up, dist[1]);
				Time(dist[1]);
			}
		}
		Time(25);
		dx = PlayerInfo.position.x - x;
		dy = PlayerInfo.position.y - y;
		if (abs(dx) < 0.4 && abs(dy) < 0.4)
		{
			success++;
			if (success >= 3)
				gotoxy0(0, 0, 2);
			return 1;
		}
		else
			return -1;
	}
}

void _move(Direction direction_t, int duration)
{
	int trytime = 0;
	while (!THUAI3::move(direction_t, duration))
	{
		Time(13);
		trytime++;
		if (trytime >= 3)
			break;
	}
}

void _put(double distance, double angle, bool isThrowDish)
{
	int trytime = 0;
	while (!THUAI3::put(distance, angle, isThrowDish))
	{
		Time(17);
		trytime++;
		if (trytime >= 5)
			break;
	}
	Time(50);
}

void _use(int type, double parameter1, double parameter2)
{
	int trytime = 0;
	while (!THUAI3::use(type, parameter1, parameter2))
	{
		Time(17);
		trytime++;
		if (trytime >= 10)
			break;
	}
	Time(50);
}

void _pick(bool isSelfPosition, ObjType _pickType, int dishOrToolType)
{
	int trytime = 0;
	while (!THUAI3::pick(isSelfPosition, _pickType, dishOrToolType))
	{
		Time(17);
		trytime++;
		if (trytime >= 5)
			break;
	}
	Time(50);
}

void _speakToFriend(string speakText)
{
	while (!THUAI3::speakToFriend(speakText))
		Time(13);
	Time(50);
}

/*
void AutoPick(int type, int prior)
{

}
*/

void _FlushMark()
{
	int i;
	int max, num;//�������Լ�����
Recheck:
	while (1)
	{
		//for (i = 0; i < 23; i++)
		//{
		//	C[i]();
		//}
		max = 0;
		num = 0;
		C[14]();
		for (i = 22; i > 0; i--)
		{
			mark[i][1] = 0;
			D[i](0);
		}
		ClearP();
		for (i = 22; i >= 14; i--)
		{
			if (TaskTime[i - 14][0] && AllFood[i])
			{
				if (AllFood[i] == 1 && My.AnotherHand == i)
					continue;
				else if(My.mark_state != 3)
				{
					My.Force = 1;
					cout << "#$#$#$#$#$#$#$#$#$#$#$" << endl;
					My.mark_state = 3;
					ClearD();
					Dicision[0][0] = i;
					Dicision[0][1] = -1;
					//break;
				}
				goto Next;
			}
		}
		//else if (My.mark_state == 1)
		//{
		//	;
		//}
		if (My.mark_state == 3)
		{
			My.mark_state = 0;
			cout << "WWWWWWWWWHHHHHHHHHHHAAAAATTT" << endl;
			My.Force = 1;
		}
		Next:
		if(My.mark_state != 3) //if (My.mark_state == 0)
		{
			for (i = 0; i < 23; i++)
			{
				if (i >= 14)
				{
					if (TaskTime[i - 14][0] && mark[i][0])
					{
						if (AllFood[i])
						{
							if (AllFood[i] == 1 && My.AnotherHand == i);
							else
								continue;
						}
						if(PlayerInfo.dish != i)
							Insert(i, mark[i][3]);
					}
				}
			}
			if (pre[0][1])
			{
				//cout << "ohhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh!" << endl;
				D[pre[0][0]](1);
				if (My.mark_state != 1)
				{
					My.mark_state = 1;
					My.Force = 1;
					cout << "�ң�";
				}
			}
			else
			{
				//cout << "RERERERERERERERE" << endl;
				Sleep(50);
				goto Recheck;
			}
		}
		Sleep(37);
	}
}

void C0()
{
	return;
}

void C1()
{
	return;
}

void C2()
{
	return;
}

void C3()
{
	return;
}

void C4()
{
	return;
}

void C5()
{
	return;
}

void C6()
{
	return;
}

void C7()
{
	return;
}

void C8()
{
	return;
}

void C9()
{
	return;
}

void C10()
{
	return;
}

void C11()
{
	return;
}

void C12()
{
	return;
}

void C13()
{
	return;
}

void C14()
{
	/*
	if (mark[CookedRice][0])
		cout << "Ready!" << endl;
	else
		cout << "Not yet!" << endl;
		*/
	//cout << "Rice: " << AllFood[Rice] << endl;
	if (AllFood[CookedRice])
	{
		mark[CookedRice][0] = 1;
		mark[CookedRice][2] = 0;
		mark[CookedRice][3] = 0;
		return;
	}
	if (My.CookingDish[0] == CookedRice || My.CookingDish[1] == CookedRice)
	{
		int start = 1000000;
		int flag;
		mark[CookedRice][0] = 1;
		if (My.CookingDish[0] == CookedRice)
		{
			start = My.StartTime[0];
			flag = 0;
		}
		if (My.CookingDish[1] == CookedRice)
		{
			if (start > My.StartTime[1])
			{
				start = My.StartTime[1];
				flag = 1;
			}
		}
		mark[CookedRice][2] = 0;
		mark[CookedRice][3] = 10000 - (getGameTime() - start);
		return;
	}
	if (AllFood[Rice])
	{
		mark[CookedRice][0] = 1;
		mark[CookedRice][2] = 1;
		mark[CookedRice][3] = 10000;
		return;
	}
	else
	{
		mark[CookedRice][0] = 0;
		mark[CookedRice][2] = 2;
		mark[CookedRice][3] = 10000;
	}
}

void C15()
{
	return;
}

void C16()
{
	return;
}

void C17()
{
	return;
}

void C18()
{
	return;
}

void C19()
{
	return;
}

void C20()
{
	return;
}

void C21()
{
	return;
}

void C22()
{
	return;

}

void D0(int flag)
{
	return;
}

void D1(int flag)
{
	return;
}

void D2(int flag)
{
	return;
}

void D3(int flag)
{
	return;
}

void D4(int flag)
{
	return;
}

void D5(int flag)
{
	return;
}

void D6(int flag)
{
	return;
}

void D7(int flag)
{
	return;
}

void D8(int flag)
{
	return;
}

void D9(int flag)
{
	return;
}

void D10(int flag)
{
	return;
}

void D11(int flag)
{
	return;
}

void D12(int flag)
{
	return;
}

void D13(int flag)
{
	return;
}

void D14(int flag)
{
	if (!flag)
	{
		//mark[CookedRice][1] = 0;
		if (mark[CookedRice][0] == 0)
		{
			mark[CookedRice][1] = 0;
		}
		else
		{
			for (int i = 1; i < 6; i++)
			{
				if (40000 - (getGameTime() - TaskTime[0][i]) > mark[CookedRice][3] + step * mark[CookedRice][2])
				{
					mark[CookedRice][1] += 20;
				}
			}
		}
	}
	else
	{
		ClearD();
		Dicision[0][0] = Rice;
		Dicision[0][1] = CookedRice;
		Dicision[0][2] = -1;
	}
}

void D15(int flag)
{
	return;
}

void D16(int flag)
{
	return;
}

void D17(int flag)
{
	return;
}

void D18(int flag)
{
	return;
}

void D19(int flag)
{
	return;
}

void D20(int flag)
{
	return;
}

void D21(int flag)
{
	return;
}

void D22(int flag)
{
	return;
}

void CookPut(int num)
{
	double dx = CookPlace[num][0] + 0.5 - PlayerInfo.position.x;
	double dy = CookPlace[num][1] + 0.5 - PlayerInfo.position.y;
	double dist = sqrt(dx * dx + dy * dy);
	double theta = asin(dy / dist);
	if (dx < 0)
		theta = 3.1416 - theta;
	_put(dist, theta, true);
}

void CookPick(int num)
{
	double dx = CookPlace[num][0] + 0.5 - PlayerInfo.position.x;
	double dy = CookPlace[num][1] + 0.5 - PlayerInfo.position.y;
	if (abs(dx) >= 0.99)
	{
		if (dx > 0)
			_move(Right, 10);
		else
			_move(Left, 10);
		Sleep(10);
	}
	else if (abs(dy) >= 0.99)
	{
		if (dy > 0)
			_move(Up, 10);
		else
			_move(Down, 10);
		Sleep(10);
	}
	_pick(false, Block, 0);
}

void FlushTask()
{
	std::list<DishType> _tasklist = task_list;
	int Task_num[9] = { 0 };
	int now = getGameTime();
	int e = 1000;
	int i, j;
	std::list<DishType>::iterator plist;
	for (plist = _tasklist.begin(); plist != _tasklist.end(); plist++)
	{
		//std::cout << "*plist = " << *plist << endl;
		if (*plist >= 14 && *plist <= 22)
			Task_num[*plist - 14]++;
	}
	for (i = 0; i < 9; i++)
	{
		if (Task_num[i] == TaskTime[i][0])
		{
			for (j = 1; j <= Task_num[i]; j++)
			{
				if (now - TaskTime[i][j] > e + DishInfo.find(i + 14)->second.TaskTime)
					TaskTime[i][j] = now;
			}
			Order(i);
		}
		else if (Task_num[i] < TaskTime[i][0])
		{
			for (j = Task_num[i + 1]; j <= 5; j++)
				TaskTime[i][j] = 0;
			TaskTime[i][0] = Task_num[i];
			for (j = 1; j <= Task_num[i]; j++)
			{
				if (now - TaskTime[i][j] > e + DishInfo.find(i + 14)->second.TaskTime)
					TaskTime[i][j] = now;
			}
			Order(i);
		}
		else
		{
			int dif = Task_num[i] - TaskTime[i][0];
			for (j = 5; j >= 5 - dif + 1; j--)
				TaskTime[i][j] = now;
			Order(i);
			TaskTime[i][0] = Task_num[i];
			for (j = 1; j <= Task_num[i]; j++)
			{
				if (now - TaskTime[i][j] > e + DishInfo.find(i + 14)->second.TaskTime)
					TaskTime[i][j] = now;
			}
			Order(i);
		}
	}
}
//�����˵���Task��0��

void _FlushTask()
{
	while (1)
	{
		FlushTask();
		for (int i = 0; i < 9; i++)
		{
			//std::cout << TaskTime[i][0] << ' ';
		}
		cout << endl;
		Sleep(100);
	}
}

//$��ʾ�쳣
void _GetMessage()
{
	char a[17];
	int num;
	//long long start;
	strcpy(a, PlayerInfo.recieveText.c_str());
	while (1)
	{
		//start = getGameTime();
		if (a != PlayerInfo.recieveText)
		{
			strcpy(a, PlayerInfo.recieveText.c_str());
			if (a[0] != '$')
			{
				num = (a[0] % 100) * 100 + (a[1] % 100);
				strcpy(Normal[num], a);
				N++;
				if (!Read(num));
			}
			else
			{
				strcpy(Abnormal[AN], a);
				AN++;
			}
		}
		FlushPlace();
		FlushAim();
		FlushHand();
		//FlushHand();
		Sleep(2);
	}
}

void FlushPlace()
{
	int i, j;
	int p = 0;
	static int last[3] = { -1, -1 , -1};
	int flag[3] = { -1, -1, -1 };
	std::list<Obj> objects;
	std::list<Obj>::iterator plist;
	for (i = 0; i < 10; i++)
	{
		if (!StorePlace[i][2])
			continue;
		if ((abs(PlayerInfo.position.x - StorePlace[i][0] - 0.5) < 2) && (abs(PlayerInfo.position.y - StorePlace[i][1] - 0.5) < 2))
		{
			//cout << "&&&&&&&&" << endl;
			//cout << PlayerInfo.sightRange << endl;
			//cout << (((abs(PlayerInfo.position.x - StorePlace[i][0] - 0.5) < PlayerInfo.sightRange - 0.1) && (abs(PlayerInfo.position.y - StorePlace[i][1] - 0.5) < PlayerInfo.sightRange - 0.1))) << endl;
			//cout << PlayerInfo.position.x << ',' << PlayerInfo.position.y << endl;
			objects = MapInfo::get_mapcell(StorePlace[i][0], StorePlace[i][1]);
			Time0[i] = getGameTime();
			flag[0] = i;
			for (plist = objects.begin(); plist != objects.end(); plist++)
			{
				if(plist->objType == Dish)
				{
					if (p >= 10)
					{
						if (plist->dish < 10)
							continue;
						else if (plist->dish >= 10 && plist->dish <= 13)
						{
							for (j = 0; j < 10; j++)
							{
								if (SP[i][j] < 10)
								{
									SP[i][j] = plist->dish;
									break;
								}
							}
						}
						else if (plist->dish >= 14)
						{
							for (j = 0; j < 10; j++)
							{
								if (SP[i][j] < 10)
								{
									SP[i][j] = plist->dish;
									break;
								}
							}
							if (j == 10)
							{
								for (j = 0; j < 10; j++)
								{
									if (SP[i][j] < 14)
									{
										SP[i][j] = plist->dish;
										break;
									}
								}
							}
						}
					}
					else
					{
						SP[i][p] = plist->dish;
						//std::cout << "I find dish! Look! it is " << plist->dish << " !" << endl;
					}
					p++;
				}
			}
			for (; p < 10; p++)
				SP[i][p] = 0;
		}
	}
	if (last[0] == -1)
	{
		last[0] = flag[0];
	}
	else
	{
		if (flag[0] == last[0]){;}
		else
		{
			while (!My.ComState)
			{
				std::cout << "Waiting..." << endl << endl;
				Sleep(10);
			}
			My.ComState = 2;
			WriteHead();
			My.ToSend[2] = 110;
			My.ToSend[3] = last[0];
			for (i = 0; i < 10; i++)
			{
				My.ToSend[4 + i] = SP[last[0]][i];
			}
			My.ToSend[14] = PlayerInfo.dish;
			ZTH(14);
			FlushSend();
			last[0] = flag[0];
			//getchar();
		}
	}
	for (i = 0; i < 4; i++)
	{
		int xx = round(PlayerInfo.position.x - 0.5);
		int yy = round(PlayerInfo.position.y - 0.5);
		if (abs(xx - CookPlace[i][0]) < PlayerInfo.sightRange && abs(yy - CookPlace[i][1]) < PlayerInfo.sightRange)
		{
			objects = MapInfo::get_mapcell(CookPlace[i][0], CookPlace[i][1]);
			for (plist = objects.begin(); plist != objects.end(); plist++)
			{
				if (plist->dish == CookingDish)
				{
					Time1[i] = getGameTime();
					break;
				}
			}
		}
	}
	for (i = 0; i < 8; i++)
	{
		//if((abs(PlayerInfo.position.x - FoodPlace[i][0] - 0.5) <= 1.5 && abs(PlayerInfo.position.y - FoodPlace[i][1] - 0.5) <= 1.5))
		if ((abs(PlayerInfo.position.x - FoodPlace[i][0] - 0.5) <= 1.5 && abs(PlayerInfo.position.y - FoodPlace[i][1] - 0.5) <= 0.5) || (abs(PlayerInfo.position.x - FoodPlace[i][0] - 0.5) <= 0.5 && abs(PlayerInfo.position.y - FoodPlace[i][1] - 0.5) <= 1.5))
		{
			flag[2] = i;
			Time2[i] = getGameTime();
			if (last[2] == -1)
			{
				last[2] = flag[2];
			}
			else if (last[2] != flag[2])
			{
				if (flag[2] != -1)
				{
					My.Force = 1;
				}
				else
				{
					while (!My.ComState)
					{
						std::cout << "Waiting..." << endl << endl;
						Sleep(10);
					}
					My.ComState = 2;
					WriteHead();
					My.ToSend[2] = 112;
					My.ToSend[3] = i;
					ZTH(3);
					FlushSend();
				}
			}
			break;
		}
	}
	FlushFood();
}

void FlushFood()
{
	int i, j;
	for (i = 0; i < 23; i++)
	{
		AllFood[i] = 0;
	}
	if (PlayerInfo.dish)
		AllFood[PlayerInfo.dish]++;
	if (My.AnotherHand)
		AllFood[PlayerInfo.dish]++;
	for (i = 0; i < 10; i++)
	{
		if (!StorePlace[i][2])
			continue;
		for (j = 0; j < 10; j++)
		{
			if (SP[i][j])
				AllFood[SP[i][j]]++;
		}
	}
}

void FlushAim()
{
	static int aim = My.aim;
	static int attr = My.attr;
	if (My.aim != aim || My.attr != attr)
	{
		aim = My.aim;
		attr = My.attr;
		while (!My.ComState)
		{
			std::cout << "Waiting..." << endl << endl;
			Sleep(10);
		}
		My.ComState = 2;
		WriteHead();
		My.ToSend[2] = 101;
		My.ToSend[3] = My.attr;
		My.ToSend[4] = My.aim;
		My.ToSend[5] = round(PlayerInfo.position.x - 0.5);
		My.ToSend[6] = round(PlayerInfo.position.y - 0.5);
		ZTH(6);
		FlushSend();
	}
}

void FlushHand()
{
	static int last[3] = {PlayerInfo.dish, My.CookingDish[0], My.mark_state};
	if (PlayerInfo.dish != last[0] || My.CookingDish[0] != last[1] || My.mark_state != last[2])
	{
		last[0] = PlayerInfo.dish;
		last[1] = My.CookingDish[0];
		last[2] = My.mark_state;
		while (!My.ComState)
		{
			std::cout << "Waiting..." << endl << endl;
			Sleep(10);
		}
		My.ComState = 2;
		WriteHead();
		My.ToSend[2] = 103;
		My.ToSend[3] = last[0];
		My.ToSend[4] = last[1];
		My.ToSend[5] = last[2];
		ZTH(5);
		FlushSend();
	}
}

void Order(int n)
{
	int i, j, temp;
	for (i = 1; i < 5; i++)
	{
		for (j = i + 1; j <= 5; j++)
		{
			if (TaskTime[n][i] < TaskTime[n][j])
			{
				temp = TaskTime[n][i];
				TaskTime[n][i] = TaskTime[n][j];
				TaskTime[n][j] = temp;
			}
		}
	}
}

int SearchNear0(int type)
{
	int i, j;
	int cold = 5000;
	long long _t = getGameTime();
	double xx = PlayerInfo.position.x;
	double yy = PlayerInfo.position.y;
	int num = -1;
	double min = 10000;
	double temp;
	if (!type)
	{
		for (i = 0; i < 10; i++)
		{
			if (num == -1 && StorePlace[i][2])
				num = i;
			if (_t - Time0[i] < cold && Time0[i] || !StorePlace[i][2])
				continue;
			temp = sqrt((xx - StorePlace[i][0] - 0.5) * (xx - StorePlace[i][0] - 0.5) + (yy - StorePlace[i][1] - 0.5) * (yy - StorePlace[i][1] - 0.5));
			if (temp < min)
			{
				min = temp;
				num = i;
			}
		}
		return num;
	}
	else
	{
		for (i = 0; i < 10; i++)
		{
			if (num == -1 && StorePlace[i][2])
				num = i;
			if (!StorePlace[i][2])
				continue;
			for (j = 0; j < 10; j++)
			{
				if (SP[i][j] == type)
				{
					temp = sqrt((xx - StorePlace[i][0] - 0.5) * (xx - StorePlace[i][0] - 0.5) + (yy - StorePlace[i][1] - 0.5) * (yy - StorePlace[i][1] - 0.5));
					if (temp < min)
					{
						min = temp;
						num = i;
					}
					break;
				}
			}
		}
		return num;
	}
}

int SearchNear1()
{
	int i;
	int cold = 10000;
	long long _t = getGameTime();
	double xx = PlayerInfo.position.x;
	double yy = PlayerInfo.position.y;
	int num = -1;
	double min = 10000;
	double temp;
	for (i = 0; i < 4; i++)
	{
		if (_t - Time1[i] < cold && Time1[i])
			continue;
		temp = sqrt((xx - CookPlace[i][0] - 0.5) * (xx - CookPlace[i][0] - 0.5) + (yy - CookPlace[i][1] - 0.5) * (yy - CookPlace[i][1] - 0.5));
		if (temp < min)
		{
			min = temp;
			num = i;
		}
	}
	return num;
}

int SearchNear2()
{
	int i;
	int cold = 15000;
	long long _t = getGameTime();
	double xx = PlayerInfo.position.x;
	double yy = PlayerInfo.position.y;
	int num = 0;
	double min = 10000;
	double temp;
	for (i = 0; i < 8; i++)
	{
		if (_t - Time2[i] < cold && Time2[i])
		{
			//cout << i << "%%%%%" << endl;
			continue;
		}
		temp = sqrt((xx - FoodPlace[i][0] - 0.5) * (xx - FoodPlace[i][0] - 0.5) + (yy - FoodPlace[i][1] - 0.5) * (yy - FoodPlace[i][1] - 0.5));
		if (temp < min)
		{
			min = temp;
			num = i;
		}
	}
	return num;
}

int SearchNear3()
{
	int a[4][2] = { {25,24},{25,25},{26,24},{26,25} };
	int cold = 5000;
	int i;
	long long _t = getGameTime();
	double xx = PlayerInfo.position.x;
	double yy = PlayerInfo.position.y;
	int num = 0;
	double min = 10000;
	double temp;
	for (i = 0; i < 4; i++)
	{
		if (_t - Time3[i] < cold && Time3[i])
		{
			//cout << i << "%%%%%" << endl;
			continue;
		}
		temp = sqrt((xx - a[i][0] - 0.5) * (xx - a[i][0] - 0.5) + (yy - a[i][1] - 0.5) * (yy - a[i][1] - 0.5));
		if (temp < min)
		{
			min = temp;
			num = i;
		}
	}
	return num;

}

void FlushSend()
{
	int i;
	int cold = 100;
	long long dif = getGameTime() - My.LastSend;
	My.ComState = 0;
	if (dif < cold)
	{
		Sleep(dif);
	}
	if (My.ToSend[0] != '$')
	{
		_speakToFriend(My.ToSend);
		My.LastSend = getGameTime();
		strcpy(NormalSend[NS], My.ToSend);
		NS++;
		for (i = 0; i < 16; i++)
			My.ToSend[i] = 0;
		My.ComState = 1;
	}
	else
	{
		My.LastSend = getGameTime();
		My.ComState = 1;
	}
}

void WriteHead()
{
	My.ToSend[1] = NS % 100;
	if (My.ToSend[1] == 0)
		My.ToSend[1] = 100;
	My.ToSend[0] = NS / 100;
	if (My.ToSend[0] == 0)
		My.ToSend[0] = 100;
}

void ZTH(int n)
{
	int i;
	for (i = 0; i <= n; i++)
	{
		if (!My.ToSend[i])
			My.ToSend[i] = 100;
	}
}

int Read(int letter)
{
	cout << "++++++++++++++++++++++++++++++++++++++++++++++++" << endl;
	switch (Normal[letter][2])
	{
	case 101: Read101(letter); break;
	case 102: Read102(letter); break;
	case 103: Read103(letter); break;
	case 110: Read110(letter); break;
	case 112: Read112(letter); break;
	}
	My.Force = 1;
	if (letter == N)
		return 1;
	else
		return 0;
}

void Read101(int letter)
{
	int attr = Normal[letter][3];
	int aim = Normal[letter][4];
	double dist1, dist2;
	if (attr == 100)
		attr = 0;
	if (aim == 100)
		aim = 0;
	if (attr == My.attr && aim == My.aim)
	{
		if (attr == 0)
		{
			dist1 = sqrt(pow((PlayerInfo.position.x - 0.5 - StorePlace[aim][0]), 2) + pow((PlayerInfo.position.y - 0.5 - StorePlace[aim][1]), 2));
			dist2 = sqrt(pow(Normal[letter][5] - StorePlace[aim][0], 2) + pow(Normal[letter][6] - StorePlace[aim][1], 2));
			if (dist1 < dist2)
			{
				while (!My.ComState)
				{
					std::cout << "Waiting..." << endl << endl;
					Sleep(10);
				}
				WriteHead();
				My.ToSend[2] = 102;
				My.ToSend[3] = attr;
				My.ToSend[4] = aim;
				ZTH(4);
				FlushSend();
			}
			else
			{
				Time0[aim] = getGameTime();
			}
		}
		else if (attr == 2)
		{
			dist1 = sqrt(pow((PlayerInfo.position.x - 0.5 - FoodPlace[aim][0]), 2) + pow((PlayerInfo.position.y - 0.5 - FoodPlace[aim][1]), 2));
			dist2 = sqrt(pow(Normal[letter][5] - FoodPlace[aim][0], 2) + pow(Normal[letter][6] - FoodPlace[aim][1], 2));
			if (dist1 < dist2)
			{
				while (!My.ComState)
				{
					std::cout << "Waiting..." << endl << endl;
					Sleep(10);
				}
				WriteHead();
				My.ToSend[2] = 102;
				My.ToSend[3] = attr;
				My.ToSend[4] = aim;
				ZTH(4);
				FlushSend();
			}
			else
			{
				Time2[aim] = getGameTime();
			}
		}
	}
}

void Read102(int letter)
{
	int attr = Normal[letter][3];
	int aim = Normal[letter][4];
	if (attr == 100)
		attr = 0;
	if (aim == 100)
		aim = 0;
	if (attr == 0)
	{
		Time0[aim] = getGameTime();
	}
	else if(attr == 1)
	{
		Time1[aim] = getGameTime();
	}
	else if (attr == 2)
	{
		Time2[aim] = getGameTime();
	}
}

void Read103(int letter)
{
	if (Normal[letter][3] == 100)
		My.AnotherHand = 0;
	else
		My.AnotherHand = Normal[letter][3];
	int cooking = Normal[letter][4];
	if (cooking == 100)
		cooking = 0;
	if (cooking != My.CookingDish[1])
	{
		My.CookingDish[1] = cooking;
		My.StartTime[1] = getGameTime();
	}
	if (Normal[letter][5] == 100)
		My.Another_state = 0;
	else
		My.Another_state = Normal[letter][5];
}

void Read110(int letter)
{
	int place = Normal[letter][3];
	int i;
	if (place == 100)
		place = 0;
	Time0[place] = getGameTime();
	for (i = 4; i < 14; i++)
	{
		if(Normal[letter][i] != 100)
			SP[place][i-4] = Normal[letter][i];
		else
			SP[place][i-4] = 0;
	}
	My.AnotherHand = Normal[letter][14];
	if (My.AnotherHand == 100)
		My.AnotherHand = 0;
	//My.Force = 1;
}

void Read112(int letter)
{
	int place = Normal[letter][3];
	if (place == 100)
		place = 0;
	Time2[place] = getGameTime();
	//My.Force = 1;
}

int CalNum(int place)
{
	int count = 0;
	for (int i = 0; i < 10; i++)
	{
		if (SP[place][i])
			count++;
	}
	return count;
}

void ClearD()
{
	int i, j;
	for (i = 0; i < 3; i++)
	{
		for (j = 0; j < 10; j++)
		{
			Dicision[i][j] = 0;
		}
	}
}

void Insert(int dish, int score)
{
	int num = 0;
	int min = 100000;
	int i, j;
	for (i = 0; i < 5; i++)
	{
		if (pre[i][1] < min)
		{
			min = pre[i][1];
			num = i;
		}
	}
	if (score > min)
	{
		pre[num][0] = dish;
		pre[num][1] = score;
		for (i = 0; i < 4; i++)
		{
			for (j = i + 1; j < 5; j++)
			{
				if (pre[i][1] < pre[j][1])
				{
					int temp;
					temp = pre[i][1];
					pre[i][1] = pre[j][1];
					pre[j][1] = temp;
					temp = pre[i][0];
					pre[i][0] = pre[j][0];
					pre[j][0] = temp;
				}
			}
		}
	}
	return;
}

void ClearP()
{
	for (int i = 0; i < 5; i++)
	{
		pre[i][0] = 0;
		pre[i][1] = 0;
	}
}

int FindDish(int dish)
{
	if (AllFood[dish] == 0 || (AllFood[dish] == 1 && My.AnotherHand == dish))
		return -1;
	if (PlayerInfo.dish == dish)
		return 100;
	double min = 100000;
	double temp;
	int num = -1;
	int i, j;
	for (i = 0; i < 10; i++)
	{
		if (StorePlace[i][2] == 0)
			continue;
		for (j = 0; j < 10; j++)
		{
			if (SP[i][j] == dish)
			{
				temp = pow(PlayerInfo.position.x - StorePlace[i][0] - 0.5, 2) + pow(PlayerInfo.position.y - StorePlace[i][1], 2);
				if (temp < min)
				{
					min = temp;
					num = i;
					break;
				}
			}
		}
	}
	return num;
}

int ClearTable(int n, int* able)
{
	int i;
	int flag = 0;
	int init = PlayerInfo.dish;
	std::list<Obj> objects = MapInfo::get_mapcell(CookPlace[My.aim][0], CookPlace[My.aim][1]);
	std::list<Obj>::iterator plist;
	if (n == 0)
	{
		for (plist = objects.begin(); plist != objects.end(); plist++)
		{
			if (plist->objType == Dish)
			{
				_pick(false, Dish, -1);
				_put(6, rand(), 1);
				flag = 1;
			}
		}
	}
	else
	{
		for (plist = objects.begin(); plist != objects.end(); plist++)
		{
			if (plist->objType == Dish)
			{
				for (i = 0; i < n; i++)
				{
					if (plist->dish == able[i])
						continue;
				}
				_pick(false, Dish, -1);
				_put(6, rand(), 1);
				flag = 1;
			}
		}
	}
	if (flag)
	{
		_pick(true, Dish, init);
		if (PlayerInfo.dish != init)
			_pick(false, Dish, init);
	}
	if (PlayerInfo.dish == init)
		return 1;
	else
		return 0;
}

void ClearCook(int i)
{
	My.StartTime[i] = 0;
	My.CP[i] = 0;
	My.CookingDish[i] = 0;
}
