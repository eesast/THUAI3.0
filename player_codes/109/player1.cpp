
#include <bits/stdc++.h>


#include <math.h>

#include <array>
#include <cmath>
#include <iostream>
#include <list>
#include <sstream>
#include <string>
#include <thread>
#include <vector>

#include "API.h"
#include "Constant.h"
#include "OS_related.h"
#include "player.h"

#define PI acos(-1.0)

void tSleep(unsigned int millisec)
{
    std::this_thread::sleep_for(std::chrono::milliseconds(millisec));
}

constexpr int INTERVAL = 5;
void tUse(int type, double parameter1, double parameter2)
{
    while (!THUAI3::use(type, parameter1, parameter2))
        tSleep(INTERVAL);
}
void tPut(double distance, double angle, bool isThrowDish)
{
    while (!THUAI3::put(distance, angle, isThrowDish))
        tSleep(INTERVAL);
}
void tPick(bool isSelfPosition, ObjType pickType, int dishOrToolType)
{
    while (!THUAI3::pick(isSelfPosition, pickType, dishOrToolType))
        tSleep(INTERVAL);
}
void tSpeakToFriend(string speakText)
{
    while (!THUAI3::speakToFriend(speakText))
        tSleep(INTERVAL);
}
void tMove(Direction direction_t, int duration)
{
    while (!THUAI3::move(direction_t, duration))
        tSleep(INTERVAL);
}

using namespace THUAI3;
Protobuf::Talent initTalent =
    Protobuf::Talent::Runner;  //指定人物天赋。选手代码必须定义此变量，否则报错

const int kCostStraight = 10;  //直移一格消耗
const int kCostDiagonal  = 15;  //斜移一格消耗
const char dir[3][3] = {{'z', 'x', 'c'}, {'a', 's', 'd'}, {'q', 'w', 'e'}};
class Dishpos
{
public:
    Dishpos(int dish = 0, int x = 0, int y = 0)
    {
        Dish = dish;
        dishx = x;
        dishy = y;
    }
    int Dish;
    int dishx;
    int dishy;
};

list<Dishpos> finishedDish;  //这里放已经完成的菜品清单

struct Point {
  int x, y;  //点坐标，这里为了方便按照C++的数组来计算，x代表横排，y代表竖列
  int F{0}, G{0}, H{0};  // F=G+H
  Point* parent{nullptr};  // parent的坐标，这里没有用指针，从而简化代码
  Point(int _x, int _y) : x(_x), y(_y) {}  //变量初始化
  Point(const Point& xy) : x(xy.x), y(xy.y) {}
  Point(const XYPosition& xy) : x(xy.x), y(xy.y) {}
};

int cooking[6] = {0, 0, 0,
                  0, 0, 0};  //{0dish,1灶台label,2用时，3起，4止，5保护}
int state = 0;
int label;  //记录“据点”是第几个灶台
int oldlabel = 0;
int surround[81][2];  //懒得考虑返回值了，直接全局变量

double calcdis(Point point, Point end);
void get_all_dish(int _label);
void get_surround();
void findbestdish();



vector<vector<int>> foodgen =  // type x y,距离 还是食物生成点下方一格
    {{0, 0, 0, 99999},   {Wheat, 4, 24, 0},  {Rice, 5, 5, 0},
     {Tomato, 7, 41, 0}, {Egg, 25, 5, 0},    {Beef, 31, 41, 0},
     {Pork, 42, 40, 0},  {Potato, 43, 6, 0}, {Lettuce, 43, 25, 0}};
bool sort_by_0(const vector<int> a, const vector<int> b)  //默认从小到大排序
{
  return a[0] < b[0];
}
bool sort_by_0_double(const vector<double> a,
                      const vector<double> b)  //默认从小到大排序
{
  return a[0] < b[0];
}

bool sort_by_1(const vector<double> a,
               const vector<double> b)  //默认从小到大排序
{
  return a[1] < b[1];
}

bool sort_by_3(const vector<int> a, const vector<int> b)  //默认从小到大排序
{
  return a[3] < b[3];
}

void get_foodgen_dis() {
  Point self(PlayerInfo.position.x, PlayerInfo.position.y);
  for (int i = 1; i <= 8; i++) {
    Point ifood(foodgen[i][1], foodgen[i][2]);
    foodgen[i][3] = int(calcdis(self, ifood));
  }
  sort(foodgen.begin(), foodgen.end(), sort_by_0);  //确保顺序
}

list<vector<double>> bestdish = {};  //编号，性价比

//=============通用方法和结构=============
//点坐标
struct dPoint {
  double x;
  double y;
  dPoint(double _x, double _y) : x(_x), y(_y) {}
  dPoint(const dPoint& xy) : x(xy.x), y(xy.y) {}
  dPoint(const Point& xy) : x(xy.x), y(xy.y) {}
  dPoint(const XYPosition& xy) : x(xy.x), y(xy.y) {}
};
//某种食材有多少个
struct StoragePerDish {
  DishType type;             //食材名
  int stepsOfProcessed = 0;  //如果是1，说明只需要合成一次，以此类推。
  int cnt = 0;               //现在有多少个
  list<dPoint> posList;  //储存地点链表
  StoragePerDish(DishType _type, int _cnt) : type(_type), cnt(_cnt){};
  void sortNow()
  {
      if (posList.empty())
          return;
      posList.sort([](const dPoint p1, const dPoint p2) -> bool
          {
              return ((PlayerInfo.position.x - p1.x) * (PlayerInfo.position.x - p1.x) +
                  (PlayerInfo.position.y - p1.y) * (PlayerInfo.position.y - p1.y)) <
                  ((PlayerInfo.position.x - p2.x) * (PlayerInfo.position.x - p2.x) +
                  (PlayerInfo.position.y - p2.y) * (PlayerInfo.position.y - p2.y));
          });
  }
};

//=============全局常量=============

//储存目前所有物品的持有数量和储存地点，允许使用下标访问
class Storage {
 private:
  vector<StoragePerDish> mstorage;

 public:
     void sortAll()
     {
         for (auto i = mstorage.begin(); i != mstorage.end(); i++)
             i->sortNow();
     }

  list<dPoint> condimentList;
  list<StoragePerDish> getRecipe(DishType _goal) {
    //返回一个非空表，如果_goal是可制作的或者已有的；否则空表
    list<DishType> recipeList{_goal};  //未经判断的列表
    list<StoragePerDish> resultList{};

    bool isStillLoop = true;
    int loopDepth = 0;
    while (isStillLoop) {
      loopDepth++;
      isStillLoop = false;
      for (auto i = recipeList.begin(); i != recipeList.end();) {
        if (getCnt(*i) != 0) {  //如果该食材是已经有的
          StoragePerDish tStorage = getStorage(*i);
          tStorage.stepsOfProcessed = loopDepth;
          resultList.push_back(tStorage);
          i = recipeList.erase(i);  //这会使迭代器i指向下一个元素
        } else {  //递归查找是否已经有存货来合成i指向的物品
          auto findResult = Constant::CookingTable.find(*i);  // i的合成表
          if (findResult != Constant::CookingTable
                                .end()) {  // i是可合成的，即，i不是最低级原料
            isStillLoop = true;
            recipeList.erase(i);  //删除i并将i的合成表合并至recipelist
            list<DishType> resultList2 = findResult->second;
            recipeList.merge(resultList2);
            //自动重置循环
            break;
          } else {  // i是不可合成的,且经过第一层if，i是存货中没有的
            return list<StoragePerDish>();  //返回一个空表
          }
        }
      }
    }
    return resultList;
  }

 public:
  Storage() {
    for (int i = 0; i < int(DishSize3); i++)
      mstorage.push_back(StoragePerDish(DishType(i), 0));
  }

  void add(DishType _dishType, dPoint _pos) {
    if (_dishType == 0 || _dishType>=DishSize3)
      return;
    mstorage[int(_dishType)].cnt += 1;
    mstorage[int(_dishType)].posList.push_back(dPoint(_pos));
  }
  void addcondiment(dPoint _pos) {  
    condimentList.push_back(_pos);
  }
  int getCnt(DishType _dishType) { return mstorage[int(_dishType)].cnt; }

  StoragePerDish getStorage(DishType _dishType) {
    return mstorage[int(_dishType)];
  }

  list<dPoint> getStoragePos(DishType _dishType) {
    return mstorage[int(_dishType)].posList;
  }

  list<DishType> getDeficient(DishType _goal) {
    //返回所缺少的材料的List
    list<DishType> recipeList{_goal};  //未经判断的列表
    list<DishType> resultList{};

    bool isStillLoop = true;

    while (isStillLoop) {
      isStillLoop = false;
      for (auto i = recipeList.begin(); i != recipeList.end();) {
        if (getCnt(*i) != 0) {  //如果该食材是已经有的，就跳过这个食材
          i = recipeList.erase(i);  //这会使迭代器i指向下一个元素
        } else {  //递归查找是否已经有存货来合成i指向的物品
          auto findResult = Constant::CookingTable.find(*i);  // i的合成表
          if (findResult != Constant::CookingTable
                                .end()) {  // i是可合成的，即，i不是最低级原料
            isStillLoop = true;
            recipeList.erase(i);  //删除i并将i的合成表合并至recipelist
            list<DishType> resultList2 = findResult->second;
            recipeList.merge(resultList2);
            break;  //重置循环
          } else {  // i是不可合成的,且经过第一层if，i是存货中没有的
            resultList.push_back(*i);
            i = recipeList.erase(i);
          }
        }
      }
    }
    return resultList;
  }
  //isinclude指示是否把食物生产点也算进来，默认为1，也就是算进来
  void updatestorage(int isinclude=1) {
    condimentList.clear();
    mstorage.clear();
    for (int i = 0; i < int(DishSize3); i++)
      mstorage.push_back(StoragePerDish(DishType(i), 0));

    get_surround();
    cout << "get surround finish";
    for (int k = 0; k <= 80; k++)
    {
        cout << " xy pos " << surround[k][0]<<" " << surround[k][1] << endl;
        if (surround[k][0] <= 0 || surround[k][0] >= 49 || surround[k][1] <= 0 || surround[k][1] >= 49) continue;
        list<Obj> l = MapInfo::get_mapcell(surround[k][0], surround[k][1]);
        if (l.empty())continue;
        for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
        {
            cout << "not empty" << endl;
            if (i->blockType == 2 && isinclude == 0) continue;
            if (i->dish != 0 && i->objType != People)  //别把人手上的食材算进来了
            {
                cout << "try add  ";
                cout << "obj: " << i->objType << "  add dish i=" << i->dish << " pos : " << i->position.x << " " << i->position.y << endl;
                add(i->dish, dPoint(i->position.x, i->position.y));
                cout << "add finish" << endl;
            }
            if (i->tool == Condiment && i->objType != People)  //调料放单独一个list吧
            {
                cout << "try add  ";
                addcondiment(dPoint(i->position.x, i->position.y));
                cout << "add condiment " << endl;
            }
        }
    }
    cout << "    add food finish" << endl;
    //对每个食材的poslist排序
    sortAll();
    cout << endl << "*****************list begin ********************" << endl;
    for (int j = 10; j <= 21; j++) {
      list<StoragePerDish> st = getRecipe(DishType(j));
      cout << "try make :" << j << endl;
      for (auto i : st) {
        cout << "Dish :" << i.type << " step:" << i.stepsOfProcessed << endl;
      }
    }
    cout << "***************list end *********************" << endl << endl;
  }
  void cout_storage() {
    for (auto i = mstorage.begin(); i != mstorage.end(); i++) {
      cout << "Dish:" << i->type << " cnt:" << i->cnt << endl;
    }
  }
  int getStorageSize() {
    int count = 0;
    for (int i = 1; i <= 8; i++) {
      if (getCnt(DishType(i)) != 0)
        count++;
    }
    cout << "size = " << count << endl;
    return count;
  }

  int getSpiceSize() {
      int count = 0;
      for (int i = 5; i <= 8; i++) {
          if (getCnt(DishType(i)) != 0)
              count++;
      }
      cout << "spice size = " << count << endl;
      return count;
  }

};

