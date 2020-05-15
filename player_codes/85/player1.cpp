// Player1
#include "API.h"
#include "Constant.h"
#include "player.h"
#include <iostream>
#include "OS_related.h"
#include <cmath>
#include <queue>
#include <sstream>
#include <fstream>
using namespace THUAI3;
Protobuf::Talent initTalent = Protobuf::Talent::Runner;

// BlockType::Table = 1
vector<XYPosition> table = { XYPosition(7.5,23.5),XYPosition(8.5,23.5),XYPosition(9.5,23.5),XYPosition(9.5,24.5),
							XYPosition(9.5,32.5),
							XYPosition(15.5,24.5),XYPosition(15.5,25.5),
							XYPosition(16.5,38.5),XYPosition(17.5,38.5),XYPosition(18.5,38.5),
							XYPosition(16.5,11.5),XYPosition(17.5,11.5),XYPosition(17.5,12.5),XYPosition(16.5,12.5),
							XYPosition(24.5,39.5),
							XYPosition(26.5,38.5),
							XYPosition(24.5,35.5),XYPosition(24.5,34.5),XYPosition(24.5,33.5),XYPosition(24.5,32.5),XYPosition(24.5,31.5),XYPosition(25.5,31.5),XYPosition(26.5,31.5),
							XYPosition(27.5,19.5),XYPosition(27.5,18.5),
							XYPosition(31.5,27.5),XYPosition(32.5,27.5),XYPosition(32.5,26.5),XYPosition(31.5,26.5),
							XYPosition(32.5,19.5),XYPosition(32.5,18.5),XYPosition(32.5,17.5),XYPosition(34.5,18.5),
							XYPosition(40.5,17.5),
							XYPosition(41.5,29.5),XYPosition(42.5,29.5),XYPosition(42.5,28.5),
							XYPosition(45.5,35.5) };
// BlockType::FoodPoint = 2
// vector<XYPosition> foodPoint = { XYPosition(7.5,41.5),XYPosition(25.5,5.5),XYPosition(42.5,40.5) };
map<DishType, XYPosition> foodPoint = { pair<DishType,XYPosition>(DishType::Wheat,XYPosition(4.5,24.5)),
pair<DishType,XYPosition>(DishType::Rice,XYPosition(5.5,5.5)),pair<DishType,XYPosition>(DishType::Tomato,XYPosition(7.5,41.5)),
pair<DishType,XYPosition>(DishType::Egg,XYPosition(25.5,5.5)),pair<DishType,XYPosition>(DishType::Beef,XYPosition(31.5,41.5)),
pair<DishType,XYPosition>(DishType::Pork,XYPosition(42.5,40.5)),pair<DishType,XYPosition>(DishType::Potato,XYPosition(43.5,6.5)),
pair<DishType,XYPosition>(DishType::Lettuce,XYPosition(43.5,25.5)) };
// BlockType::Cooker = 3
vector<XYPosition> cooker = { XYPosition(8.5,24.5),XYPosition(25.5,38.5),XYPosition(33.5,18.5),XYPosition(41.5,28.5) };
// BlockType::RubbishBin = 4
vector<XYPosition> rubbishBin = { XYPosition(12.5,19.5),XYPosition(15.5,32.5),XYPosition(25.5,13.5),XYPosition(36.5,35.5),XYPosition(38.5,24.5) };
// BlockType::TaskPoint = 5
vector<XYPosition> taskPoint = { XYPosition(24.5,24.5),XYPosition(24.5,25.5),XYPosition(25.5,25.5),XYPosition(25.5,24.5) };
// RandomDirection
queue<Direction> RandomDirection;

// Process1
enum Process1 : int {
	Start = 0,
	DecideWhereToCook,
	GoToCooker,
	DecideWhichToCook,
	DecideWhichToPut,
	GoToPutDishToCooker,
	PutDishToCooker,
	CheckCooker,
	Cooking,
	WaitCookFinish,
	DecideWhereToThrow,
	ThrowToDustbin,
	GoingToPickDishToHand,
	DecideWhereToHandTask,
	GoingToTaskPoint,
	HandingTask,
	ReturnToWait,
	PickAndUseTool,
	Finish,
	Process1Size
};

// global variables
const double PI = 3.1415926;
XYPosition Last_Choose_FoodPoint = XYPosition(-1, -1);
XYPosition Choose_FoodPoint = XYPosition(0, 0);
XYPosition Last_Choose_Cooker = XYPosition(-1, -1);
XYPosition Choose_Dish = XYPosition(0, 0);
XYPosition Choose_Cooker = XYPosition(0, 0);
XYPosition Choose_Tool = XYPosition(0, 0);
XYPosition Choose_TaskPoint = XYPosition(0, 0);
XYPosition Choose_Dustbin = XYPosition(0, 0);
XYPosition ThrowPosition = XYPosition(-1, -1);
XYPosition LastLastPosition = XYPosition(-1, -1);
XYPosition LastPosition = XYPosition(-1, -1);
XYPosition lastDestination = XYPosition(-1, -1);
XYPosition Destination = XYPosition(0, 0);
XYPosition HandingDishPosition = XYPosition(-1, -1);
DishType DoingDish = DishType::DishEmpty;
DishType FindDish = DishType::DishEmpty;
DishType HandingDish = DishType::DishEmpty;
vector<DishType> needThings;
vector<DishType> needThings_back;
vector<DishType> temp;
vector<XYPosition>needThingsPositions;
bool HaveBeenTo[50][50] = { false };
bool DishLevel[50];
bool DishTime[50];
bool stop = false;
bool first_do = true;
bool first = true;
int error = 0;
int speak = 0;
int number = 0;
int go_time = 0;
int stop_time = 0;
int wait_time = 0;
int random_time = 0;
double g_factor = 0;
const double barrier = 0.9;
const double volumn = 0.5;
const double random_go_time = 1.5;
const int  max_go_time = 20;
const int random_probability = 500;
const int begin_time = 1000;
const int sleep_time = 25;
const int max_speak = 10;
const int max_error = 3;
const int tool_distance = 99999999 /* 8 */;
const int max_stop_time = 1;
const int max_random_time = 5;
const int speak_time = 100;
const int max_wait_time = 20000;
Process1 last_flag = Process1::Start;
Process1 flag = Process1::Start;
// ofstream of("D:\\Tsinghua\\����\\��������ս\\THUAI3.0\\player1.txt");
	
