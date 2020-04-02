#define DEVELOPER_ONLY
#include "MessageToClient.pb.h"
using namespace Protobuf;
#include "Constant.h"
#include <utility>
#include <vector>
#include <list>
#include <memory>
#include "structures.h"
#include "CAPI.h"


const double Constant::Tool::Condiment::ScoreParameter = 0.5;
const double Constant::Talent::Cook::Condiment::ScoreParameter = 0.5;


std::vector<std::vector<std::unordered_map<int64_t, std::shared_ptr<Obj>>>>MapInfo::obj_map;
std::vector<std::vector<std::mutex*>>MapInfo::mutex_map;
std::unordered_map<int64_t, std::shared_ptr<Obj>>MapInfo::obj_list;


std::list<DishType> task_list;

void MapInfo::initialize_map()
{
	obj_map.resize(init_mapinfo.size());
	mutex_map.resize(init_mapinfo.size());
	for (int x = 0; x < obj_map.size(); x++)
	{
		obj_map[x].resize(init_mapinfo[x].size());
		mutex_map[x].resize(init_mapinfo[x].size());
		for (int y = 0; y < obj_map[x].size(); y++)
		{
			mutex_map[x][y] = new std::mutex();
			switch (init_mapinfo[x][y])
			{
			case 1:
				obj_map[x][y].insert(std::pair<int64_t, std::shared_ptr<Obj>>(-1, std::make_shared<Obj>(XYPosition(x + 0.5, y + 0.5), ObjType::Block)));
				obj_map[x][y].begin()->second->blockType = BlockType::TaskPoint;
				break;
			case 4:
				obj_map[x][y].insert(std::pair<int64_t, std::shared_ptr<Obj>>(-1, std::make_shared<Obj>(XYPosition(x + 0.5, y + 0.5), ObjType::Block)));
				obj_map[x][y].begin()->second->blockType = BlockType::RubbishBin;
				break;
			case 5:
				obj_map[x][y].insert(std::pair<int64_t, std::shared_ptr<Obj>>(-1, std::make_shared<Obj>(XYPosition(x + 0.5, y + 0.5), ObjType::Block)));
				obj_map[x][y].begin()->second->blockType = BlockType::Wall;
				break;
			case 6:
				obj_map[x][y].insert(std::pair<int64_t, std::shared_ptr<Obj>>(-1, std::make_shared<Obj>(XYPosition(x + 0.5, y + 0.5), ObjType::Block)));
				obj_map[x][y].begin()->second->blockType = BlockType::Table;
				break;
			default:
				break;
			}
		}
	}
}

void MapInfo::print_map()
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

void MapInfo::print_map(int x, int y)
{
	if (x < 0 || x >= obj_map.size() || y < 0 || y >= obj_map.size())
	{
		std::cout << "Invalid" << std::endl;
		return;
	}

	std::cout << std::endl
		<< "map[" << x << "][" << y << "] : " << std::endl;
	for (std::unordered_map<int64_t, std::shared_ptr<Obj>>::iterator i = obj_map[x][y].begin(); i != obj_map[x][y].end(); i++)
	{
		std::cout << "\tobjType : " << i->second->objType << std::endl;
	}
	std::cout << std::endl;
}

void MapInfo::print_obj_list()
{
	std::cout << std::endl
		<< "obj_list : " << std::endl;
	for (std::unordered_map<int64_t, std::shared_ptr<Obj>>::iterator i = obj_list.begin(); i != obj_list.end(); i++)
	{
		std::cout << "\t" << i->first << " , " << i->second->objType << std::endl;
	}
}

std::list<Obj> MapInfo::get_mapcell(const int x, const int y)
{
	if (x < 0 || x >= obj_map.size() || y < 0 || y >= obj_map.size())
	{
		return list<Obj>();
	}
	if (abs(x - PlayerInfo._position.x) > PlayerInfo._sightRange || abs(y - PlayerInfo._position.y) > PlayerInfo._sightRange)
	{
		return list<Obj>();
	}
	list<Obj> list;
	mutex_map[x][y]->lock();
	for (std::unordered_map<int64_t, shared_ptr<Obj>>::iterator i = obj_map[x][y].begin(); i != obj_map[x][y].end(); i++)
	{
		list.push_back(*(i->second));
	}
	mutex_map[x][y]->unlock();
	return list;
}

Obj::Obj(const XYPosition& pos, ObjType objType) : position(pos), objType(objType)
{ }
player_info PlayerInfo;