Storage mystorage;
Storage nullptrstorage;
////////////////////////////////////////////////////////////////
int getCookedTime(DishType _goal) {
    //返回0如果是最低级材料
    if (_goal < 0 || _goal > SpicedPot6)
        return 0;//非法输入
    if (_goal >= SpicedPot && _goal <= SpicedPot6)
        return Constant::DishInfo.at(SpicedPot).CookTime;
    list<DishType> recipeList{ _goal };
    int result = 0;
    bool isStillLoop = true;
    while (isStillLoop) {
        isStillLoop = false;
        for (auto i = recipeList.begin(); i != recipeList.end();) {
            //递归查找是否已经有存货来合成i指向的物品
            auto findResult = Constant::CookingTable.find(*i);  // i的合成表
            if (findResult !=
                Constant::CookingTable.end()) {  // i是可合成的，即，i不是最低级原料
                isStillLoop = true;
                result += Constant::DishInfo.at(*i).CookTime;
                recipeList.erase(i);  //删除i并将i的合成表合并至recipelist
                list<DishType> resultList2 = findResult->second;
                recipeList.merge(resultList2);
                break;//自动重置循环
            }
            else
                i = recipeList.erase(i);//i是不可合成的,删去i
        }
    }
    return result;
}

DishType getGoal(list<DishType> raws) {
  //返回可以制作的目标，如果不能找到，则返回DishEmpty
  raws.sort();  //先把传入数组从小到大排序
  for (auto i = Constant::CookingTable.begin();
       i != Constant::CookingTable.end();
       i++) {  //遍历合成表，查询有无和传入参数相同的表
    if (raws.size() != i->second.size())
      continue;
    auto p = raws.begin();
    auto q = i->second.begin();
    while (true) {
      if (p == raws.end())
        return DishType(i->first);
      if (int(*p++) != int(*q++))
        break;
    }
  }
  return DishType::DishEmpty;
}

void use_dest(int type, Point dest);

class Astar {
 public:
  Astar() {
    initmaze=std::vector<std::vector<short>>(50, std::vector<short>(50));
    for (int row = 0; row < 50; row++)
      for (int col = 0; col < 50; col++)
        initmaze[row][col] = init_mapinfo[row][col] ? 1 : 0;
    resetmap();
  }
  void setmap(int x, int y, int label) {
    maze[x][y] = label;  //临时改变map
  }
  void resetmap() {
    maze = initmaze;  //恢复初始map
  }
  int getmap(int x, int y) { return maze[x][y] ? 1 : 0;
  }
  std::list<char> GetPath(double x, double y, bool isIgnoreCorner);
  std::list<char> gotolist;  // qweadzxc
  std::vector<std::vector<short>> maze;

  bool mazeUpdate() {
      resetmap();
      //cout << "reset finish" << endl;
      int xL = (PlayerInfo.position.x - PlayerInfo.sightRange - 1) < 0
          ? 0
          : (PlayerInfo.position.x - PlayerInfo.sightRange - 1),
          xR = (PlayerInfo.position.x + PlayerInfo.sightRange + 2) > 50
          ? 50
          : (PlayerInfo.position.x + PlayerInfo.sightRange + 2),
          yB = (PlayerInfo.position.y - PlayerInfo.sightRange - 1) < 0
          ? 0
          : (PlayerInfo.position.y - PlayerInfo.sightRange - 1),
          yT = (PlayerInfo.position.y + PlayerInfo.sightRange + 2) > 50
          ? 50
          : (PlayerInfo.position.y + PlayerInfo.sightRange + 2);
      for (int x = xL; x < xR; x++)
          for (int y = yB; y < yT; y++) {
              //cout << "getmap " << x << "," << y << endl;
              if (getmap(x, y))
                  continue;  // if originally impassable
              auto result = MapInfo::get_mapcell(x, y);
              if (result.empty())
                  continue;  // out of sight or the map
              for (auto obj : result)
                  if (obj.objType == People && x != int(PlayerInfo.position.x) && y != int(PlayerInfo.position.y))
                  {
                      setmap(x, y, 1);  // marked as impassable
                      cout << "meet people in " << x << "," << y << endl;
                      if ((PlayerInfo.tool == Bow || PlayerInfo.tool == ThrowHammer)&& obj.team!=PlayerInfo.team )
                      {
                          use_dest(1, Point(obj.position.x, obj.position.y));
                          tSleep(50);
                      }
                      break;
                  }
          }
      setmap(int(PlayerInfo.position.x), int(PlayerInfo.position.y), 0);
      return true;
  }

 private:
  Point* findPath(Point& startPoint, Point& endPoint, bool isIgnoreCorner);
  std::vector<Point*> getSurroundPoints(const Point* point,
                                        bool isIgnoreCorner) const;
  std::vector<Point*> getSurroundPoints_only(const Point* point,
                                             bool isIgnoreCorner) const;
  bool isCanreach(const Point* point,
                  const Point* target,
                  bool isIgnoreCorner) const;  //判断某点是否可以用于下一步判断
  Point* isInList(const std::list<Point*>& list,
                  const Point* point) const;  //判断开启/关闭列表中是否包含某点
  Point* getLeastFpoint();  //从开启列表中返回F值最小的节点
                            //计算FGH值
  int calcG(Point* temp_start, Point* point);
  int calcH(Point* point, Point* end);
  int calcF(Point* point);

 private:
  std::list<Point*> openList;   //开启列表
  std::list<Point*> closeList;  //关闭列表
  std::vector<std::vector<short>> initmaze;
};

inline int Astar::calcG(Point* temp_start, Point* point) {
  int extraG =
      (abs(point->x - temp_start->x) + abs(point->y - temp_start->y)) == 1
          ? kCostStraight
          : kCostDiagonal ;
  int parentG = point->parent == nullptr
                    ? 0
                    : point->parent->G;  //如果是初始节点，则其父节点是空
  return parentG + extraG;
}

inline int Astar::calcH(Point* point, Point* end) {
  //用简单的欧几里得距离计算H，这个H的计算是关键，还有很多算法，没深入研究^_^
  return sqrt(double(end->x - point->x) * double(end->x - point->x) +
              double(end->y - point->y) * double(end->y - point->y)) *
         kCostStraight;
}

inline int Astar::calcF(Point* point) {
  return point->G + point->H;
}

Point* Astar::getLeastFpoint() {
  if (!openList.empty()) {
    auto resPoint = openList.front();
    for (auto& point : openList)
      if (point->F < resPoint->F)
        resPoint = point;
    return resPoint;
  }
  return nullptr;
}

Point* Astar::findPath(Point& startPoint,
                       Point& endPoint,
                       bool isIgnoreCorner) {
  openList.push_back(new Point(
      startPoint.x, startPoint.y));  //置入起点,拷贝开辟一个节点，内外隔离
  while (!openList.empty()) {
    auto curPoint = getLeastFpoint();  //找到F值最小的点
    openList.remove(curPoint);         //从开启列表中删除
    closeList.push_back(curPoint);     //放到关闭列表
                                    ////1,找到当前周围八个格中可以通过的格子
    auto surroundPoints =
        getSurroundPoints(curPoint, isIgnoreCorner);  // only就是只能上下左右了
    for (auto& target : surroundPoints) {
      // 2,对某一个格子，如果它不在开启列表中，加入到开启列表，设置当前格为其父节点，计算F
      // G H
      if (!isInList(openList, target)) {
        target->parent = curPoint;
        target->G = calcG(curPoint, target);
        target->H = calcH(target, &endPoint);
        target->F = calcF(target);
        openList.push_back(target);
      }  // 3，对某一个格子，它在开启列表中，计算G值, 如果比原来的大,
         // 就什么都不做, 否则设置它的父节点为当前点,并更新G和F
      else {
        int tempG = calcG(curPoint, target);
        if (tempG < target->G) {
          target->parent = curPoint;
          target->G = tempG;
          target->F = calcF(target);
        }
      }
      Point* resPoint = isInList(openList, &endPoint);
      if (resPoint)
        return resPoint;  //返回列表里的节点指针，不要用原来传入的endpoint指针，因为发生了深拷贝
    }
  }
  return nullptr;
}
int sgn(double x) {
  if (x > 0)
    return 1;
  if (x == 0)
    return 0;
  if (x < 0)
    return -1;
}
inline std::list<char> Astar::GetPath(double x, double y, bool isIgnoreCorner) {
  Point startPoint((int)(PlayerInfo.position.x),
                   (int)(PlayerInfo.position.y));  //网上往右是+
  cout << "start:" << PlayerInfo.position.x << "," << PlayerInfo.position.y
       << "end with " << x << "," << y << endl;
  Point endPoint(x, y);
  if ((int)(PlayerInfo.position.x) == x && (int)(PlayerInfo.position.x) == y)
  {
      gotolist.clear();
      gotolist.push_back('s');
      return gotolist;
  }
  // A*算法找寻路径
  Point* result = findPath(startPoint, endPoint, isIgnoreCorner);
  std::list<Point*> path;  //返回路径，如果没找到路径，返回空链表
  while (result) {
    path.push_front(result);
    result = result->parent;
  }  // 清空临时开闭列表，防止重复执行GetPath导致结果异常
  openList.clear();
  closeList.clear();
  // return path
  list<Point*>::iterator p;
  list<char>::iterator lp;
  gotolist.clear();
  cout << "generate list  ";
  for (p = path.begin(); p != path.end();) {
      Point* pre = *(p);
      if (++p == path.end())
          break;
      int movex = (*p)->x - pre->x;
      // dir[3][3] = { {'z','x','c' },{'a','s','d'},{'q','w','e'} };
      int movey = (*p)->y - pre->y;
      char c = dir[movey + 1][movex + 1];
      cout << c << ' ';
      gotolist.push_back(c);  //例如dir[2][1]=w,dir[0][2]=c
  }
  cout << endl << "return gotolist" << endl;
  return gotolist;
}

Point* Astar::isInList(const std::list<Point*>& list, const Point* point)
    const {  //判断某个节点是否在列表中，这里不能比较指针，因为每次加入列表是新开辟的节点，只能比较坐标
  for (auto p : list)
    if (p->x == point->x && p->y == point->y)
      return p;
  return nullptr;
}

inline bool Astar::isCanreach(const Point* point,
                       const Point* target,
                       bool isIgnoreCorner) const {
  if (target->x < 0 || target->x > maze.size() - 1 || target->y < 0 ||
      target->y > maze[0].size() - 1 || maze[target->x][target->y] == 1 ||
      target->x == point->x && target->y == point->y ||
      isInList(
          closeList,
          target))  //如果点与当前节点重合、超出地图、是障碍物、或者在关闭列表中，返回false
    return false;
  else {
    if (abs(point->x - target->x) + abs(point->y - target->y) ==
        1)  //非斜角可以
      return true;
    else {  //斜对角要判断是否绊住
      if (maze[point->x][target->y] == 0 && maze[target->x][point->y] == 0)
        return true;
      else
        return isIgnoreCorner;
    }
  }
}
//只保留上下左右四个方向，不然太容易卡住了呜呜呜

std::vector<Point*> Astar::getSurroundPoints_only(const Point* point,
                                                  bool isIgnoreCorner) const {
  std::vector<Point*> surroundPoints;
  int x = point->x - 1;
  int y = point->y;
  if (isCanreach(point, new Point(x, y), isIgnoreCorner))
    surroundPoints.push_back(new Point(x, y));
  x = point->x + 1;
  y = point->y;
  if (isCanreach(point, new Point(x, y), isIgnoreCorner))
    surroundPoints.push_back(new Point(x, y));
  x = point->x;
  y = point->y + 1;
  if (isCanreach(point, new Point(x, y), isIgnoreCorner))
    surroundPoints.push_back(new Point(x, y));
  x = point->x;
  y = point->y - 1;
  if (isCanreach(point, new Point(x, y), isIgnoreCorner))
    surroundPoints.push_back(new Point(x, y));
  return surroundPoints;
}