// functions
bool findPeople(vector<XYPosition> & positions); // ObjType::People = 0
bool findEnermy(vector<XYPosition> & positions); // ObjType::People = 0
bool findBlock(vector<XYPosition> & positions, BlockType block); // ObjType::Block = 1
bool findBlock(vector<XYPosition> & positions, vector<Obj>& objects); // ObjType::Block = 1
bool findBlock(vector<XYPosition> & positions);// ObjType::Block = 1
bool findDish(vector<XYPosition> & positions, DishType dish); // ObjType::Dish = 2
bool findDish(vector<XYPosition> & positions, vector<Obj>& objects); // ObjType::Dish = 2
bool findDish(vector<XYPosition> & positions); // ObjType::Dish = 2
bool findTool(vector<XYPosition> & positions, ToolType tool); // ObjType::Tool = 3
bool findTool(vector<XYPosition> & positions, vector<Obj>& objects); // ObjType::Tool = 3
bool findTool(vector<XYPosition> & positions); // ObjType::Tool = 3
bool findTrigger(vector<XYPosition> & positions, TriggerType trigger); // ObjType::Trigger = 4
bool findTrigger(vector<XYPosition> & positions, vector<Obj>& objects); // ObjType::Trigger = 4
bool findTrigger(vector<XYPosition> & positions); // ObjType::Trigger = 4
bool isPeople(XYPosition position); // ObjType::People = 0
bool isBlock(XYPosition position); // ObjType::Block = 1
bool isDish(XYPosition position); // ObjType::Dish = 2
bool isTool(XYPosition position); // ObjType::Tool = 3
bool isTrigger(XYPosition position); // ObjType::Trigger = 4
bool isBlock(XYPosition position, BlockType block); // ObjType::Block = 1
bool isDish(XYPosition position, DishType dish); // ObjType::Dish = 2
bool isTool(XYPosition position, ToolType tool); // ObjType::Tool = 3
bool isTrigger(XYPosition position, TriggerType trigger); // ObjType::Trigger = 4
inline double euclideanDistance(XYPosition position1, XYPosition position2);
inline double euclideanDistance(double x, double y, double x1, double x2);
inline int mahattanDistance(XYPosition position1, XYPosition position2);
inline int mahattanDistance(int x, int y, int x1, int y1);
void GoTo(XYPosition position);
void GoToDestination(XYPosition position);
void adjustFacingDirection(XYPosition position);
XYPosition findMin(vector<XYPosition> positions, XYPosition last);
XYPosition findMin(vector<XYPosition> positions);
void clear();
bool put(XYPosition destination, bool isThrowDish);
bool useTool(ToolType tool);
bool isCooking(XYPosition position);
bool canDo();
void initializeRandomDirection();
XYPosition getThrowDishPosition(XYPosition cooker);
double g(XYPosition position);
bool isIn(vector<DishType>& v, DishType d);

