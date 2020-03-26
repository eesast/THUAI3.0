#define DEVELOPER_ONLY
#include "MessageToClient.pb.h"
using namespace Protobuf;
#include "Constant.h"
#include <utility>
#include <vector>
#include <list>
#include <memory>
#include "structures.h"

std::vector<std::vector<std::unordered_map<int64_t, std::shared_ptr<Obj>>>> obj_map;
std::unordered_map<int64_t, std::shared_ptr<Obj>> obj_list;
std::list<DishType> task_list;

void initialize_map()
{
	obj_map.resize(mapinfo.size());
	for (int i = 0; i < obj_map.size(); i++)
	{
		obj_map[i].resize(mapinfo[i].size());
	}
}

void print_map()
{
	std::cout << std::endl;
	for (int i = 0; i < obj_map.size(); i++)
	{
		std::cout << "map[" << i << "] : ";
		for (int j = 0; j < obj_map[i].size(); j++)
		{
			std::cout << obj_map[i][j].size() << " , ";
		}
		std::cout << std::endl;
	}
}

void print_map(int x, int y)
{
	std::cout << std::endl
		<< "map[" << x << "][" << y << "] : ";
	for (std::unordered_map<int64_t, std::shared_ptr<Obj>>::iterator i = obj_map[x][y].begin(); i != obj_map[x][y].end(); i++)
	{
		std::cout << i->second->objType << " , ";
	}
	std::cout << std::endl;
}

void print_obj_list()
{
	std::cout << std::endl
		<< "obj_list : " << std::endl;
	for (std::unordered_map<int64_t, std::shared_ptr<Obj>>::iterator i = obj_list.begin(); i != obj_list.end(); i++)
	{
		std::cout << "\t" << i->first << " , " << i->second->objType << std::endl;
	}
}

Obj::Obj(const XYPosition& pos, ObjType objType) : position(pos), objType(objType)
{ }


player_info PlayerInfo;