std::vector<Point*> Astar::getSurroundPoints(const Point* point,
                                             bool isIgnoreCorner) const {
  std::vector<Point*> surroundPoints;
  for (int x = point->x - 1; x <= point->x + 1; x++)
    for (int y = point->y - 1; y <= point->y + 1; y++)
      if (isCanreach(point, new Point(x, y), isIgnoreCorner))
        surroundPoints.push_back(new Point(x, y));
  return surroundPoints;
}


Astar astar;
map<char, Protobuf::Direction> ctodire{
    {'d', Protobuf::Direction::Right}, {'e', Protobuf::Direction::RightUp},
    {'w', Protobuf::Direction::Up},    {'q', Protobuf::Direction::LeftUp},
    {'a', Protobuf::Direction::Left},  {'z', Protobuf::Direction::LeftDown},
    {'x', Protobuf::Direction::Down},  {'c', Protobuf::Direction::RightDown}};
bool isDiagonal(char c) {
  if (c == 'q' || c == 'e' || c == 'z' || c == 'c')
    return true;
  return false;
}

//重载一下，i=1时只移动50ms表示时间，主要用于修改朝向。
void move_dir(char c, int i) {
  if (c == 's')return;
  //cout << "move only 50 * " << i << endl;
  tMove(ctodire[c], 50 * i);
  tSleep(50 * i);
}
//按照c中表示的方向移动
void move_dir(char c) {
  double speed = PlayerInfo.moveSpeed;
  if (c == 's')return;
  if (speed == 4) {
    tMove(ctodire[c], 250 + 100 * isDiagonal(c));
    tSleep(250 + 100 * isDiagonal(c));
  } else  // speed==9,
  {
    int time = 100;  //希望移动的期望是111，要求有0.25
    if (rand() % 5 == 0)
      time = 150;
    tMove(ctodire[c], isDiagonal(c) ? 150 : time);
    tSleep(isDiagonal(c) ? 150 : time);
  }
}
double calcdis(Point end)
{
    double x = PlayerInfo.position.x;
    double y = PlayerInfo.position.y;
    return sqrt(((double)end.x+0.5 - x) *
        ((double)end.x + 0.5 - x) +
        ((double)end.y + 0.5 - y) *
        ((double)end.y + 0.5 - y));
}
double calcdis(Point point, Point end) {
  //用简单的欧几里得距离计算H，这个H的计算是关键，还有很多算法，没深入研究^_^
  return sqrt(((double)end.x - (double)point.x) *
                  ((double)end.x - (double)point.x) +
              ((double)end.y - (double)point.y) *
                 ((double)end.y - (double)point.y));
}

////////////////////////////////////////////////////////////////////////////////

Point findnearfood()  //找最近的食物生成点
{
  Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);
  cout << "get dis" << endl;
  get_foodgen_dis();
  cout << "sort begin" << endl;
  sort(foodgen.begin(), foodgen.end(), sort_by_3);
  vector<int> destinfo = *foodgen.begin();
  Point nearest(destinfo[1], destinfo[2]);
  sort(foodgen.begin(), foodgen.end(), sort_by_0);
  return nearest;
}

Point findsecondfood()  //找第二近的食物生成点
{
  Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);
  get_foodgen_dis();
  sort(foodgen.begin(), foodgen.end(), sort_by_3);
  vector<int> destinfo = foodgen[1];
  Point secondp(destinfo[1], destinfo[2]);
  sort(foodgen.begin(), foodgen.end(), sort_by_0);
  return secondp;
}

char dir_4[5] = {0, 'a', 'w', 'd', 'x'};  //用这个数组存一下最后一步的方向
double angle_4[5] = {0, PI, PI / 2, 0, -PI / 2};  //用这个数组存一下最后扔的方向

//player2换一个表
/*
int cooklabel[5][5] = {{},
                       {8, 25, 1, 4, 0},
                       {25, 37, 2, 2, 0},
                       {40, 28, 3, 3, 0},
                       {33, 17, 4, 2, 0}};
*/
int cooklabel[5][5] = { {24,24,},
                       {7, 24, 1, 3, 0},
                       {24, 38, 2, 3, 0},
                       {41, 27, 3, 2, 0},
                       {33, 19, 4, 4, 0} };
//第一行都是空着备用。
//【灶台边空地坐标xy,灶台编号，最后一步的方向,label(检查的时候方便吧大概)，在做菜吗】

//新款，灶台本体位置，这里【】【3】没用
int cookonly[5][5] = { {25,25,},
                       {8, 24, 1, 4, 0},
                       {25, 38, 2, 2, 0},
                       {41, 28, 3, 3, 0},
                       {33, 18, 4, 2, 0} };



Point findfarfood(int isrand=0)  //找最远的食物生成点
{
    Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);
    get_foodgen_dis();
    sort(foodgen.begin(), foodgen.end(), sort_by_3);    //排序玩后0-7有效
    if (isrand == 0)
    {
        vector<int> destinfo = foodgen[7];
        cout << "dest info" << destinfo[1] << "," << destinfo[2] << endl;
        Point p(destinfo[1], destinfo[2]);
        sort(foodgen.begin(), foodgen.end(), sort_by_0);
        return p;
    }
    else
    {
        vector<int> destinfo = foodgen[rand() % 3 + 5];
        Point p(destinfo[1], destinfo[2]);
        cout << "rand dest info" << destinfo[1] << "," << destinfo[2] << endl;
        sort(foodgen.begin(), foodgen.end(), sort_by_0);
        return p;
    }
}

int findnearcook()  //找最近的灶台
{
  Point Point1(8, 25), Point2(25, 37), Point3(40, 28),
      Point4(33, 17);  //四个灶台
  Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);
  double dis1 = calcdis(Point1, Pos);
  double dis2 = calcdis(Point2, Pos);
  double dis3 = calcdis(Point3, Pos);
  double dis4 = calcdis(Point4, Pos);
  if (dis1 <= dis2 && dis1 <= dis3 && dis1 <= dis4)
    return 1;
  if (dis2 <= dis1 && dis2 <= dis3 && dis2 <= dis4)
    return 2;
  if (dis3 <= dis1 && dis3 <= dis2 && dis3 <= dis4)
    return 3;
  if (dis4 <= dis1 && dis4 <= dis2 && dis4 <= dis3)
    return 4;
}

//从字符获得下一步的x和y怎么变化
int nextx(char c) {
  if (c == 'd' || c == 'e' || c == 'c')
    return 1;
  if (c == 'a' || c == 'q' || c == 'z')
    return -1;
  else
    return 0;
}

int nexty(char c) {
  if (c == 'w' || c == 'q' || c == 'e')
    return 1;
  if (c == 'x' || c == 'z' || c == 'c')
    return -1;
  else
    return 0;
}
Point getnear(Point dest)
{  //如果四个都赌上了，返回
    int x, y;
    Point self(PlayerInfo.position.x, PlayerInfo.position.y);
    vector<vector<double>> dis;  // dis,x,y,
    //   cout << "change dest" << endl;
    if (astar.maze[dest.x][dest.y] == 0)
    {
        x = dest.x;
        y = dest.y;
        vector<double> push = { calcdis(self, Point(x, y)), double(x), double(y) };
        dis.push_back(push);
    }
    if (astar.maze[dest.x + 1][dest.y] == 0)
    {
        x = dest.x + 1;
        y = dest.y;
        vector<double> push = { calcdis(self, Point(x, y)), double(x), double(y) };
        dis.push_back(push);
    }
    if (astar.maze[dest.x - 1][dest.y] == 0)
    {
        x = dest.x - 1;
        y = dest.y;
        vector<double> push = { calcdis(self, Point(x, y)), double(x), double(y) };
        dis.push_back(push);
    }
    if (astar.maze[dest.x][dest.y + 1] == 0)
    {
        x = dest.x;
        y = dest.y + 1;
        vector<double> push = { calcdis(self, Point(x, y)), double(x), double(y) };
        dis.push_back(push);
    }
    if (astar.maze[dest.x][dest.y - 1] == 0)
    {
        x = dest.x;
        y = dest.y - 1;
        vector<double> push = { calcdis(self, Point(x, y)), double(x), double(y) };
        dis.push_back(push);
    }
    if (dis.empty() == FALSE)
    {
        sort(dis.begin(), dis.end(), sort_by_0_double);  //按距离从小到大排序
        x = (int)(*dis.begin())[1];
        y = (int)(*dis.begin())[2];
          cout << "change dest : " << x << "," << y <<"dis : "<<dis.front()[3]<<endl;
        return Point(x, y);
    }
    else  //四个点都赌上了？？开什么玩笑？？返回中间几个点吧。。
    {
        int ix, iy;
        for (ix = 23; ix <= 26; ix++)
        {
            for (iy = 23; iy <= 26; iy++)
            {
                if (astar.maze[ix][iy] == 0)
                    return Point(ix, iy);
            }
        }
    }
}