void play()
{
	
	// istringstream in(PlayerInfo.recieveText);
	// ostringstream out;
	vector<XYPosition> found_things_positions;
	vector<Obj> found_things;
	list<Obj> mapcell;
	int xi, yi;
	bool goout = false;
	bool cannotcook = false;

	if (flag != Process1::GoingToPickDishToHand && flag != Process1::ReturnToWait &&
		flag != Process1::PickAndUseTool &&
		flag != Process1::DecideWhereToHandTask && flag != Process1::HandingTask &&
		flag != Process1::GoingToTaskPoint && findDish(found_things_positions, found_things)) {
		for (list<DishType>::iterator iter = task_list.begin(); iter != task_list.end(); ++iter) {
			for (vector<Obj>::iterator iter2 = found_things.begin(); iter2 != found_things.end(); ++iter2) {
				if (*iter == iter2->dish) {
					// of << "Before handing, My flag is " << flag << endl;
					if (flag == Process1::WaitCookFinish) {
						// of << "I should return to wait" << endl;
						last_flag = Process1::ReturnToWait;
					}
					else {
						last_flag = Process1::GoToCooker;
					}
					if (!stop) {
						stop = true;
						// of << "I want to hand task! So I tell my teammate must stop!" << endl;
						speak = 0;
						while (PlayerInfo.recieveText != "Stop") {
							speakToFriend("Stop");
							Sleep(speak_time);
							// wait();
							speak++;
							// of << "Speak " << speak << endl;
							if (speak >= max_speak) {
								// stop = false;
								speak = 0;
								break;
							}
						}
					}
					// of << "Now I want to hand dish " << *iter << endl;
					HandingDish = *iter;
					HandingDishPosition = iter2->position;
					Destination = HandingDishPosition;
					goout = true;
					flag = Process1::GoingToPickDishToHand;
					clear();
					break;
				}
			}
			if (goout) { break; }
		}
	}

	vector<XYPosition> foundEnermyPositions;
	if (findEnermy(foundEnermyPositions)
		&& (PlayerInfo.tool == ToolType::ThrowHammer || PlayerInfo.tool == ToolType::Bow)) {
		// of << "I find enermy! So I attack him!" << endl;
		use(PlayerInfo.tool, euclideanDistance(PlayerInfo.position, foundEnermyPositions.back()),
			atan2(foundEnermyPositions.back().y - PlayerInfo.position.y, foundEnermyPositions.back().x - PlayerInfo.position.x));
		Sleep(1000);
	}

	if (PlayerInfo.tool != ToolType::ToolEmpty) {
		if (useTool(PlayerInfo.tool)) {
			// of << "Use tool: " << PlayerInfo.tool << endl;
		}
	}

	vector<XYPosition> foundToolPositions;
	if (flag != Process1::GoingToPickDishToHand && /*flag != Process1::WaitCookFinish &&*/
		flag != Process1::PickAndUseTool && flag != Process1::DecideWhereToHandTask &&
		flag != Process1::HandingTask && flag != Process1::GoingToTaskPoint && flag != Process1::ReturnToWait &&
		PlayerInfo.tool == ToolType::ToolEmpty && findTool(foundToolPositions)) {
		Choose_Tool = findMin(foundToolPositions);
		if (mahattanDistance(PlayerInfo.position, Choose_Tool) <= tool_distance) {
			// of << "Suddenly I find a tool, so I pick and use it" << endl;
			if (flag == Process1::WaitCookFinish) {
				last_flag = Process1::ReturnToWait;
			}
			else {
				last_flag = Process1::GoToCooker;
			}
			Destination = Choose_Tool;
			if (!stop) {
				stop = true;
				// of << "I want to pick tools! So I tell my teammate to stop!" << endl;
				speak = 0;
				while (PlayerInfo.recieveText != "Stop") {
					speakToFriend("Stop");
					Sleep(speak_time);
					// wait();
					speak++;
					// of << "Speak " << speak << endl;
					if (speak >= max_speak) {
						// stop = false;
						speak = 0;
						break;
					}
				}
			}
			flag = Process1::PickAndUseTool;
			clear();
		}
	}


	switch (flag) {
	case Process1::Start:
		Sleep(begin_time);
		srand(time(NULL));
		// of << "I'm player1! My task are cooking and handing!" << endl;
		// of << "Let's begin!" << endl;
		initializeRandomDirection();
		flag = Process1::DecideWhereToCook;
		break;
	case Process1::DecideWhereToCook:
		// of << "I'm deciding where to cook! emmmmmm......" << endl;
		Last_Choose_Cooker = Choose_Cooker;
		if (PlayerInfo.position.x < 25 && PlayerInfo.position.y > 25) {
			Choose_Cooker = XYPosition(25.5, 38.5);
		}
		else {
			Choose_Cooker = findMin(cooker, Last_Choose_Cooker);
		}
		Destination = Choose_Cooker;
		// of << "Cooker(" << Choose_Cooker.x << ", " << Choose_Cooker.y << ")!" << endl;
		ThrowPosition = getThrowDishPosition(Choose_Cooker);
		/*
		out << Choose_Cooker.x << " " << Choose_Cooker.y;
		// of << "I tell my teammate: " << out.str() << endl;
		while (PlayerInfo.recieveText != "Get") {
			speakToFriend(out.str());
			Sleep(speak_time);
			// wait();
		}
		*/
		flag = Process1::GoToCooker;
		stop = true;
		clear();
		break;
	case Process1::GoToCooker:
		if (!stop) {
			stop = true;
			// of << "I want to go to cooker! So I tell my teammate to stop!" << endl;
			speak = 0;
			while (PlayerInfo.recieveText != "Stop") {
				speakToFriend("Stop");
				Sleep(speak_time);
				// wait();
				speak++;
				// of << "Speak " << speak << endl;
				if (speak >= max_speak) {
					// stop = false;
					speak = 0;
					break;
				}
			}
		}
		if (euclideanDistance(PlayerInfo.position, Choose_Cooker) <= 3) {
			// of << "Get to the cooker(" << Choose_Cooker.x << ", " << Choose_Cooker.y << ")" << endl;
			flag = Process1::DecideWhichToCook;
		}
		else { GoTo(Choose_Cooker); }
		break;
	case Process1::DecideWhichToCook:
		if (canDo()) {
			if (stop && first_do) { Sleep(1000); }
			if (!stop) {
				stop = true;
				// of << "I want to cook! So I tell my teammate to stop!" << endl;
				speak = 0;
				while (PlayerInfo.recieveText != "Stop") {
					speakToFriend("Stop");
					Sleep(speak_time);
					// wait();
					speak++;
					// of << "Speak " << speak << endl;
					if (speak >= max_speak) {
						// stop = false;
						speak = 0;
						break;
					}
				}
			}
			// of << "I can cook dish " << DoingDish << "!" << endl;
			// of << "I need dish ";
			for (vector<DishType>::iterator iter = needThings.begin(); iter != needThings.end(); ++iter) {
				// of << *iter << " ";
			}
			// of << endl;
			first_do = false;
			flag = Process1::DecideWhichToPut;
			needThings_back = needThings;
		}
		else {
			if (stop) {
				stop = false;
				// of << "I can't cook anything! So I tell my teammate to pick dished!" << endl;
				speak = 0;
				while (PlayerInfo.recieveText != "Go") {
					speakToFriend("Go");
					Sleep(speak_time);
					// wait();
					speak++;
					// of << "Speak " << speak << endl;
					if (speak >= max_speak) {
						// stop = true;
						speak = 0;
						break;
					}
				}
			}
			first_do = true;
		}
		// Sleep(sleep_time);
		break;
	case Process1::DecideWhichToPut:
		if (!needThings.empty()) {
			if (findDish(found_things_positions, needThings.back())) {
				Choose_Dish = found_things_positions.back();
				FindDish = needThings.back();
				Destination = Choose_Dish;
				// of << "I'm now choosing dish " << needThings.back() << "(" << Choose_Dish.x << ", " << Choose_Dish.y << ") to cooker!" << endl;
				needThings.pop_back();
				flag = Process1::GoToPutDishToCooker;
				clear();
			}
			else {
				// of << "Something wrong?" << endl;
				flag = Process1::DecideWhichToCook;
			}
		}
		else {
			// of << "Now I can cook!" << endl;
			Destination = Choose_Cooker;
			flag = Process1::CheckCooker;
			clear();
		}
		break;
	case Process1::GoToPutDishToCooker:
		if (mahattanDistance(PlayerInfo.position, Choose_Dish) <= 0.5) {
			// // of << "I'm now in (" << PlayerInfo.position.x << ", " << PlayerInfo.position.y << ")!" << endl;
			// of << "I'm picking dish " << FindDish << " just here(" << Choose_Dish.x << ", " << Choose_Dish.y << ")!" << endl;
			// // of << "mahattanDistance: " << mahattanDistance(PlayerInfo.position, Choose_Dish) << endl;
			while (PlayerInfo.dish != FindDish) {
				pick(true, ObjType::Dish, FindDish);
				Sleep(sleep_time);
				// wait();
				error++;
				if (error == max_error - 1) {
					pick(false, ObjType::Dish, FindDish);
					Sleep(sleep_time);
					// wait();
				}
				if (error >= max_error) {
					error = 0;
					move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					flag = Process1::DecideWhichToCook;
					return;
				}
			}
			error = 0;
			flag = Process1::PutDishToCooker;
			break;
		}
		else if (mahattanDistance(PlayerInfo.position, Choose_Dish) <= 1.5) {
			// // of << "I'm now in (" << PlayerInfo.position.x << ", " << PlayerInfo.position.y << ")!" << endl;
			adjustFacingDirection(Choose_Dish);
			// of << "I'm picking dish " << FindDish << "(" << Choose_Dish.x << ", " << Choose_Dish.y << ")!" << endl;
			// // of << "mahattanDistance: " << mahattanDistance(PlayerInfo.position, Choose_Dish) << endl;
			while (PlayerInfo.dish != FindDish) {
				pick(false, ObjType::Dish, FindDish);
				Sleep(sleep_time);
				// wait();
				error++;
				if (error == max_error - 1) {
					pick(true, ObjType::Dish, FindDish);
					Sleep(sleep_time);
					// wait();
				}
				if (error >= max_error) {
					error = 0;
					move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					flag = Process1::DecideWhichToCook;
					return;
				}
			}
			error = 0;
			flag = Process1::PutDishToCooker;
			break;
		}
		else { GoTo(Choose_Dish); }
		break;
	case Process1::PutDishToCooker:
		// of << "I'm now putting it to the cooker!" << endl;
		while (PlayerInfo.dish != DishType::DishEmpty) {
			put(Choose_Cooker, true);
			error++;
			if (error >= max_error) {
				move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
				flag = Process1::DecideWhichToCook;
				error = 0;
				return;
			}
		}
		flag = Process1::DecideWhichToPut;
		error = 0;
		break;
	case Process1::CheckCooker:
		if (mahattanDistance(PlayerInfo.position, Choose_Cooker) <= 1.5) {
			adjustFacingDirection(Choose_Cooker);
			mapcell = MapInfo::get_mapcell(Choose_Cooker.x, Choose_Cooker.y);
			temp.clear();
			for (list<Obj>::iterator it = mapcell.begin(); it != mapcell.end(); ++it) {
				if (it->objType == ObjType::Dish) {
					// of << it->dish << " ";
					temp.push_back(it->dish);
				}
			}
			for (vector<DishType>::iterator iter1 = temp.begin(); iter1 != temp.end(); ++iter1) {
				if (!isIn(needThings_back, *iter1)) {
					pick(false, ObjType::Dish, *iter1);
					Sleep(2 * sleep_time);
					put(ThrowPosition, true);
				}
			}
			mapcell = MapInfo::get_mapcell(Choose_Cooker.x, Choose_Cooker.y);
			temp.clear();
			for (list<Obj>::iterator it = mapcell.begin(); it != mapcell.end(); ++it) {
				if (it->objType == ObjType::Dish) {
					// of << it->dish << " ";
					temp.push_back(it->dish);
				}
			}
			for (vector<DishType>::iterator iter2 = needThings_back.begin(); iter2 != needThings_back.end(); ++iter2) {
				if (!isIn(temp, *iter2)) {
					flag = Process1::DecideWhichToCook;
					return;
				}
			}
			flag = Process1::Cooking;
		}
		else {
			GoTo(Choose_Cooker);
		}
		// flag = Process1::Cooking;
		break;
	case Process1::Cooking:
		if (mahattanDistance(PlayerInfo.position, Choose_Cooker) <= 1.5) {
			adjustFacingDirection(Choose_Cooker);
			// of << "Begin cooking!" << endl;
			while (!isCooking(Choose_Cooker)) {
				use(0, 0, 0);
				Sleep(sleep_time);
				// wait();
				error++;
				if (error >= max_error) {
					error = 0;
					flag = Process1::DecideWhichToCook;
					return;
				}
			}
			flag = Process1::WaitCookFinish;
			error = 0;
		}
		else {
			GoTo(Choose_Cooker);
		}
		break;
	case Process1::WaitCookFinish:
		if (stop) {
			stop = false;
			// of << "I'm waiting cook finish! So I tell my teammate to go!" << endl;
			speak = 0;
			while (PlayerInfo.recieveText != "Go") {
				speakToFriend("Go");
				Sleep(speak_time);
				// wait();
				speak++;
				// of << "Speak " << speak << endl;
				if (speak >= max_speak) {
					// stop = true;
					speak = 0;
					break;
				}
			}
		}
		Sleep(sleep_time);
		wait_time += sleep_time;
		mapcell = MapInfo::get_mapcell(Choose_Cooker.x, Choose_Cooker.y);
		for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
			if (iter->objType == ObjType::Block && iter->blockType == BlockType::Cooker) {
				if (iter->dish == DishType::DishEmpty) {
					flag = Process1::DecideWhichToCook;
					return;
				}
				if (iter->dish != DishType::CookingDish) {
					if (!stop) {
						stop = true;
						// of << "Cook done! So I tell my teammate to stop!" << endl;
						speak = 0;
						while (PlayerInfo.recieveText != "Stop") {
							speakToFriend("Stop");
							Sleep(speak_time);
							// wait();
							speak++;
							// of << "Speak " << speak << endl;
							if (speak >= max_speak) {
								// stop = false;
								speak = 0;
								break;
							}
						}
					}
					adjustFacingDirection(Choose_Cooker);
					while (PlayerInfo.dish == DishType::DishEmpty) {
						pick(false, ObjType::Block, 0);
						Sleep(sleep_time);
						// wait();
						error++;
						if (error >= max_error) {
							error = 0;
							wait_time = 0;
							// of << "I can't pick dish from cooker!" << endl;
							move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
							flag = Process1::DecideWhichToCook;
							return;
						}
					}
					for (list<DishType>::iterator iter = task_list.begin(); iter != task_list.end(); ++iter) {
						if (PlayerInfo.dish == *iter) {
							wait_time = 0;
							last_flag = Process1::GoToCooker;
							flag = Process1::DecideWhereToHandTask;
							// of << "Now I can hand in my dish!" << endl;
							HandingDish = *iter;
							return;
						}
					}
					/*
					xi = 0; yi = 0;
					while (xi == 0 && yi == 0) {
						xi = 1 - rand() % 3;
						yi = 1 - rand() % 3;
					}
					ThrowPosition = XYPosition(Choose_Cooker.x + xi, Choose_Cooker.y + yi);
					*/
					while (PlayerInfo.dish != DishType::DishEmpty) {
						put(ThrowPosition, true);
						error++;
						if (error >= max_error) {
							error = 0;
							wait_time = 0;
							// of << "I can't push my dish in my hand!" << endl;
							move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
							flag = Process1::DecideWhichToCook;
							return;
						}
					}
					// of << "I put it!" << endl;
					flag = Process1::DecideWhichToCook;
					wait_time = 0;
					error = 0;
					return;
				}
			}
		}
		if (wait_time >= max_wait_time) {
			wait_time = 0;
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				if (iter->objType == ObjType::Block && iter->blockType == BlockType::Cooker) {
					if (iter->dish == DishType::DishEmpty) {
						flag = Process1::DecideWhichToCook;
						break;
					}
					else{
						// of << "I cooked wrong! Throw it!" << endl;
						pick(false, ObjType::Block, 0);
						Sleep(sleep_time);
						// wait();
						flag = Process1::DecideWhereToThrow;
						break;
					}
				}
			}
		}
		break;
	case Process1::DecideWhereToThrow:
		Choose_Dustbin = findMin(rubbishBin);
		Destination = Choose_Dustbin;
		flag = Process1::ThrowToDustbin;
		clear();
		break;
	case Process1::ThrowToDustbin:
		GoTo(Choose_Dustbin);
		if (euclideanDistance(Choose_Dustbin, PlayerInfo.position) <= 10) {
			if (put(Choose_Dustbin, true)) {
				// of << "Throw it!" << endl;
			}
			if (PlayerInfo.dish == DishType::DishEmpty) {
				flag = Process1::GoToCooker;
			}			
		}
		break;
	case Process1::GoingToPickDishToHand:
		if (mahattanDistance(PlayerInfo.position, HandingDishPosition) <= 0.5) {
			// of << "Get to the dish(" << HandingDishPosition.x << ", " << HandingDishPosition.y << ")" << endl;
			while (PlayerInfo.dish != HandingDish) {
				pick(true, ObjType::Dish, HandingDish);
				Sleep(sleep_time);
				// wait();
				error++;
				if (error == max_error - 1) {
					pick(false, ObjType::Dish, HandingDish);
					Sleep(sleep_time);
					// wait();
				}
				if (error >= max_error) {
					move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					error = 0;
					// of << "Something wrong!" << endl;
					flag = last_flag;
					return;
				}
			}
			error = 0;
			flag = Process1::DecideWhereToHandTask;
			break;
		}
		else if (mahattanDistance(PlayerInfo.position, HandingDishPosition) <= 1.5) {
			// of << "Get to the dish(" << HandingDishPosition.x << ", " << HandingDishPosition.y << ")" << endl;
			// // of << "Adjust the Facing direction!" << endl;
			// // of << "Facing Direction: " << PlayerInfo.facingDirection << endl;
			adjustFacingDirection(HandingDishPosition);
			while (PlayerInfo.dish != HandingDish) {
				pick(false, ObjType::Dish, HandingDish);
				Sleep(sleep_time);
				// wait();
				error++;
				if (error == max_error - 1) {
					pick(true, ObjType::Dish, HandingDish);
					Sleep(sleep_time);
				}
				if (error >= max_error) {
					move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					error = 0;
					// of << "Something wrong!" << endl;
					flag = last_flag;
					return;
				}
			}
			error = 0;
			flag = Process1::DecideWhereToHandTask;
			break;
			// // of << "Facing Direction: " << PlayerInfo.facingDirection << endl;
		}
		else { GoTo(HandingDishPosition); }
		break;
	case Process1::DecideWhereToHandTask:
		// of << "I'm deciding where to hand task! emmmmmm......";
		Choose_TaskPoint = findMin(taskPoint);
		Destination = Choose_TaskPoint;
		// of << "I choose the taskPoint(" << Choose_TaskPoint.x << ", " << Choose_TaskPoint.y << ")!" << endl;
		flag = Process1::GoingToTaskPoint;
		clear();
		break;
	case Process1::GoingToTaskPoint:
		if (mahattanDistance(PlayerInfo.position, Choose_TaskPoint) <= 0.5) {
			// of << "Get to the taskPoint(" << Choose_TaskPoint.x << ", " << Choose_TaskPoint.y << ")" << endl;
			flag = Process1::HandingTask;
		}
		else if (mahattanDistance(PlayerInfo.position, Choose_TaskPoint) <= 1.5) {
			// of << "Get to the taskPoint(" << Choose_TaskPoint.x << ", " << Choose_TaskPoint.y << ")" << endl;
			// // of << "Adjust the Facing direction!" << endl;
			// // of << "Facing Direction: " << PlayerInfo.facingDirection << endl;
			adjustFacingDirection(Choose_TaskPoint);
			flag = Process1::HandingTask;
			// // of << "Facing Direction: " << PlayerInfo.facingDirection << endl;
		}
		else { GoTo(Choose_TaskPoint); }
		break;
	case Process1::HandingTask:
		// of << "I'm now handing task!" << endl;
		if (PlayerInfo.tool == ToolType::Condiment) {
			// of << "I use the condiment to get more score!" << endl;
			use(PlayerInfo.tool);
			Sleep(sleep_time);
		}
		while (PlayerInfo.dish != DishType::DishEmpty) {
			use(0, 0, 0);
			Sleep(sleep_time);
			error++;
			if (error >= max_error) {
				move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
				error = 0;
				flag = last_flag;
				return;
			}
		}
		// of << "Now our score is " << PlayerInfo.score << endl;
		flag = last_flag;
		break;
	case Process1::ReturnToWait:
		if (mahattanDistance(PlayerInfo.position, Choose_Cooker) <= 1.5) {
			// of << "Get back to wait!" << endl;
			adjustFacingDirection(Choose_Cooker);
			flag = Process1::WaitCookFinish;
		}
		else {
			GoTo(Choose_Cooker);
		}
		break;
	case Process1::PickAndUseTool:
		if (mahattanDistance(PlayerInfo.position, Choose_Tool) <= 0.5) {
			adjustFacingDirection(Choose_Tool);
			mapcell = MapInfo::get_mapcell(Choose_Tool.x, Choose_Tool.y);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				if (iter->objType == ObjType::Tool) {
					pick(true, ObjType::Tool, iter->tool);
					Sleep(2 * sleep_time);
					if (PlayerInfo.tool == ToolType::ToolEmpty) {
						pick(false, ObjType::Tool, iter->tool);
						Sleep(2 * sleep_time);
						move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					}
					break;
				}
			}
			if (useTool(PlayerInfo.tool)) {
				// of << "Use tool: " << PlayerInfo.tool << endl;
			}
			clear();
			flag = last_flag;
			break;
		}else if (mahattanDistance(PlayerInfo.position, Choose_Tool) <= 1.5) {
			adjustFacingDirection(Choose_Tool);
			mapcell = MapInfo::get_mapcell(Choose_Tool.x, Choose_Tool.y);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				if (iter->objType == ObjType::Tool) {
					pick(false, ObjType::Tool, iter->tool);
					Sleep(2 * sleep_time);
					if (PlayerInfo.tool == ToolType::ToolEmpty) {
						pick(true, ObjType::Tool, iter->tool);
						Sleep(2 * sleep_time);
						move(static_cast<Direction>(rand() % 4 * 2), 50); Sleep(50);
					}
					break;
				}
			}
			if (useTool(PlayerInfo.tool)) {
				// of << "Use tool: " << PlayerInfo.tool << endl;
			}
			clear();
			flag = last_flag;
			break;
		}
		else { GoTo(Choose_Tool); }
		break;
	case Process1::Finish:
		// of << "Finish!" << endl;
		flag = Process1::Process1Size;
		break;
	default:
		break;
	}
	// of.flush();
}

bool findPeople(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::People) {
						positions.push_back(this_obj.position);
				}
			}
		}
	}
	return !positions.empty();
}

bool findEnermy(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::People) {
					if (this_obj.team != PlayerInfo.team) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findBlock(vector<XYPosition>& positions, vector<Obj>& objects) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Block) {
					positions.push_back(this_obj.position);
					objects.push_back(this_obj);
				}
			}
		}
	}
	return !positions.empty();
}

bool findBlock(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Block) {
					positions.push_back(this_obj.position);
				}
			}
		}
	}
	return !positions.empty();
}

bool findBlock(vector<XYPosition>& positions, BlockType block) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Block) {
					if (this_obj.blockType == block) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findDish(vector<XYPosition>& positions, vector<Obj>& objects) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Dish) {
					positions.push_back(this_obj.position);
					objects.push_back(this_obj);
				}
			}
		}
	}
	return !positions.empty();
}

bool findDish(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Dish) {
					positions.push_back(this_obj.position);
				}
			}
		}
	}
	return !positions.empty();
}

bool findDish(vector<XYPosition>& positions, DishType dish) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Dish) {
					if (this_obj.dish == dish) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findTool(vector<XYPosition>& positions, vector<Obj>& objects) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Tool) {
					if (this_obj.tool != ToolType::Fertilizer && this_obj.tool != ToolType::Stealer
						&& this_obj.tool != ToolType::SpaceGate && this_obj.tool != ToolType::BreastPlate
						&& this_obj.tool != ToolType::TeleScope && this_obj.tool != ToolType::TigerShoes) {
						positions.push_back(this_obj.position);
						objects.push_back(this_obj);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findTool(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Tool) {
					if (this_obj.tool != ToolType::Fertilizer && this_obj.tool != ToolType::Stealer
						&& this_obj.tool != ToolType::SpaceGate && this_obj.tool != ToolType::BreastPlate
						&& this_obj.tool!=ToolType::TeleScope && this_obj.tool!=ToolType::TigerShoes) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findTool(vector<XYPosition>& positions, ToolType tool) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Tool) {
					if (this_obj.tool == tool) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool findTrigger(vector<XYPosition>& positions, vector<Obj>& objects) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Trigger) {
					positions.push_back(this_obj.position);
					objects.push_back(this_obj);
				}
			}
		}
	}
	return !positions.empty();
}