int gotodest(Point dest, int istimelimited = 0, int getdish = 0)  //默认不限制，如果限制，把20000改成cooklabel[4]-5000,返回值为1表示成功,返回0表示中断
{//getdish=1~8表示沿路定向捡食材，-1表示沿路随机捡

    if (dest.x > 50 || dest.x < 0)
    {
        tSleep(5);
        return 0;
    }
    if (dest.y > 50 || dest.y < 0)
    {
        tSleep(5);
        return 0;
    }

    list<char>::iterator lp;
    int x = dest.x;
    int y = dest.y;
    //用Getpath获得一个由字符组成的链表，字符代表移动方向，每一个是用200ms的时间走一格。
    cout << "speed" << PlayerInfo.moveSpeed << "dest x,y " << x << ","<<y << endl;

    //复原地图
    //更新地图
    astar.mazeUpdate();

    if (astar.maze[dest.x][dest.y] == 1)  //首先检测目标是否是障碍物，如果是，搜索周围四格中不是障碍物的点作为目标。
    {
        Point posnear = getnear(dest);
        x = posnear.x;
        y = posnear.y;
        cout << "change dest to " << x << ',' << y << endl;
    }
    int posx = floor(PlayerInfo.position.x);
    int posy = floor(PlayerInfo.position.y);

    if (posx == x && posy == y)  //如果已经在目的地，没必要移动。
    {
        cout << "no need move" << endl;
        return 1;
    }
    int timeinit = getGameTime();
    int time;
    while (1)
    {
        if (istimelimited == 0)
        {
            time = getGameTime() - timeinit;
            cout << "time" << time << endl;
            if (time >= 13000)
            {
                cout << "kia zhu le !!!" << endl;
                return 0;  //这么久肯定是被卡住了，放弃吧。
            }
            int calc = calcdis(dest);
            if (calc <= 5)//
            {
                //
            }
        }
        else//timelimited==1
        {
            if (cooking[4] < getGameTime() + 5000) {
                cout << "xian hui qu ba" << endl;
                return 0;  //正在做菜呢，先回去不然糊了
            }
        }
        auto pathget = astar.GetPath(x, y, false);
        for (auto& lp :pathget)
        {

            double pos_prex = PlayerInfo.position.x;
            double pos_prey = PlayerInfo.position.y;
            double pos_nextx = pos_prex + nextx(lp);
            double pos_nexty = pos_prey + nexty(lp);
            Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);

            double disc;  //和label灶台的距离，如果太近，不捡调料
            if (label == 0) {
                disc = 9999;
            }
            else {
                int cookx = cooklabel[label][0] +
                    nextx(dir_4[cooklabel[label][3]]);  //这是灶台的坐标
                int cooky = cooklabel[label][1] + nexty(dir_4[cooklabel[label][3]]);
                Point cook(cookx, cooky);
                disc = calcdis(cook, Pos);  //计算和灶台的距离，太近就不捡材料
                cout << "dis to cook = " << disc << endl;
            }
            // list<Obj> l = MapInfo::get_mapcell((int)pos_nextx,
            // (int)pos_nexty);//先检查下一个格子有没有能捡起来的或者trigger
            // trigger不检查了 就检查脚下有没有东西吧

            list<Obj> l = MapInfo::get_mapcell((int)pos_prex, (int)pos_prey);
            for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
            {
                if (i->objType == Tool && PlayerInfo.tool != Condiment)
                {
                    int t = i->tool;
                    cout << "tool" << t << endl;
                    switch (t)
                    {
                    case StrengthBuff:  // 3
                    case TeleScope:     // 4
                    case BreastPlate:   // 7 护心镜
                        tPick(TRUE, Tool, t);
                        tSleep(50);
                        if (PlayerInfo.tool == t) {
                            tUse(1, 0, 0);
                            tSleep(50);
                        }
                        cout << "use tool " << t << endl;
                        break;
                    case SpaceGate:  // 8 传送门
                        tPick(TRUE, Tool, t);
                        tSleep(50);
                        if (PlayerInfo.tool == t) {
                            tUse(1, x - (int)PlayerInfo.position.x,
                                y - (int)PlayerInfo.position.y == y);  //相对位移
                            tSleep(50);
                            cout << "use tool " << t << endl;
                            if (PlayerInfo.position.x == x && PlayerInfo.position.y == y)
                                return 1;
                        }
                        break;
                    case Condiment:  //调料
                      //对于灶台周围区域，只在做菜的时候捡起condiment，走路不捡

                        if (label != 0 && disc >= 3) {
                            tPick(TRUE, Tool, t);
                            tSleep(50);
                            cout << "pick condiment " << t << endl;
                        }
                        else {
                            cout << "near cook not pick condiment!" << endl;
                        }
                        break;
                    case LandMine:
                    case TrapTool:
                    case FlashBomb:
                        tPick(TRUE, Tool, t);
                        tSleep(50);
                        if (PlayerInfo.tool == t) {
                            cout << "put trigger " << t << endl;
                            tUse(1, 0, 0);
                            tSleep(50);
                        }
                    case ThrowHammer:
                    case Bow:
                        tPick(TRUE, Tool, t);
                        tSleep(50);
                    default:
                        break;
                    }
                    // if (i->objType == 4) cout << "trigger" << i->trigger << endl;


                }
                if (i->objType == Dish && PlayerInfo.dish == 0 && getdish == -1)
                {
                    tPick(TRUE, Dish, i->dish);
                    tSleep(50);
                    cout << "pick dish " << i->dish << endl;
                }
            }

            move_dir(lp);  //处理完刚刚的问题再移动
            // cout << "form" << PlayerInfo.position.x << "  " <<
            // PlayerInfo.position.y << "  move:" << lp << endl;
            if (PlayerInfo.position.x == pos_prex && PlayerInfo.position.y == pos_prey)
            {
                cout << "error!" << PlayerInfo.position.x << ","
                    << PlayerInfo.position.y << endl;
                // char dir_4[5] = { 0,'a','w','d','x'
                // };//用这个数组存一下最后一步的方向
                int rand1 = rand() % 4 + 1;  // 1,2,3,4
                int rand2 = rand() % 2 - 1;  //-1,0,1  0~5
                int add12 = rand1 + rand2;
                if (add12 == 0)  add12 = 4;
                if (add12 == 5)  add12 = 1;
                cout << "random move" << dir_4[rand1] << dir_4[add12] << endl;
                ;
                move_dir(dir_4[rand1]);
                move_dir(dir_4[add12]);
                break;  // break for 循环
            }  //如果移动失败了，随机向某个方向移动一格，无论是否成功，都break循环，重新生成Getpath
               // cout << THUAI3::getGameTime() << endl;

            //复原地图
            //更新地图
            astar.mazeUpdate();

            if (astar.maze[x][y] == 1)  //重新检测目标是否是障碍物，找dest周围的点
            {
                Point posnear = getnear(dest);
                x = posnear.x;
                y = posnear.y;
                cout << " meet people" << endl;
                break;//break for循环
            }
        }
        if ((int)PlayerInfo.position.x == (int)x && (int)PlayerInfo.position.y == (int)y)
        {
            cout << "move finish!" << x << ',' << y << endl;
            return 1;
        }
        //直到到达目的地再break循环。
    }
    return 1;
}

void smallmove(
    double x,
    double
        y)  //仅用于微小的移动，通过50ms=0.25个单位来实现,即差距乘4，四舍五入之后移动50ms
{
  double posx = PlayerInfo.position.x;
  double posy = PlayerInfo.position.y;
  double detx = x - posx;
  double dety = y - posy;
  double step = PlayerInfo.moveSpeed / 20;  //每50ms移动step格。
  if (abs(detx) > 2 || abs(dety) > 2)
    return;
  if (detx >= step) {
    int count = round(detx / step);
    move_dir('d', count);  //右移50
  }
  if (detx <= -step) {
    int count = round(abs(detx / step));
    move_dir('a', count);  //左移50
  }
  if (dety >= step) {
    int count = round(dety / step);
    move_dir('w', count);  //右移50
    count--;
  }
  if (dety <= -step) {
    int count = round(abs(detx / step));
    move_dir('x', count);  //左移50
  }
}

//走到灶台后第一件事检查黑暗料理
int throw_darkdish(
    int _label)  //检查灶台上有无黑暗料理，如果有，尝试拾取，拾取成功就扔掉，拾取失败就标记这个灶台不行，找下一个灶台
{
  int rand1 = rand() % 4 + 1;  // 1,2,3,4
  int rand2 = rand() % 2 - 1;  //-1,0,1  0~5
  int add12 = rand1 + rand2;
  if (add12 == 0)
    add12 = 4;
  if (add12 == 5)
    add12 = 1;
  if (cooklabel[_label][4] == 1)
    return 0;  //这个灶台是自己正在做的，不要扔黑暗料理
  //有时候自己做了菜，但是没有拿到手，这时候会把label变成0，但是这还是我的菜啊
  int x = cooklabel[_label][0] + nextx(dir_4[cooklabel[_label][3]]);
  int y = cooklabel[_label][1] + nexty(dir_4[cooklabel[_label][3]]);
  char c = dir_4[cooklabel[_label][3]];
  list<Obj> l = MapInfo::get_mapcell(x, y);
  string stop("s");  //精细操作，要求对方停下来
  speakToFriend(stop);
  int mydish = PlayerInfo.dish;
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
    if (i->blockType == 3 && i->dish != 0)  //如果发现灶台里面有菜
    {
      cout << "dish in cook!" << endl;
      tPut(0, PI, TRUE);       //先把手里的东西放脚下
      move_dir(c, 0);         //确保朝向
      tPick(FALSE, Block, 0);  //尝试拾取
      tSleep(50);
      cout << "DISH IN BLOCK  " << i->dish << "  _label  " << _label
           << " label " << label << "  is my cook  " << cooklabel[label][4]
           << endl;
      if (PlayerInfo.dish == 0)  //说明没有拿到dish,也就是这个灶台正在烹饪
      {
        //随便走两步，再拿一下试试
        cout << "random move" << dir_4[rand1] << dir_4[add12] << endl;
        ;
        move_dir(dir_4[rand1]);
        move_dir(dir_4[add12]);
        gotodest(Point(cooklabel[_label][0], cooklabel[_label][1]));
        smallmove(cooklabel[_label][0] + 0.5, cooklabel[_label][1] + 0.5);
        move_dir(c, 1);         //确保朝向
        tPick(FALSE, Block, 0);  //尝试拾取
        if (PlayerInfo.dish == 0) {
          if (label == _label)
            label = 0;  //如果这就是我标记的据点，那么取消标记。
          tPick(TRUE, Dish, mydish);
          return 1;  //继续跑第二近的灶台。
        }
      }
      if (PlayerInfo.dish >= OverCookedDish)  //如果拿到了黑暗料理，扔出去
      {
        cout << "throw dark dish  " << PlayerInfo.dish << endl;
        tPut(2, 0, TRUE);  //先往右扔两格,我下次看看最多能扔多远……
        // mystorage.add((DishType)mydish, dPoint(PlayerInfo.position.x,
        // PlayerInfo.position.y));
        return 2;  //用这个灶台做菜，不要慌
      } else       //不是黑暗料理！捡到宝了！
      {
        // mystorage.add((DishType)mydish, dPoint(PlayerInfo.position.x,
        // PlayerInfo.position.y));
        return 3;  //准备提交食物，耶
      }
    }
  }
  return 2;  //灶台里啥都没有，直接做菜。
}

int findallcook(
    int labelofcook)  //从label开始转一圈圈,int=1表示找到了可以用的灶台，int=2表示拿到菜了。
{
  int flabel = labelofcook;
  int isfind = 0;
  while (isfind == 0) {
    flabel = labelofcook + 1;  // 2 3 4 5
    if (flabel == 5)
      flabel = 1;
    cout << "goto next label=" << flabel << endl;
    gotodest(Point(cooklabel[flabel][0],
                   cooklabel[flabel][1]));  //{x,y,编号，朝向，label}
    int nextstate = throw_darkdish(flabel);  //到达灶台后，先检查有无黑暗料理
    cout << "label  " << label << "  is my cook  " << cooklabel[label][4]
         << endl;
    stringstream slabinfo;
    string labinfo;
    switch (nextstate) {
      case 0:  //这是我的据点？我正在做菜？bug吧？
        cout << "what wrong?" << endl;
        if (label != flabel) {
          label = flabel;
          /////
          labinfo.assign(slabinfo.str());
          slabinfo << 'l' << " " << label;
          cout << "send to team:" << slabinfo.str() << endl;
          speakToFriend(labinfo);
          /////
        }
        isfind = 1;
        break;
      case 1:  //继续跑第二近的灶台。
        cout << "next next label" << endl;
        break;
      case 2:
        if (label != flabel) {
          label = flabel;  //用这个灶台好啦
                           /////
          labinfo.assign(slabinfo.str());
          slabinfo << 'l' << " " << label;
          cout << "send to team:" << slabinfo.str() << endl;
          speakToFriend(labinfo);
          /////
        }
        cout << "this is ok!" << endl;
        isfind = 1;
        return 1;
        break;
      case 3:
        cout << "get dish" << endl;
        state = 2;
        isfind = 1;
        return 2;
        break;
    }
  }
}

// 自身为中心共81格，顺序从远到近,从2，2开始顺时针绕回中心。
void get_surround()
{
    int x = PlayerInfo.position.x;
    int y = PlayerInfo.position.y;
    int k = 0;
    memset(surround, 0, sizeof surround);
    for (int i = -4; i <= 4; i++)
        for (int j = -4; j <= 4; j++)
        {
            surround[k][0] = x + i;
            surround[k][1] = y + j;
            k++;
        }
}

/*
void get_surround()
{
    int x = PlayerInfo.position.x;
    int y = PlayerInfo.position.y;
    int k = 0;
    memset(surround, 0, sizeof surround);

    for (int i = 3; i >= -3; i--)
    {
        surround[k][0] = x + 3;
        surround[k][1] = y + i;
        k++;
    }
    for (int i = 2; i >= -3; i--)
    {
        surround[k][0] = x + i;
        surround[k][1] = y - 3;
        k++;
    }
    for (int i = -2; i <= 3; i++)
    {
        surround[k][0] = x - 3;
        surround[k][1] = y + i;
        k++;
    }
    for (int i = -2; i <= 2; i++)
    {
        surround[k][0] = x + i;
        surround[k][1] = y + 3;
        k++;
    }
    for (int i = 2; i >= -2; i--)
    {
        surround[k][0] = x + 2;
        surround[k][1] = y + i;
        k++;
    }
    for (int i = 1; i >= -2; i--)
    {
        surround[k][0] = x + i;
        surround[k][1] = y - 2;
        k++;
    }
    for (int i = -1; i <= 2; i++)
    {
        surround[k][0] = x - 2;
        surround[k][1] = y + i;
        k++;
    }
    for (int i = -1; i <= 1; i++)
    {
        surround[k][0] = x + i;
        surround[k][1] = y + 2;
        k++;
    }
    for (int i = 1; i >= -1; i--)
    {
        surround[k][0] = x + 1;
        surround[k][1] = y + i;
        k++;
    }
    surround[k][0] = x;
    surround[k][1] = y - 1;
    k++;
    surround[k][0] = x - 1;
    surround[k][1] = y - 1;
    k++;
    surround[k][0] = x - 1;
    surround[k][1] = y;
    k++;
    surround[k][0] = x - 1;
    surround[k][1] = y + 1;
    k++;
    surround[k][0] = x;
    surround[k][1] = y + 1;
    k++;
    surround[k][0] = x;
    surround[k][1] = y;
    k++;
}
*/