bool findTrigger(vector<XYPosition>& positions) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Trigger) {
					positions.push_back(this_obj.position);				}
			}
		}
	}
	return !positions.empty();
}

bool findTrigger(vector<XYPosition>& positions, TriggerType trigger) {
	int x = PlayerInfo.position.x;
	int y = PlayerInfo.position.y;
	positions.clear();
	for (int xi = x - PlayerInfo.sightRange + 1; xi < x + PlayerInfo.sightRange; xi++) {
		for (int yi = y - PlayerInfo.sightRange + 1; yi < y + PlayerInfo.sightRange; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				Obj this_obj = *iter;
				if (this_obj.objType == ObjType::Trigger) {
					if (this_obj.trigger == trigger) {
						positions.push_back(this_obj.position);
					}
				}
			}
		}
	}
	return !positions.empty();
}

bool isPeople(XYPosition position) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::People && (this_obj.position.x != PlayerInfo.position.x || this_obj.position.y != PlayerInfo.position.y)) {
			return true;
		}
	}
	return false;
}

bool isBlock(XYPosition position) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Block) {
			return true;
		}
	}
	return false;
}

bool isBlock(XYPosition position, BlockType block) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Block) {
			if (this_obj.blockType == block) {
				return true;
			}
		}
	}
	return false;
}

bool isDish(XYPosition position) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Dish) {
			return true;
		}
	}
	return false;
}

bool isDish(XYPosition position, DishType dish) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Dish) {
			if (this_obj.dish == dish) {
				return true;
			}
		}
	}
	return false;
}

bool isTool(XYPosition position) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Tool) {
			return true;
		}
	}
	return false;
}

bool isTool(XYPosition position, ToolType tool) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Tool) {
			if (this_obj.tool == tool) {
				return true;
			}
		}
	}
	return false;
}

bool isTrigger(XYPosition position) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Trigger) {
			return true;
		}
	}
	return false;
}

bool isTrigger(XYPosition position, TriggerType trigger) {
	if (abs(position.x - PlayerInfo.position.x) > PlayerInfo.sightRange || abs(position.y - PlayerInfo.position.y) > PlayerInfo.sightRange) {
		return false;
	}
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		Obj this_obj = *iter;
		if (this_obj.objType == ObjType::Trigger) {
			if (this_obj.trigger == trigger) {
				return true;
			}
		}
	}
	return false;
}

inline double euclideanDistance(XYPosition position1, XYPosition position2) {
	return euclideanDistance(position1.x, position1.y, position2.x, position2.y);
}

inline double euclideanDistance(double x, double y, double x1, double y1) {
	return sqrt((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y));
}

inline int mahattanDistance(XYPosition position1, XYPosition position2) {
	return mahattanDistance(position1.x, position1.y, position2.x, position2.y);
}

inline int mahattanDistance(int x, int y, int x1, int y1) {
	return abs(x - x1) + abs(y - y1);
}

void GoTo(XYPosition position) {
	double speed = (1000 / PlayerInfo.moveSpeed);
	double min_distance = 99999999;
	double distance = 0;
	bool straight = 1;
	Direction direction = RandomDirection.front();
	RandomDirection.pop();
	RandomDirection.push(direction);
	if (go_time >= max_go_time) {
		go_time = 0;
		clear();
	}
	if (random_time >= max_random_time) {
		// of << "I get tripped! Clear!" << endl;
		random_time = 0;
		clear();
	}
	if (stop_time >= max_stop_time) {
		// of << "Random move!" << endl;
		move(direction, random_go_time * speed); Sleep(random_go_time * speed);
		stop_time = 0;
		random_time++;
		// clear();
		return;
	}
	if (rand() % random_probability == 0 || (LastLastPosition.x == PlayerInfo.position.x && LastLastPosition.y == PlayerInfo.position.y)) {
		int move_time = rand() % 5;
		move(direction, move_time * speed); Sleep(move_time * speed);
		return;
	}
	if (PlayerInfo.position.x == LastPosition.x && PlayerInfo.position.y == LastPosition.y) {
		// cout << "Trap!" << endl;
		LastLastPosition = LastPosition;
		LastPosition = PlayerInfo.position;
		stop_time++;
	}
	else {
		stop_time = 0;
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x][(int)PlayerInfo.position.y + 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier))) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x, PlayerInfo.position.y + 1), position) + g(XYPosition(PlayerInfo.position.x, PlayerInfo.position.y + 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::Up;
			straight = 1;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x + 1][(int)PlayerInfo.position.y] &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn))) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y), position) + g(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::Right;
			straight = 1;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x][(int)PlayerInfo.position.y - 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier))) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x, PlayerInfo.position.y - 1), position) + g(XYPosition(PlayerInfo.position.x, PlayerInfo.position.y - 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::Down;
			straight = 1;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x - 1][(int)PlayerInfo.position.y] &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn))) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y), position) + g(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::Left;
			straight = 1;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x + 1][(int)PlayerInfo.position.y + 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier))
		) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y + 1), position) + g(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y + 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::RightUp;
			straight = 0;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x - 1][(int)PlayerInfo.position.y + 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y + barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y + barrier))
		) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y + 1), position) + g(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y + 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::LeftUp;
			straight = 0;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x + 1][(int)PlayerInfo.position.y - 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier))
		) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y - 1), position) + g(XYPosition(PlayerInfo.position.x + 1, PlayerInfo.position.y - 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::RightDown;
			straight = 0;
		}
	}
	if (!HaveBeenTo[(int)PlayerInfo.position.x - 1][(int)PlayerInfo.position.y - 1] &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - volumn)) &&
		!isBlock(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isBlock(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y + volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - barrier, PlayerInfo.position.y - volumn)) &&
		!isPeople(XYPosition(PlayerInfo.position.x + volumn, PlayerInfo.position.y - barrier)) &&
		!isPeople(XYPosition(PlayerInfo.position.x - volumn, PlayerInfo.position.y - barrier))
		) {
		distance = euclideanDistance(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y - 1), position) + g(XYPosition(PlayerInfo.position.x - 1, PlayerInfo.position.y - 1));
		if (distance < min_distance) {
			min_distance = distance;
			direction = Direction::LeftDown;
			straight = 0;
		}
	}
	// // of << "Direction: " << direction << endl;
	/*
	switch (direction) {
	case Direction::Right:HaveBeenTo[(int)PlayerInfo.position.x + 1][(int)PlayerInfo.position.y] = true; break;
	case Direction::Left:HaveBeenTo[(int)PlayerInfo.position.x - 1][(int)PlayerInfo.position.y] = true; break;
	case Direction::Up:HaveBeenTo[(int)PlayerInfo.position.x][(int)PlayerInfo.position.y + 1] = true; break;
	case Direction::Down:HaveBeenTo[(int)PlayerInfo.position.x][(int)PlayerInfo.position.y - 1] = true; break;
	}
	*/
	LastLastPosition = LastPosition;
	LastPosition = PlayerInfo.position;
	if (min_distance == 99999999) {
		int move_time = rand() % 5;
		move(direction, move_time * speed); Sleep(move_time * speed);
		clear();
		return;
	}
	if (straight) {
		move(direction, speed); Sleep(speed / 2);
	}
	else if (!straight) {
		move(direction, speed* sqrt(2)); Sleep(speed* sqrt(2) / 2);
	}
	HaveBeenTo[(int)PlayerInfo.position.x][(int)PlayerInfo.position.y] = true;
	go_time++;
}

void GoToDestination(XYPosition position) {
	int walls[50][50] = { 0 };
	double speed = 1000 / PlayerInfo.moveSpeed;
	struct XY {
		int x, y;
		XY(int x1, int y1) { x = x1, y = y1; }
	};
	class node {
	public:
		int xPosition;
		int yPosition;
		char direction;
		int cost;
		node(int x, int y, char d, int c) {
			xPosition = x, yPosition = y, direction = d, cost = c;
		}
		bool operator== (const node& n) const {
			return this->xPosition == n.xPosition && this->yPosition == n.yPosition;
		}
	};
	class nodeComparition {
	public:
		bool operator() (list<node>& lhs, list<node>& rhs) {
			return rhs.size() + (--rhs.end())->cost < lhs.size() + (--lhs.end())->cost;
		}
	};
	priority_queue<list<node>, vector<list<node>>, nodeComparition> mypq;
	list<node> firstnode;
	firstnode.push_back(node((int)PlayerInfo.position.x, (int)PlayerInfo.position.y, 's', 0));
	mypq.push(firstnode);
	list<XY> explored;
	list<node> successors;
	while (!mypq.empty()) {
		list<node> leafnode = mypq.top();
		mypq.pop();
		XY currentstate = XY(leafnode.back().xPosition, leafnode.back().yPosition);
		if (currentstate.x == (int)position.x && currentstate.y == (int)position.y) {
			for (list<node>::iterator iter = ++leafnode.begin(); iter != leafnode.end(); ++iter) {
				switch (iter->direction) {
				case 'd': move(Direction::Right, speed); Sleep(speed);  break;
				case 'e': move(Direction::RightUp, speed); Sleep(speed); break;
				case 'w': move(Direction::Up, speed); Sleep(speed); break;
				case 'q': move(Direction::LeftUp, speed); Sleep(speed);  break;
				case 'a': move(Direction::Left, speed); Sleep(speed);  break;
				case 'z': move(Direction::LeftDown, speed); Sleep(speed);  break;
				case 'x': move(Direction::Down, speed); Sleep(speed);  break;
				case 'c': move(Direction::RightDown, speed); Sleep(speed); break;
				}
			}
			return;
		}
		bool inExplored = false;
		for (list<XY>::iterator iter = explored.begin(); iter != explored.end(); ++iter) {
			if (iter->x == currentstate.x && iter->y == currentstate.y) {
				inExplored = true;
				break;
			}
		}
		if (inExplored) continue;
		explored.push_back(currentstate);
		if (0 <= currentstate.x + 1 && currentstate.x + 1 < 50 && 0 <= currentstate.y && currentstate.y < 50) {
			if (init_mapinfo[currentstate.x + 1][currentstate.y] == 0)
				successors.push_back(node(currentstate.x + 1, currentstate.y, 'd', mahattanDistance(currentstate.x + 1, currentstate.y, position.x, position.y)));
		}
		if (0 <= currentstate.x + 1 && currentstate.x + 1 < 50 && 0 <= currentstate.y + 1 && currentstate.y + 1 < 50) {
			if (init_mapinfo[currentstate.x + 1][currentstate.y + 1] == 0)
				successors.push_back(node(currentstate.x + 1, currentstate.y + 1, 'e', mahattanDistance(currentstate.x + 1, currentstate.y + 1, position.x, position.y)));
		}
		if (0 <= currentstate.x && currentstate.x < 50 && 0 <= currentstate.y + 1 && currentstate.y + 1 < 50) {
			if (init_mapinfo[currentstate.x][currentstate.y + 1] == 0)
				successors.push_back(node(currentstate.x, currentstate.y + 1, 'w', mahattanDistance(currentstate.x, currentstate.y + 1, position.x, position.y)));
		}
		if (0 <= currentstate.x - 1 && currentstate.x - 1 < 50 && 0 <= currentstate.y + 1 && currentstate.y + 1 < 50) {
			if (init_mapinfo[currentstate.x - 1][currentstate.y + 1] == 0)
				successors.push_back(node(currentstate.x - 1, currentstate.y + 1, 'q', mahattanDistance(currentstate.x - 1, currentstate.y + 1, position.x, position.y)));
		}
		if (0 <= currentstate.x - 1 && currentstate.x - 1 < 50 && 0 <= currentstate.y && currentstate.y < 50) {
			if (init_mapinfo[currentstate.x - 1][currentstate.y] == 0)
				successors.push_back(node(currentstate.x - 1, currentstate.y, 'a', mahattanDistance(currentstate.x - 1, currentstate.y, position.x, position.y)));
		}
		if (0 <= currentstate.x - 1 && currentstate.x - 1 < 50 && 0 <= currentstate.y - 1 && currentstate.y - 1 < 50) {
			if (init_mapinfo[currentstate.x - 1][currentstate.y - 1] == 0)
				successors.push_back(node(currentstate.x - 1, currentstate.y - 1, 'z', mahattanDistance(currentstate.x - 1, currentstate.y - 1, position.x, position.y)));
		}
		if (0 <= currentstate.x && currentstate.x < 50 && 0 <= currentstate.y - 1 && currentstate.y - 1 < 50) {
			if (init_mapinfo[currentstate.x][currentstate.y - 1] == 0)
				successors.push_back(node(currentstate.x, currentstate.y - 1, 'x', mahattanDistance(currentstate.x, currentstate.y - 1, position.x, position.y)));
		}
		if (0 <= currentstate.x + 1 && currentstate.x + 1 < 50 && 0 <= currentstate.y - 1 && currentstate.y - 1 < 50) {
			if (init_mapinfo[currentstate.x + 1][currentstate.y - 1] == 0)
				successors.push_back(node(currentstate.x + 1, currentstate.y - 1, 'c', mahattanDistance(currentstate.x + 1, currentstate.y - 1, position.x, position.y)));
		}
		while (!successors.empty()) {
			node successor = successors.back();
			successors.pop_back();
			inExplored = false;
			for (list<XY>::iterator iter = explored.begin(); iter != explored.end(); ++iter) {
				if (iter->x == successor.xPosition && iter->y == successor.yPosition) {
					inExplored = true;
					break;
				}
			}
			if (inExplored) continue;
			list<node> successorpath = leafnode;
			successorpath.push_back(successor);
			mypq.push(successorpath);
		}
	}
}