int get_one_dish(int x, int y)  //看看食物生成点有没有食物
{
  list<Obj> l = MapInfo::get_mapcell(
      x, y);  //先扫一遍食物生成点，看看有没有食物，如果有返回0
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
    if (i->objType == Block && i->dish != 0) {
      cout << "dish:" << i->dish << endl;
      return 0;  //
    }
  }
  l = MapInfo::get_mapcell((int)PlayerInfo.position.x,
                           (int)PlayerInfo.position.y);
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
    if (i->objType == Dish && i->dish != 0) {
      cout << "dish:" << i->dish << endl;
      return i->dish;  //再扫一遍脚下，如果有返回dish
    }
  }
  return -1;  //如果啥都没有，返回-1
}

int pick_dish_in_block(Point food, int timelimit = 0) {  // Point 是食物生成点

  if (((int)PlayerInfo.position.x) != food.x ||
      ((int)PlayerInfo.position.y) != food.y) {
    gotodest(Point(food.x, food.y));
    int movex = sgn((int)food.x - (int)PlayerInfo.position.x);
    int movey = sgn((int)food.y - (int)PlayerInfo.position.y);
    char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
    move_dir(fooddir, 1);                      //调整朝向
  }
  int dish = get_one_dish((int)food.x, (int)food.y);
  while (dish == -1)  //如果dish=-1说明没有食物,就一直等着
  {
    tSleep(50);
    dish = get_one_dish((int)food.x, (int)food.y);
    cout << "wait for dish" << endl;
    if (((int)PlayerInfo.position.x) != food.x ||
        ((int)PlayerInfo.position.y) != food.y) {
      gotodest(Point(food.x, food.y));
      int movex = sgn((int)food.x - (int)PlayerInfo.position.x);
      int movey = sgn((int)food.y - (int)PlayerInfo.position.y);
      char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
      move_dir(fooddir, 1);                      //调整朝向
    }
    if (timelimit) {
      if (cooking[4] < getGameTime() + 5000) {
        return 0;
        cout << "time limited! go back!" << endl;
      }
    }
  }
  if (dish == 0) {
    tPick(FALSE, Block, 0);  //是block的时候第三个随便输入,表示捡起block里的食材
    tSleep(50);
    cout << "pick dish in block finish" << endl;
    move_dir(dir_4[rand() % 4 + 1]);  //随机走一下，防止卡住

  } else {
    tPick(TRUE, Dish, dish);  //捡起脚下的食材
    tSleep(50);
    cout << "pick dish on the ground finish" << endl;
    move_dir(dir_4[rand() % 4 + 1]);  //随机走一下，防止卡住
  }
  return PlayerInfo.dish;
}

int xiangguo;

int whichfood() 
{
  int which = 0;
  /*
  if (find(task_list.begin(), task_list.end(), SpicedPot) != task_list.end()) 
  {
    int sizeraw = mystorage.getSpiceSize();
    
    if (sizeraw >= 3 && mystorage.condimentList.empty() != TRUE && getGameTime()<=510000)  {
      cout << "can make spice pot!" << endl;
      return SpicedPot;  //如果有调料，而且食材够，可以做香锅。
    }
  }
  */
  cout << "which food to make?" << endl;
  if (mystorage.getRecipe(Flour).empty() == FALSE &&
      mystorage.getCnt(DishType(Flour)) == 0)
    which = Flour;
  if (mystorage.getRecipe(Noodle).empty() == FALSE &&
      mystorage.getCnt(DishType(Noodle)) == 0)
    which = Noodle;
  if (mystorage.getRecipe(Ketchup).empty() == FALSE &&
      mystorage.getCnt(DishType(Ketchup)) == 0)
    which = Ketchup;
  if (mystorage.getRecipe(Bread).empty() == FALSE &&
      mystorage.getCnt(DishType(Bread)) == 0)
    which = Bread;

  //做性价比最高的菜
  findbestdish();  //编号 性价比，已完成排序
  for (auto i : bestdish) {
    int dish = i[0];
    if (mystorage.getRecipe(DishType(dish)).empty() == FALSE &&
        mystorage.getCnt(DishType(dish)) == 0) {
      cout << "make best food" << endl;
      which = dish;  //优先制作性价比高的菜
    }
  }
  //最高优先级：任务列表里有的
  list<DishType> tk = task_list;
  list<StoragePerDish> tododish;
  for (list<DishType>::iterator i = tk.end(); i != tk.begin(); i--) {
    if (*i <= DarkDish && *i > 0 && mystorage.getRecipe(*i).empty() == FALSE &&
        mystorage.getCnt(*i) == 0) {
      cout << "make food in task list!  Dish:" << *i << endl;
      which = *i;  //优先制作任务列表里有的菜
    }
  }

  int maxstep = 0;
  //检查一下刚刚到底要做的是什么

  if (which == TomatoFriedEggNoodle)  //如果要做的是西红柿鸡蛋面
  {
    int tomatoegg = mystorage.getCnt(DishType(TomatoFriedEgg)),
        noodle = mystorage.getCnt(DishType(Noodle)),
        flour = mystorage.getCnt(DishType(Flour));
    if (tomatoegg != 0 && noodle != 0) {
      return TomatoFriedEggNoodle;
    }
    if (tomatoegg == 0) {
      return TomatoFriedEgg;  //如果没有西红柿炒蛋，那就做
    }
    if (noodle == 0)  //有西红柿炒鸡蛋和面粉或小麦
    {
      if (flour == 0)
        return Flour;
      return Noodle;
    }
  } else {
    list<StoragePerDish> st = mystorage.getRecipe(DishType(which));
    for (auto i : st) {
      if (i.stepsOfProcessed > maxstep)
        maxstep = i.stepsOfProcessed;
    }
    if (maxstep == 2)
      return which;  //一步完成
    else {
      list<DishType> raw;
      for (auto i : st) {
        if (i.stepsOfProcessed ==
            maxstep)  //看看步数等于最大步数时，能做哪些中间产物
        {
          raw.push_back(i.type);
        }
      }
      which = getGoal(raw);
    }
  }
  return which;
}

// const char dir[3][3] = { {'z','x','c' },{'a','s','d'},{'q','w','e'} };

double angle_abs(Point dest)  //啊 是弧度
{
  double x = dest.x + 0.5 - PlayerInfo.position.x;
  double y = dest.y + 0.5 - PlayerInfo.position.y;
  cout << "destx=" << dest.x << ", posx=" << PlayerInfo.position.x << endl;
  cout << "desty=" << dest.y << ", posy=" << PlayerInfo.position.y << endl;
  if (x == 0) {
    x += 0.00000001;
  }
  if (y == 0) {
    y += 0.00000001;
  }                    //不知道=0会不会崩，偏一点点吧。
  return atan2(y, x);  // atan2返回弧度
}

void use_dest(int type, Point dest)  //计算从当前位置到目标位置，需要的角度和距离,目标x+0.5,y+0.5才是中心点
{
    double angle = angle_abs(dest);
    double dis = calcdis(dest);
    cout << "use:" << PlayerInfo.tool << " dis=" << dis << "   angle:" << angle << endl;
    tUse(type, dis, angle);
    tSleep(50);
    return;
}

void put_dest(Point dest,bool isdish)  //计算从当前位置到目标位置，需要的角度和距离,目标x+0.5,y+0.5才是中心点
{
  double angle = angle_abs(dest);
  double dis =
      calcdis(dest);
  cout << "put: dis=" << dis << "   angle:" << angle << endl;
  tPut(dis, angle, isdish);
  tSleep(50);
  return;
}

void move_allfood_to_left() {
  //先把灶台里所有的东西都丢到左边,并且修改刚刚的坐标到左边一格
  cout << "move all food to right" << endl;
  int cookx =
      cooklabel[label][0] + nextx(dir_4[cooklabel[label][3]]);  //这是灶台的坐标
  int cooky = cooklabel[label][1] + nexty(dir_4[cooklabel[label][3]]);
  char c = dir_4[cooklabel[label][3]];  //朝向
  gotodest(Point(cooklabel[label][0], cooklabel[label][1]));
  move_dir(c, 1);                                    //调整朝向
  list<Obj> l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
    cout << "Obj TYPE :" << i->objType << " DISH:" << i->dish << endl;
    if (i->objType == Dish && i->dish != 0) {
      tPick(FALSE, Dish, i->dish);
      tSleep(50);
      tPut(1, PI, TRUE);  //扔到左边，修改坐标为本人左一格。
      tSleep(50);
    }
  }
}

//捡调料，参数为1要扔到灶台，参数为0不扔
int getcondiment(int isthrow) {
  if (mystorage.condimentList.empty() == TRUE)
    return 0;

  Point foodpos(mystorage.condimentList.begin()->x,
                mystorage.condimentList.begin()->y);  //查看食材所在位置
  gotodest(foodpos);
  cout << "condiment pos : " << foodpos.x << "," << foodpos.y << endl;

  int cookx = cookonly[label][0];
  int cooky = cookonly[label][1];
  int movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
  int movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
  char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向

  if (fooddir == 's')  //如果就在自己脚下
  {
    tPick(TRUE, Tool, Condiment);
    tSleep(50);
  } else {
    move_dir(fooddir, 0);  //调整朝向
    tPick(FALSE, Tool, Condiment);
  }

  tSleep(50);
  if (PlayerInfo.tool == 0) {
    cout << "pick condiment failed!" << endl;
    return 0;  //这样就是没捡起来，返回0失败，直接重新找食材。
  }
  if (isthrow == 1)  //如果要扔到灶台，那扔呗
  {
    put_dest(Point(cookx, cooky), FALSE);  //这里+0.5没用，因为point是整数。
    cout << "put condiment to cook" << endl;
  }
  return 1;
}