void adjustFacingDirection(XYPosition position) {
	double speed = 1000 / PlayerInfo.moveSpeed;
	if (position.x - PlayerInfo.position.x > 0.75) {
		// move(Direction::Right, speed); Sleep(speed);
		move(Direction::Right, 0); Sleep(sleep_time);
		return;
	}
	if (position.x - PlayerInfo.position.x < -0.75) {
		// move(Direction::Left, speed); Sleep(speed);
		move(Direction::Left, 0); Sleep(sleep_time);
		return;
	}
	if (position.y - PlayerInfo.position.y > 0.75) {
		// move(Direction::Up, speed); Sleep(speed);
		move(Direction::Up, 0); Sleep(sleep_time);
		return;
	}
	if (position.y - PlayerInfo.position.y < -0.75) {
		// move(Direction::Down, speed); Sleep(speed);
		move(Direction::Down, 0); Sleep(sleep_time);
		return;
	}
}

void clear() {
	for (int i = 0; i < 50; i++) {
		for (int j = 0; j < 50; j++) {
			HaveBeenTo[i][j] = false;
		}
	}
}

XYPosition findMin(vector<XYPosition> positions, XYPosition last) {
	double min_distance = 99999999;
	double distance = 0;
	XYPosition destination(0, 0);
	for (vector<XYPosition>::iterator iter = positions.begin(); iter != positions.end(); ++iter) {
		distance = mahattanDistance(PlayerInfo.position, *iter);
		if (iter->x != last.x && iter->y != last.y && distance < min_distance) {
			min_distance = distance;
			destination = *iter;
		}
	}
	return destination;
}

XYPosition findMin(vector<XYPosition> positions) {
	return findMin(positions, XYPosition(-1, -1));
}

bool useTool(ToolType tool) {
	switch (tool)
	{
	case Protobuf::ToolEmpty:
		break;
	case Protobuf::TigerShoes:
		break;
	case Protobuf::SpeedBuff:
	case Protobuf::StrengthBuff:
		use(tool, 0, 0);
		Sleep(sleep_time);
		return true;
		break;
	case Protobuf::TeleScope:
	case Protobuf::Condiment:
	case Protobuf::Fertilizer:
		break;
	case Protobuf::BreastPlate:
		// use(tool, 0, 0);
		// Sleep(sleep_time);
		// return true;
		break;
	case Protobuf::SpaceGate:
		break;
	case Protobuf::WaveGlueBottle:
	case Protobuf::LandMine:
	case Protobuf::TrapTool:
	case Protobuf::FlashBomb:
		use(tool, 0, 0);
		Sleep(sleep_time);
		return true;
		break;
	case Protobuf::ThrowHammer:
	case Protobuf::Bow:
		// use(tool, 1, PI);
		// Sleep(sleep_time);
		// return true;
		break;
	case Protobuf::Stealer:
		break;
	case Protobuf::ToolSize:
		break;
	case Protobuf::ToolType_INT_MIN_SENTINEL_DO_NOT_USE_:
		break;
	case Protobuf::ToolType_INT_MAX_SENTINEL_DO_NOT_USE_:
		break;
	default:
		break;
	}
	return false;
}

bool put(XYPosition destination, bool isThrowDish) {
	//�Ӳ���
	if (euclideanDistance(PlayerInfo.position, destination) > 10/*PlayerInfo.maxThrowDistance*/) {
		return false;
	}
	//���ϰ���
	int x1 = destination.x < PlayerInfo.position.x ? destination.x : PlayerInfo.position.x;
	int x2 = destination.x < PlayerInfo.position.x ? PlayerInfo.position.x : destination.x;
	int y1 = destination.y < PlayerInfo.position.y ? destination.y : PlayerInfo.position.y;
	int y2 = destination.y < PlayerInfo.position.y ? PlayerInfo.position.y : destination.y;
	for (int xi = x1; xi <= x2; xi++) {
		for (int yi = y1; yi <= y2; yi++) {
			list<Obj> mapcell = MapInfo::get_mapcell(xi, yi);
			for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
				if (iter->objType == ObjType::Block) {
					if (iter->blockType == BlockType::Wall || iter->blockType == BlockType::FoodPoint ||
						iter->blockType == BlockType::TaskPoint) {
						return false;
					}
				}
			}
		}
	}
	//��
	// // of << "Put: " << euclideanDistance(PlayerInfo.position, destination) << ", " << atan2(destination.y - PlayerInfo.position.y, destination.x - PlayerInfo.position.x) << endl;
	put(euclideanDistance(PlayerInfo.position, destination), atan2(destination.y - PlayerInfo.position.y, destination.x - PlayerInfo.position.x), isThrowDish);
	Sleep(1000 * euclideanDistance(PlayerInfo.position, destination) / 10);
	return true;
}

bool isCooking(XYPosition position) {
	list<Obj> mapcell = MapInfo::get_mapcell(position.x, position.y);
	for (list<Obj>::iterator iter = mapcell.begin(); iter != mapcell.end(); ++iter) {
		if (iter->objType == ObjType::Block && iter->blockType == BlockType::Cooker) {
			return !(iter->dish == DishType::DishEmpty);
		}
	}
	return false;
}