int makefood(int food)  //传入目标的编号 whichfood
{
  if (food == 0)
    return 0;  //如果收到的是0，那就啥都做不了，接着找食材吧
  string stop("s");  //精细操作，要求对方停下来
  speakToFriend(stop);

  int cookx = cooklabel[label][0] + nextx(dir_4[cooklabel[label][3]]);
  int cooky = cooklabel[label][1] + nexty(dir_4[cooklabel[label][3]]);
  char c = dir_4[cooklabel[label][3]];  //朝向
  int row = food - 20;                  //行号
  move_allfood_to_left();

  tSleep(50);
  mystorage.updatestorage();
  if (food == SpicedPot)  //把调料和三-四种原料丢进去。//
  {
    cout << "make spicepot!!" << endl;
    int sizeraw = mystorage.getSpiceSize();
    if (sizeraw > 4)
      sizeraw = 4;
    if (sizeraw <= 2)
        return 0;
    cout <<"size raw"<< sizeraw << endl;
    getcondiment(1);  //把调料扔到灶台上
    get_foodgen_dis();
    sort(foodgen.begin(), foodgen.end(), sort_by_3);  //根据距离从小到大排序
    int k = 5;
    for (int j = 1; j <= sizeraw && k <= 8;
         j++)  // j指示已经放入了多少个食材，k指示第几近的食材。
    {
      int destraw = foodgen[k][0];
      k++;
      if (mystorage.getCnt((DishType)destraw) == 0) {
        j--;  //没有食材k，j不增加
        cout << " no destraw :" << destraw << endl;
      } else {
        cout << "spice raw number :" << j << endl;
        dPoint i = *mystorage.getStoragePos((DishType)destraw).begin();
        Point foodpos(i.x, i.y);  //查看食材所在位置
        cout << " try to get :" << destraw << " in pos " << foodpos.x << ","
             << foodpos.y << endl;
        gotodest(foodpos);
        smallmove(foodpos.x, foodpos.y);
        int movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
        int movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
        char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
        cout << "picked :" << destraw << endl;
        if (fooddir == 's')  //如果就在自己脚下
        {
          tPick(TRUE, Dish, destraw);
          tSleep(50);
          if (PlayerInfo.dish == 0)
            return 0;  //这样就是没捡起来，返回0失败，直接重新找食材。
          put_dest(Point(cookx, cooky),TRUE);  //这里+0.5没用，因为point是整数。

        } else {
          move_dir(fooddir, 0);  //调整朝向
          cout << "move : " << fooddir << endl;
          tPick(FALSE, Dish, destraw);
          tSleep(50);
          if (PlayerInfo.dish == 0) {
            cout << "pick raw food failed!  pos:" << PlayerInfo.position.x
                 << " , " << PlayerInfo.position.y << endl;
            return 0;  //这样就是没捡起来，返回0失败，直接重新找食材。
          }
          put_dest(Point(cookx, cooky), TRUE);
        }
      }
    }
    sort(foodgen.begin(), foodgen.end(), sort_by_0);
    cout << "back to cook" << endl;
    gotodest(Point(cooklabel[label][0], cooklabel[label][1]));  //再回到灶台跟前
    move_dir(c, 1);                                             //调整朝向
    list<Obj> l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
    int iscondiment = 0;
    for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
      if (i->tool == Condiment)
        iscondiment = 1;
    }
    if (iscondiment == 0)
      return 0;
  } else {
    list<StoragePerDish> st = mystorage.getRecipe(DishType(food));
    //先检索最大的step,然后把和最大的step相同的step都丢到灶台里
    int maxstep = 0;
    for (auto i : st) {
      cout << "try make :" << food << "    Dish :" << i.type
           << " step:" << i.stepsOfProcessed << endl;
      if (i.stepsOfProcessed > maxstep)
        maxstep = i.stepsOfProcessed;
    }
    if (maxstep == 1)
      return 0;  //已经有成品菜了，不做这个。
    cout << "start to pick food to cook" << endl;
    for (auto i : st) {
      if (i.stepsOfProcessed == maxstep)  //如果步数等于最大步数，就丢到锅里
      {
        int destraw = i.type;
        Point foodpos(i.posList.begin()->x,
                      i.posList.begin()->y);  //查看食材所在位置
        cout << " try to get :" << destraw << " in pos " << foodpos.x << ","
             << foodpos.y << endl;
        gotodest(foodpos);
        smallmove(foodpos.x, foodpos.y);
        int movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
        int movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
        cout << "movex:" << movex << "  movey:" << movey << endl;
        char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
        if (fooddir == 's')                        //如果就在自己脚下
        {
          tPick(TRUE, Dish, destraw);
          tSleep(50);
          if (PlayerInfo.dish == 0)
            return 0;  //这样就是没捡起来，返回0失败，直接重新找食材。
          put_dest(Point(cookx, cooky),
                   TRUE);  //这里+0.5没用，因为point是整数。
          cout << "put finish " << endl;

        } else {
          move_dir(fooddir, 0);  //调整朝向
          cout << "move : " << fooddir << endl;
          tPick(FALSE, Dish, destraw);
          tSleep(50);
          if (PlayerInfo.dish == 0) {
            cout << "pick raw food failed!  pos:" << PlayerInfo.position.x
                 << " , " << PlayerInfo.position.y << endl;
            return 0;  //这样就是没捡起来，返回0失败，直接重新找食材。
          }
          put_dest(Point(cookx, cooky), TRUE);
        }
        cout << "picked :" << destraw << endl;
      }
    }

    //如果要做香锅，调用getcondiment(int isthrow=1)。
    cout << "back to cook" << endl;
    gotodest(Point(cooklabel[label][0], cooklabel[label][1]));  //再回到灶台跟前
    move_dir(c, 0);                                             //调整朝向

    list<Obj> l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
    for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
      auto rawlist = Constant::CookingTable.find(food)->second;
      if (i->objType == Dish) {
        // i的合成表
        if (find(rawlist.begin(), rawlist.end(), Dish) ==
            rawlist.end())  //没有找到
        {
          tPick(FALSE, Dish, i->dish);
          tSleep(50);
          tPut(1, PI, TRUE);
          tSleep(50);
        }
      }
    }
  }
  ///////////////////////////////////////////////////////////////////////

  smallmove(cooklabel[label][0] + 0.5, cooklabel[label][1] + 0.5);
  move_dir(c, 1);  //调整朝向
  list<Obj> l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
  cout <<endl<< "************** map info of cook: ***************" << endl;
  int dishnumber = 0;
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
      cout << "TYPE:" << i->blockType << "   dish:" << i->dish << "   tool:" << i->tool << endl;
      if (i->dish >= 5 && i->dish <= 8) dishnumber++;
  }
  if (food == SpicedPot && dishnumber <= 2)
  {
      cout << " <=2 dishnumber = " << dishnumber << endl;
      return 0;
  }
  cout << "**************  end map info of cook: *************" << endl<<endl;
  tUse(0, 0, 0);    //开始做菜
  tSleep(50);
  smallmove(cooklabel[label][0], cooklabel[label][1]);
  move_dir(c, 1);  //调整朝向
  tUse(0, 0, 0);    //多试一次呗
  cout << "*****start to cook:" << food << endl;
  tSleep(50);
  l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
  for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
    if (i->blockType == 3 && i->dish == DarkDish)
      return 1;  //在做了在做了
    cout << "error :: block type:" << i->blockType << "  dish:" << i->dish
         << endl;
  }
  return 0;  //如果刚刚没有return1，那就是没做上。
}

Point findsave()  //找最近的食物储藏
{
  Point save1(2, 34), save2(27, 47),
      save3(33, 2);  //准备三个储藏点，放到最近的一个藏起来
  Point Pos(PlayerInfo.position.x, PlayerInfo.position.y);
  double dis1 = calcdis(save1, Pos);
  double dis2 = calcdis(save2, Pos);
  double dis3 = calcdis(save3, Pos);
  if (dis1 <= dis2 && dis1 <= dis3)
    return save1;
  if (dis2 <= dis1 && dis2 <= dis3)
    return save2;
  if (dis3 <= dis1 && dis3 <= dis2)
    return save3;
}

int commitTask() {
  //手上拿有成品菜式时调用此方法
  Point StoragePos();                  //储存成品的地点
  //static vector<int[3]> finishedDish;  //这里放已经完成的菜品清单
  int flag = -1;  // 1:去交菜品 2:放置菜品至储存点
  int randpoint = rand() % 2;
  Point destsubmit(26, 24 + randpoint);  //随机去两个点之一交任务
  Point save = findsave();  //准备最近的藏匿点，并且发消息告诉队友
  if (find(task_list.begin(), task_list.end(), PlayerInfo.dish) != task_list.end() ||
      (PlayerInfo.dish >= SpicedPot && PlayerInfo.dish <= SpicedPot6 && find(task_list.begin(), task_list.end(), SpicedPot) != task_list.end()))
  {
    //手上的菜品在任务清单里面
      auto findres = Constant::DishInfo.find(PlayerInfo.dish);
      if (findres->second.Score >= 60)
      {
          cout << "high score dish get condiment  " << findres->second.Score << endl;
          mystorage.updatestorage();
          getcondiment(0);
      }

    gotodest(destsubmit);
    move_dir('a', 1);
    while (PlayerInfo.dish != 0 && find(task_list.begin(), task_list.end(),PlayerInfo.dish) != task_list.end()) 
    {
      cout << " commit task : "<< PlayerInfo.dish;
      if (PlayerInfo.tool == Condiment) {
        tUse(1, 0, 0);  //用调料提交
        tSleep(50);
        cout << "  with condiment" << endl;
      } else {
          cout << "  without condiment" << endl;
        tUse(0, 0, 0);
        tSleep(50);
      }
      move_dir(dir_4[rand()%4+1]);//随机走一步 防止卡住
      gotodest(destsubmit);
      Sleep(50);
      move_dir('a', 1);
    }
    if (PlayerInfo.dish != 0)  //手上还有菜，说明任务超时了，把菜放回原来的地方
    {
      gotodest(save);
      tSleep(50);
      if (PlayerInfo.dish != 0) {
        stringstream sdishinfo;
        sdishinfo << 'd' << " " << PlayerInfo.dish << " " << (int)save.x << " " << (int)save.y;
        cout << "send" << sdishinfo.str() << endl;
        speakToFriend(string(sdishinfo.str()));
        tPut(0, 0, TRUE);
        tSleep(50);
        if (PlayerInfo.tool == Condiment) {
          tPut(0, 0, FALSE);
          tSleep(50);
        }
      }
      return 0;
    }
    return 1;  //返回1说明提交成功
  } 
  else       //不在任务清单里 先存起来
  {
    mystorage.updatestorage();
    if (mystorage.condimentList.empty() == FALSE) {
      getcondiment(0);  //捡起来拿在手上。提交的时候可以用
    }
    // d dish x y
    if (PlayerInfo.dish == 0) return -1;
    int dish = PlayerInfo.dish;
    gotodest(save);
    tSleep(50);
    tPut(0, 0, TRUE);
    tSleep(50);
    // player2 only
    finishedDish.push_back(Dishpos(dish, save.x, save.y));
    ///
    if (PlayerInfo.tool == Condiment) {
      tPut(0, 0, FALSE);
    }
    return 0;  //返回0说明存起来了
  }
}

void findbestdish() {
  bestdish.clear();   // list<array<int,2>>
  get_foodgen_dis();  //更新距离各食物生产点的距离
  for (int i = CookedRice; i <= Hamburger; i++)  // 14~21，计算每个的性价比。
  {
    double sumdis = 0;
    list<DishType> ls = nullptrstorage.getDeficient(DishType(i));
    for (auto dish : ls) {
      sumdis += foodgen[dish][3];  //计算各个食物生产点的距离之和
    }
    auto findres = Constant::DishInfo.find(i);
    double maketime = getCookedTime(DishType(i));
    double r_value;
    if (PlayerInfo.score >= 250) {
        r_value = ((maketime + 5000) * sqrt(sumdis)) / (findres->second.Score);
    }
    else
    {
        r_value = sumdis / (findres->second.Score);
    }
    // sort默认从小到大排序,最好是r_value最小，也就是距离/分数约小越好
    vector<double> thisdish = {double(i), r_value};  //编号，性价比
    bestdish.push_back(thisdish);
  }
  bestdish.sort(sort_by_1);
}

int findbestfoodgen(int timelimited = 0)  //传入cookedtime
{
  bestdish.clear();   // list<array<int,2>>
  get_foodgen_dis();  //更新距离各食物生产点的距离
  cout << "find best dish" << endl;
  if (timelimited != 0 && timelimited < 60000) 
  {
    cout << "return rand() with time limited" << endl;
    vector<int> destinfo;
    sort(foodgen.begin(), foodgen.end(), sort_by_3);  //根据距离从小到大排序
    cout << "sort finish 3" << endl;
    destinfo = foodgen[rand() % 5];  //  前五个里面随机一个吧
    if (timelimited <= 15000)
      destinfo = foodgen[rand() % 3 +1];  //  前三个里面随机一个吧,去掉最近的一个，容易崩。。
    int mydish = destinfo[0];
    sort(foodgen.begin(), foodgen.end(), sort_by_0);
    cout << "sort finish 0" << endl;
    cout << "foodgen my dish :" << mydish << endl;
    return mydish;
  } 
  else 
  {
      list<DishType> tklist = task_list;
      cout << "size of tklist " << task_list.size() << endl;
      for (list<DishType>::iterator l = tklist.begin(); l != tklist.end(); l++)  //用任务列表计算性价比
      {
          int i = *l;
      cout << "calculate value from tklist:" << i << endl;
      if (i != SpicedPot && i>=1 && i<=DarkDish) 
      {
        double sumdis = 0;
        list<DishType> ls = mystorage.getDeficient(DishType(i));
        if (ls.empty() == FALSE) 
        {
          for (auto dish : ls) 
          {
              if (dish > 8)
              {
                  cout << ">8 dish=  "<< dish << endl;
              }
            sumdis += foodgen[dish][3];  //计算各个食物生产点的距离之和
          }
          cout << "sumdis = " << sumdis;
          auto findres = Constant::DishInfo.find(i);
          double maketime = getCookedTime(DishType(i));
          double r_value=9999;
          if (PlayerInfo.score >= 250) {
              r_value = ((maketime + 5000) * sqrt(sumdis)) / (findres->second.Score);
          }
          else
          {
              r_value = sumdis / (findres->second.Score);
          }
          cout<< "  r-value = " << r_value << endl;
          // sort默认从小到大排序,最好是r_value最小，也就是距离/分数约小越好
          vector<double> thisdish = {double(i), r_value};  //编号，性价比
          bestdish.push_back(thisdish);
          cout << "pushback finish" << endl;
        } else {
          cout << "empty " << i << endl;
        }
      }
    }
    if (bestdish.empty()) {
      cout << "all empty" << endl;
      return rand() % 8 + 1;
    }
    bestdish.sort(sort_by_1);
    cout << "bestdish sort finish" << endl;
    int mybest = (*bestdish.begin())[0];  // dish的编号
    cout << "mybest - " << mybest << endl;
    list<DishType> st = mystorage.getDeficient(DishType(mybest));
    if (st.empty() || mybest <= 0 || mybest >= DarkDish) {
      cout << "return rand()" << endl;
      return rand() % 8 + 1;
    }
    int mydish = *st.begin();
    cout << "mydish" << mydish << " size: " << st.size() << endl;
    return mydish;
  }
}

void mainswitch(int nextstate) {
  int angle, dish_make;
  int labelofcook = label;
  cout << "nextstate:" << nextstate << endl;
  stringstream slabinfo;
  string labinfo;
  int randput = rand() % 4 + 1;
  switch (nextstate) {
    case 0:  //自己的灶台正在做，这时候应该继续找食材再回来看。
      cout << "my cook is used" << endl;
      state = 1;
      break;
    case 1:  //这个灶台有别人正在用，找第二近的灶台
      cout << "this cook is being used" << endl;
      //如果返回2说明手里有菜可以提交了
      if (findallcook(labelofcook) == 1)  //如果找到了可以用的灶台
      {
        //把case2的复制一遍
        cout << "now find another cook! " << label << endl;
        labelofcook = label;
        //随机放在周围四格，防止被一锅端
        // tPut(1, angle_4[cooklabel[labelofcook][3]], TRUE);这是原来的定点投放。
        int randput = rand() % 4 + 1;
        if (randput == 2 && label == 4) randput = 4;//这个是因为，4灶台网上扔会放在视野外
        cout << "rand put " << randput << " in cooklabel : " << label << endl;
        tPut(1, angle_4[randput], TRUE);
        if (PlayerInfo.tool == Condiment) {
          tSleep(50);
          tPut(0, 0, FALSE);  //调料扔到脚下
        }
        if (label != labelofcook) {
          label = labelofcook;
          /////
          labinfo.assign(slabinfo.str());
          slabinfo << 'l' << " " << label;
          cout << "send to team:" << slabinfo.str() << endl;
          speakToFriend(labinfo);
          /////
        }
        tSleep(50);
        //先把手里的食材放下来，然后开始做菜
        cout << "generate raw food list" << endl;
        mystorage.updatestorage();
        dish_make = whichfood();
        cout << "which to make" << dish_make << endl;
        if (makefood(dish_make) == 1) {
          cooklabel[labelofcook][4] = 1;  //自己正在做菜呢
          cout << "now make:" << dish_make << endl;
          cooking[0] = dish_make;
          cooking[1] = labelofcook;
          auto findres = Constant::DishInfo.find(dish_make);
          cooking[2] = findres->second.CookTime;  // time
          cooking[3] = getGameTime() - 50;        // start
          cooking[4] = cooking[3] + cooking[2];   // finished time
          cooking[5] = cooking[3] + cooking[2] * 1.25;
          cout << "cooking: ";
          for (int i = 0; i <= 5; i++) {
            cout << " " << cooking[i];
          }
          cout << endl;
          state = 1;  //进入【有灶台正在烹饪】的状态
        } else {
          state = 0;
        }
        tSleep(50);
        if (PlayerInfo.dish != 0) {
          tPut(0, 0, TRUE);
        }
      }

      break;
    case 2:  //用这个灶台做菜就好，不用慌
      angle = angle_4[cooklabel[labelofcook][3]];  //从编号获得角度，扔到灶台里
      cout << "cooklabel=" << labelofcook << ",angle=" << angle << endl;
      cout << "angle=" << angle << endl;
      //随机放在周围四格，防止被一锅端
      // tPut(1, angle_4[cooklabel[labelofcook][3]], TRUE);这是原来的定点投放。
      cout << "random put:" << randput;
      tPut(1, angle_4[randput], TRUE);
      if (PlayerInfo.tool == Condiment) {
        tSleep(50);
        tPut(0, 0, FALSE);  //调料扔到脚下
      }
      if (label != labelofcook) {
        label = labelofcook;
        /////
        labinfo.assign(slabinfo.str());
        slabinfo << 'l' << " " << label;
        cout << "send to team:" << slabinfo.str() << endl;
        speakToFriend(labinfo);
        /////
      }
      tSleep(50);
      //先把手里的食材放下来，然后开始做菜
      cout << "generate raw food list" << endl;
      mystorage.updatestorage();

      dish_make = whichfood();
      cout << "which to make" << dish_make << endl;
      if (makefood(dish_make) == 1) {
        cooklabel[labelofcook][4] = 1;  //自己正在做菜呢
        cout << "now make:" << dish_make << endl;
        cooking[0] = dish_make;
        cooking[1] = labelofcook;
        auto findres = Constant::DishInfo.find(dish_make);
        cooking[2] = findres->second.CookTime;  // time
        cooking[3] = getGameTime() - 50;        // start
        cooking[4] = cooking[3] + cooking[2];   // finished time
        cooking[5] = cooking[3] + cooking[2] * 1.25;
        cooklabel[labelofcook][4] = 1;
        cout << "cooking: ";
        for (int i = 0; i <= 5; i++) {
          cout << " " << cooking[i];
        }
        cout << endl;
        state = 1;  //进入【有灶台正在烹饪】的状态
      } else {
        state = 0;
      }
      tSleep(50);
      if (PlayerInfo.dish != 0) {
        tPut(0, 0, TRUE);
      }
      break;
    case 3:  //准备提交菜肴，此时菜肴已经在手里了。
      cout << "food in hand to be submit =" << PlayerInfo.dish << endl;
      state = 2;
      break;
  }
}
//player 2



int getspeak()
{
    char c;
    static string olds;
    int l, dish, x, y;
    string re = PlayerInfo.recieveText;
    //cout << re << endl;
    if (olds.size() >= 1 && re == olds)
    {
        //cout << "no change" << endl;
        return 0;//没有改变
    }
    olds = re;
    stringstream restream;
    restream << re.c_str();
    restream >> c;
    switch (c)
    {
    case 'l':
        restream >> l;
        cout << "label change to:" << l << endl;
        label = l;
        break;
        return 1;//label changed
    case 'd':
        restream >> dish >> x >> y;
        cout << "dish to submit:" << dish << "in :" << x << "," << y << endl;
        finishedDish.push_back(Dishpos(dish, x, y));
        return 2;//dish added
        break;
    case 's':
        cout << "stop 2 s!" << endl;
        tSleep(2000);
        return 3;//stop
        break;
    }

}
int gotosubmit(Dishpos i);
int can_submit()//判断任务列表里有没有可以去提交的,return 2没捡到菜，return1成功提交，return-1放回原处,return 0没有可以提交的任务。
{
    list<DishType> tk = task_list;
    for (auto i = finishedDish.begin(); i != finishedDish.end(); i++) {//根据成品菜列表，看看任务列表里有没有相同的
        if (find(tk.begin(), tk.end(), i->Dish) != tk.end())//如果有，就跑过去检菜交菜
        {
            Dishpos save(i->Dish, i->dishx, i->dishy);
            finishedDish.erase(i);//删掉这个信息
            int ret = gotosubmit(save);
            return ret;
        }
    }
    //如果刚刚没有return,那就是列表里没有了
    return 0;
}

int gotosubmit(Dishpos i)//return1成功提交，return - 1放回原处,return 2没捡到菜，return-1放回原处
{
    int randpoint = rand() % 2;
    Point destsubmit(26, 24 + randpoint);//随机去两个点之一交任务
    Point psave(i.dishx, i.dishy);//存放点
    gotodest(psave);
    tSleep(50);
    tPick(TRUE, Dish, i.Dish);
    tSleep(50);
    cout << "try to pick dish" << i.Dish <<"  get" << PlayerInfo.dish<< endl;
    if (PlayerInfo.dish == 0)
    {
        tPick(TRUE, Dish, i.Dish);
        tSleep(50);
    }
    if (PlayerInfo.dish == 0)
    {
        cout << "pick failed" << endl;
        return 2;//捡起来失败了，继续游荡吧
    }
    if (PlayerInfo.tool != Condiment) {//如果手上没有调料，捡起来试试
        cout << "try to get condiment" << endl;
        mystorage.updatestorage();
        if (mystorage.condimentList.empty() == FALSE) 
        {
            cout << "pick condiment" << endl;
            getcondiment(0);
            tPick(TRUE, Tool, Condiment);  //捡起来拿在手上。提交的时候可以用
        }
    }

    //捡起来了，去提交任务吧
    gotodest(destsubmit);
    move_dir('a', 1);
    cout << "try to submit!" << endl;
    while (PlayerInfo.dish != 0 && find(task_list.begin(), task_list.end(), PlayerInfo.dish) != task_list.end())
    {
        if (PlayerInfo.tool == Condiment)
        {
            tUse(1, 0, 0);//用调料提交
            cout << "  with condiment" << endl;
            tSleep(50);
        }
        else
        {
            tUse(0, 0, 0);
            cout << "  without condiment" << endl;
            tSleep(50);
        }
        move_dir(dir_4[rand() % 4 + 1]);//随机走一步 防止卡住
        gotodest(destsubmit);
        Sleep(50);
        move_dir('a', 1);
    }
    cout << "score now" << PlayerInfo.score << endl;

    if (PlayerInfo.dish != 0)//手上还有菜，说明任务超时了，把菜放回原来的地方
    {
        cout << "submit failed!" << endl;
        gotodest(psave);
        tPut(0, 0, TRUE);
        finishedDish.push_back(Dishpos(PlayerInfo.dish, psave.x, psave.y));
        return -1;
    }

}

int tricklabel = 0;//记录哪一家可以捣乱（食材多）,在trick()中修改。

int getfoodwhentrick()//从高级产品往低级产品检索，如果捡到了就返回1，否则继续
{
    mystorage.updatestorage(0);
    for (int i = SpicedPot; i >= 1; i--)
    {
        cout << "getfoodwhentrick " << i << endl;
        if (mystorage.getCnt(DishType(i)) != 0)
        {
            dPoint dest(*mystorage.getStoragePos(DishType(i)).begin());
            int destraw = i;
            Point foodpos(dest.x, dest.y);  //查看食材所在位置
            cout << " trick  --  get :" << destraw << " in pos " << foodpos.x << "," << foodpos.y << endl;
            gotodest(foodpos);
            smallmove(dest.x, dest.y);
            int movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
            int movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
            cout << "movex:" << movex << "  movey:" << movey << endl;
            char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
            if (fooddir == 's')                        //如果就在自己脚下
            {
                tPick(TRUE, Dish, destraw);
                tSleep(50);
                if (PlayerInfo.dish != 0)
                {
                    cout << "picked :" << destraw << endl;
                    return 1;  //捡到菜了
                }
            }
            else {
                move_dir(fooddir, 0);  //调整朝向
                cout << "move : " << fooddir << endl;
                tPick(FALSE, Dish, destraw);
                tSleep(50);
                if (PlayerInfo.dish != 0)
                {
                    cout << "picked :" << destraw << endl;
                    return 1;  //捡到菜了
                }
            }
            
        }
    }
    return 0;
}