bool canDo() {
	while (PlayerInfo.dish != DishType::DishEmpty) {
		put(ThrowPosition, true);
		error++;
		if (error >= max_error) {
			error = 0;
			break;
		}
	}
	error = 0;
	vector<XYPosition> positions;
	DoingDish = DishType::DishEmpty;
	needThings.clear();
	if (findDish(positions, DishType::Tomato) && findDish(positions, DishType::Egg)) {
		DoingDish = DishType::TomatoFriedEgg;
		needThings.push_back(DishType::Tomato);
		needThings.push_back(DishType::Egg);
		return true;
	}
	if (findDish(positions, DishType::TomatoFriedEgg) && findDish(positions, DishType::Noodle)) {
		DoingDish = DishType::TomatoFriedEggNoodle;
		needThings.push_back(DishType::TomatoFriedEgg);
		needThings.push_back(DishType::Noodle);
		return true;
	}
	if (findDish(positions, DishType::Beef) && findDish(positions, DishType::Noodle)) {
		DoingDish = DishType::BeefNoodle;
		needThings.push_back(DishType::Beef);
		needThings.push_back(DishType::Noodle);
		return true;
	}
	/*
	if (findDish(positions, DishType::Pork) && findDish(positions, DishType::Cabbage) && findDish(positions, DishType::Rice)) {
		DoingDish = DishType::OverRice;
		needThings.push_back(DishType::Pork);
		needThings.push_back(DishType::Cabbage);
		needThings.push_back(DishType::Rice);
		return true;
	}
	*/
	if (findDish(positions, DishType::Pork) && findDish(positions, DishType::Potato) && findDish(positions, DishType::Rice)) {
		DoingDish = DishType::OverRice;
		needThings.push_back(DishType::Pork);
		needThings.push_back(DishType::Potato);
		needThings.push_back(DishType::Rice);
		return true;
	}
	/*
	if (findDish(positions, DishType::ChickenMeat) && findDish(positions, DishType::Potato) && findDish(positions, DishType::Rice)) {
		DoingDish = DishType::YellowPheasant;
		needThings.push_back(DishType::ChickenMeat);
		needThings.push_back(DishType::Potato);
		needThings.push_back(DishType::Rice);
		return true;
	}
	*/
	if (findDish(positions, DishType::Lettuce) && findDish(positions, DishType::Pork)) {
		DoingDish = DishType::Barbecue;
		needThings.push_back(DishType::Lettuce);
		needThings.push_back(DishType::Pork);
		return true;
	}
	if (findDish(positions, DishType::Potato) && findDish(positions, DishType::Ketchup)) {
		DoingDish = DishType::FrenchFries;
		needThings.push_back(DishType::Potato);
		needThings.push_back(DishType::Ketchup);
		return true;
	}
	/*
	if (findDish(positions, DishType::DarkPlum) && findDish(positions, DishType::Hawthorn)) {
		DoingDish = DishType::PlumJuice;
		needThings.push_back(DishType::Hawthorn);
		needThings.push_back(DishType::DarkPlum);
		return true;
	}
	*/
	if (findDish(positions, DishType::Beef) && findDish(positions, DishType::Lettuce) && findDish(positions, DishType::Bread)) {
		DoingDish = DishType::Hamburger;
		needThings.push_back(DishType::Beef);
		needThings.push_back(DishType::Lettuce);
		needThings.push_back(DishType::Bread);
		return true;
	}
	/*
	if (findDish(positions, DishType::Strawberry) && findDish(positions, DishType::Cream)) {
		DoingDish = DishType::StrawberryIcecream;
		needThings.push_back(DishType::Strawberry);
		needThings.push_back(DishType::Cream);
		return true;
	}
	*/
	/*
	if (findDish(positions, DishType::ChickenMeat) && findDish(positions, DishType::Ketchup)) {
		DoingDish = DishType::PopcornChicken;
		needThings.push_back(DishType::ChickenMeat);
		needThings.push_back(DishType::Ketchup);
		return true;
	}
	*/
	/*
	if (findDish(positions, DishType::Egg) && findDish(positions, DishType::Agaric)) {
		DoingDish = DishType::AgaricFriedEgg;
		needThings.push_back(DishType::Egg);
		needThings.push_back(DishType::Agaric);
		return true;
	}
	*/
	/*
	if (findDish(positions, DishType::Hawthorn)) {
		DoingDish = DishType::SugarCoatedHaws;
		needThings.push_back(DishType::Hawthorn);
		return true;
	}
	*/
	/*
	if (findDish(positions, DishType::Egg) && findDish(positions, DishType::Strawberry)
		&& findDish(positions, DishType::Flour) && findDish(positions, DishType::Cream)) {
		DoingDish = DishType::Cake;
		needThings.push_back(DishType::Egg);
		needThings.push_back(DishType::Strawberry);
		needThings.push_back(DishType::Flour);
		needThings.push_back(DishType::Cream);
		return true;
	}
	*/
	/*
	if (findDish(positions, DishType::Tomato) && findDish(positions, DishType::Apple)
		&& findDish(positions, DishType::Banana) && findDish(positions, DishType::Strawberry)) {
		DoingDish = DishType::FruitSalad;
		needThings.push_back(DishType::Tomato);
		needThings.push_back(DishType::Apple);
		needThings.push_back(DishType::Banana);
		needThings.push_back(DishType::Strawberry);
		return true;
	}
	*/
	if (findDish(positions, DishType::Wheat)) {
		DoingDish = DishType::Flour;
		needThings.push_back(DishType::Wheat);
		return true;
	}
	if (!findDish(positions, DishType::Ketchup) && findDish(positions, DishType::Tomato)) {
		DoingDish = DishType::Ketchup;
		needThings.push_back(DishType::Tomato);
		return true;
	}
	/*
	if (findDish(positions, DishType::Milk)) {
		DoingDish = DishType::Cream;
		needThings.push_back(DishType::Milk);
		return true;
	}
	*/
	if (!findDish(positions, DishType::CookedRice) && findDish(positions, DishType::Rice)) {
		DoingDish = DishType::CookedRice;
		needThings.push_back(DishType::Rice);
		return true;
	}
	if (!findDish(positions, DishType::Noodle) && findDish(positions, DishType::Flour)) {
		DoingDish = DishType::Noodle;
		needThings.push_back(DishType::Flour);
		return true;
	}
	if (!findDish(positions, DishType::Bread) && findDish(positions, DishType::Flour) && findDish(positions, DishType::Egg)) {
		DoingDish = DishType::Bread;
		needThings.push_back(Flour);
		needThings.push_back(Egg);
		return true;
	}
	return false;
}

XYPosition getThrowDishPosition(XYPosition cooker) {
	if (cooker.x == 8.5 && cooker.y == 24.5) {
		return XYPosition(8.5, 25.5);
	}
	if (cooker.x == 25.5 && cooker.y == 38.5) {
		return XYPosition(25.5, 37.5);
	}
	if (cooker.x == 33.5 && cooker.y == 18.5) {
		return XYPosition(33.5, 19.5);
	}
	if (cooker.x == 41.5 && cooker.y == 28.5) {
		return XYPosition(41.5, 27.5);
	}
	return XYPosition(-1, -1);
}

void initializeRandomDirection() {
	RandomDirection.push(Direction::Up);
	RandomDirection.push(Direction::Left);
	RandomDirection.push(Direction::Down);
	RandomDirection.push(Direction::Right);
}

double g(XYPosition position) {
	return g_factor * euclideanDistance(position, findMin(cooker, Choose_Cooker));
}

bool isIn(vector<DishType>& v, DishType d) {
	for (vector<DishType>::iterator it = v.begin(); it != v.end(); ++it) {
		if (*it == d) {
			return true;
		}
	}
	return false;
}