int trick(int _label,int res)//_label是被捣乱的编号,把这个灶台周围所有食材都丢进去一锅端了。已经在throw darkdish里检查过成品了,return1 时一窝端
//res=0说明灶台正在使用 ，=1灶台没有人使用。
//返回2表示捡到成品食材，返回1表示送到主灶台，否则无事发生
{
    int cookx = cooklabel[_label][0] + nextx(dir_4[cooklabel[_label][3]]);
    int cooky = cooklabel[_label][1] + nexty(dir_4[cooklabel[_label][3]]);
    cout << "trick label :" << _label << endl;
    //首先检查有无成品食材
    getfoodwhentrick();//调用此函数捡起视野内编号最大的dish
    cout << "dish in player 2 :" << PlayerInfo.dish << endl;
    if (PlayerInfo.dish == 0)
    {
        cout << " no dish   set tricklabel=0" << endl;
        tricklabel =0;
        return 0;//无事发生
    }
    else
    {
        tricklabel = _label;//捡到菜了 那么下次再来这里看看。
    }

    if (res == 1)//res=1说明灶台没有在做菜，可以去点火搞事情
    {
        Point cookpos(cookonly[_label][0], cookonly[_label][1]);
        gotodest(cookpos);
        astar.mazeUpdate();
        Point posnear = getnear(cookpos);
        smallmove(posnear.x + 0.5, posnear.y + 0.5);
        int movex = sgn((int)cookpos.x - (int)PlayerInfo.position.x);
        int movey = sgn((int)cookpos.y - (int)PlayerInfo.position.y);
        char cookdir = dir[movey + 1][movex + 1];  //计算最后的朝向
        move_dir(cookdir, 0); //调整朝向
        ////
        if(PlayerInfo.dish<Flour || label==0) 
            put_dest(Point(cookx, cooky), TRUE);//丢进去
        ///
        tUse(0, 0, 0);    //做菜

        list<Obj> l = MapInfo::get_mapcell(cookx, cooky);  //看看灶台里有啥
        for (list<Obj>::iterator i = l.begin(); i != l.end(); i++) {
            cout << "TYPE:" << i->blockType << "   dish:" << i->dish << "   tool:" << i->tool << endl;
        }

        if (PlayerInfo.dish == 0)
        {
            getfoodwhentrick();
        }
    }

    if (PlayerInfo.dish >= CookedRice && PlayerInfo.dish <= SpicedPot)
    {
        tricklabel = _label;
        return 2;//准备提交
    }

    if (PlayerInfo.dish > 0 && PlayerInfo.dish <= Ketchup)
    {
        tricklabel = _label;
        return 1;//送回主label灶台
    }

    return 0;
}

int first = 1;//只在第一轮的时候跑到食物生成点般一个食物。


//0:有人在做菜，看看周围有没有可以捡走的菜；1：空灶台；0，1调用trick就好；2：准备提交成品菜
int throw_darkdish2(int _label)
{
    //if (cooklabel[_label][4] == 1)return 0;//这个灶台是自己正在做的，不要扔黑暗料理
    //有时候自己做了菜，但是没有拿到手，这时候会把label变成0，但是这还是我的菜啊
    int x = cookonly[_label][0];
    int y = cookonly[_label][1];
    Point cookpos(cookonly[_label][0], cookonly[_label][1]);
    cout << "go to foodpos in throw dark dish 2 label =" <<_label << endl;
    gotodest(cookpos);
    astar.mazeUpdate();
    Point posnear = getnear(cookpos);
    smallmove(posnear.x + 0.5, posnear.y + 0.5);
    int movex = sgn((int)cookpos.x - (int)PlayerInfo.position.x);
    int movey = sgn((int)cookpos.y - (int)PlayerInfo.position.y);
    char cookdir = dir[movey + 1][movex + 1];  //计算最后的朝向
    move_dir(cookdir, 0); //调整朝向

    list<Obj> l = MapInfo::get_mapcell(x, y);
    for (list<Obj>::iterator i = l.begin(); i != l.end(); i++)
    {
        if (i->blockType == 3 && i->dish != 0 && i->dish < OverCookedDish)//如果发现灶台里面有菜,而且不是overcook或者黑暗料理
        {
            cout << "DISH IN BLOCK  " << i->dish << "  _label  " << _label << "   my cook label " << label <<"  "<< cooklabel[label][4] << endl;
            if (i->dish == CookingDish)
            {
                return 0;//没捡到，这个灶台有人在用，我看看周围有没有菜可以拿走
            }
            int dishtemp = PlayerInfo.dish;
            tPut(0, PI, TRUE);//先把手里的东西放脚下
            tSleep(50);
            tPick(FALSE, Block, 0);//尝试拾取
            tSleep(50);

            if (PlayerInfo.dish == 0) 
            {
                tPick(TRUE, Dish, dishtemp);
                return 0;//没捡到，这个灶台有人在用，我看看周围有没有菜可以拿走
            }

            if (PlayerInfo.dish >= OverCookedDish)//如果拿到了黑暗料理，扔出去
            {
                cout << "throw dark dish  " << PlayerInfo.dish << endl;
                put_dest(Point(cookpos.x, cookpos.y), TRUE);
                tSleep(50);
                //放回灶台里 为难一下别人嘛
                return 1;//用这个灶台捣乱
            }
            else //不是黑暗料理！捡到宝了！
            {
                return 2;//准备提交食物，耶
            }
        }

    }
    //灶台里啥都没有，直接做菜，把周围菜扔进去点火
    return 1;
}

int myround = 0;
void play() {
    try
    {
        //state=3 没任务 游荡，一旦发现有可以提交的，就去提交，变成state=4，随时接受信息，搜索tk
    //state=4 

    //通信规范 字母 数值
    //'l' 数字            label的改变
    //'d' dish 坐标 坐标   存放dish

    //那最开始延时25ms是不是就能错开了……
    //只在第一次的时候跑到最远的食物生成点般一个食物，等label更新之后送过去。

        if (first == 1) {
            first = 0;
            tSleep(125);
            Point foodpos = findfarfood();
            cout << "foodpos : " << foodpos.x << "," << foodpos.y << endl;
            gotodest(foodpos);
            int movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
            int movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
            char fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
            move_dir(fooddir, 1);                      //调整朝向
            while (1)//守株待兔
            {
                pick_dish_in_block(foodpos);
                if (PlayerInfo.dish != 0)
                {
                    cout << "finish" << endl;
                    break;//真的捡到了吗？捡到了就break，否则继续守株待兔
                }
                cout << "pick failed" << endl;
                getspeak();//等的时候随时修改
            }
            while (label == 0)
            {
                getspeak();
                cout << "wait for label:" << label << endl;
                tSleep(50);
                //wait for label
            }

            Point cookpos(cookonly[label][0], cookonly[label][1]);
            gotodest(cookpos);
            ////           
            int randput = rand() % 4 + 1;
            //if (randput == 2 && label == 4) randput = 4;//这个是因为，4灶台网上扔会放在视野外
            //随机放在周围四格，防止被一锅端
            //tPut(1, angle_4[cooklabel[labelofcook][3]], TRUE);这是原来的定点投放。
            cout << "random put:" << randput;
            tPut(1, angle_4[randput], TRUE);
            tSleep(50);
            if (PlayerInfo.tool == Condiment)
            {
                tPut(0, 0, FALSE);//调料扔到脚下
            }
            state = 3;//state好像没什么用 先保留吧
            tSleep(50);
            if (PlayerInfo.dish != 0)
            {
                tPut(0, 0, TRUE);
            }
        }


        int randl = rand() % 3;
        int randlabel;//随机去某个灶台捣乱。1234 +-1,+2
        if (label != 0)
        {
            randlabel = label;
            switch (randl)
            {
            case 0:
                randlabel--;
                break;
            case 1:
                randlabel++;
                break;
            case 2:
                randlabel++; randlabel++;
                break;
            default:randlabel++;
                break;
            }
            if (randlabel >= 5) randlabel = randlabel - 4;
            if (randlabel == 0) randlabel = 4;
        }
        else
        {
            randlabel = rand() % 4 + 1;
        }
        int c = can_submit();// return1成功提交，return - 1放回原处,return 2没捡到菜， return 0没有可以提交的任务。
        // return 0是常态吧 如果0那就随机给50%的概率取捣乱。
        if (c == 0 || 1)
        {
            c = 2 * (rand() % 2 == 0);
        }
        if (tricklabel != 0 && tricklabel<=4)
        {
            cout << "tricklabel != 0 !! " <<tricklabel<<endl;
            randlabel = tricklabel;
            c = 2;//如果记录了可以到捣乱的灶台，就捣乱。
        }
        int randput = rand() % 4 + 1;
        Point foodpos = findfarfood(1);// rand()%3+6 
        int movex, movey;
        char fooddir;
        switch (c)
        {
        case 1:
        case 0:
            cout << " go to get food with c = " << c <<" tricklabel = "<<tricklabel<< endl;
            gotodest(foodpos);
            movex = sgn((int)foodpos.x - (int)PlayerInfo.position.x);
            movey = sgn((int)foodpos.y - (int)PlayerInfo.position.y);
            fooddir = dir[movey + 1][movex + 1];  //计算最后的朝向
            move_dir(fooddir, 1);                      //调整朝向
            while (1)//守株待兔
            {
                pick_dish_in_block(foodpos);
                if (PlayerInfo.dish != 0)
                {
                    cout << "finish" << endl;
                    break;//真的捡到了吗？捡到了就break，否则继续守株待兔
                }
                cout << "wait";
                getspeak();//等的时候随时修改
            }
            cout << "goto dest cook label = " << label << " with dish " << PlayerInfo.dish << endl;
            gotodest(Point(cookonly[label][0], cookonly[label][1])); //{x,y,编号，朝向，lab

            //随机放在周围四格，防止被一锅端
            //tPut(1, angle_4[cooklabel[labelofcook][3]], TRUE);这是原来的定点投放。
            //if (randput == 2 && label == 4) randput = 4;//这个是因为，4灶台网上扔会放在视野外
            cout << "rand put " << randput << " in cooklabel : " << label << endl;
            tPut(1, angle_4[randput], TRUE);
            tSleep(50);
            if (PlayerInfo.tool == Condiment)
            {
                tSleep(50);
                tPut(0, 0, FALSE);//调料扔到脚下
                tSleep(50);
            }
            state = 3;//state好像没什么用 先保留吧
            if (PlayerInfo.dish != 0)
            {
                tPut(0, 0, TRUE);
                tSleep(50);
            }

            break;
        case 2:
        case -1:
            if (randlabel != label)
            {
                cout << "rand label = " << randlabel << endl;
                Point cookpos(cookonly[randlabel][0], cookonly[randlabel][1]);
                gotodest(cookpos);
                astar.mazeUpdate();
                Point posnear = getnear(cookpos);
                smallmove(posnear.x + 0.5, posnear.y + 0.5);
                int movex = sgn((int)cookpos.x - (int)PlayerInfo.position.x);
                int movey = sgn((int)cookpos.y - (int)PlayerInfo.position.y);
                char cookdir = dir[movey + 1][movex + 1];  //计算最后的朝向
                move_dir(cookdir, 0); //调整朝向
                cout << "throw dark dish 2" << endl;
                int res = throw_darkdish2(randlabel);
                if (res == 2)//手上有成品菜
                {
                    commitTask();
                }
                else//调用trick，res=0说明灶台正在使用 ，=1一锅端。
                {
                    switch (trick(randlabel, res))//返回2表示捡到成品食材，返回1表示送到主灶台，否则无事发生
                    {
                    case 2:
                        commitTask();
                        break;
                    case 1:
                        gotodest(Point(cookonly[label][0], cookonly[label][1])); //{x,y,编号，朝向，lab
                        //if (randput == 2 && label == 4) randput = 4;//这个是因为，4灶台网上扔会放在视野外
                        cout << "rand put " << randput << " in cooklabel : " << label << endl;
                        tPut(1, angle_4[randput], TRUE);
                    }
                   
                }
            }
            break;
        }
        getspeak();
        tSleep(200);      
    }
    catch (exception & e) {
        cerr << "Exception happens" << e.what() << endl;
        return;
    }
}
