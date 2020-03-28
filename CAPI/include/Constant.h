#ifndef CONSTANT_H
#define CONSTANT_H
#include <unordered_map>
#include <map>
#include "MessageToClient.pb.h"

namespace Constant
{
	struct Player
	{
		const static int InitMoveSpeed = 5;
		const static int InitThrowDistance = 10;
		const static int InitSightRange = 9;
	};
	const static int ItemMoveSpeed = 10;
	const static int TaskRefreshTime = 8000;
	const static int ToolRefreshTime = 10000;
	const static int FoodPointInitRefreshTime = 10000;
	struct Trigger
	{
		struct WaveGlue
		{
			const static int ExtraMoveSpeed = -3;
			const static int Duration = 30000;
		};
		struct Mine
		{
			const static int StunDuration = 1000;
			const static int Score = -50;
		};
		struct Trap
		{
			const static int StunDuration = 5000;
		};
		struct Bomb
		{
			const static int StunDuration = 2000;
		};
		struct Arrow
		{
			const static int Score = -20;
			const static int StunDuration = 500;
			const static int Speed = 15;
		};
		struct Hammer
		{
			const static int StunDuration = 1000;
			const static int Speed = 10;
		};
	};
	struct Tool
	{
		struct SpeedBuff
		{
			const static int Duration = 20000;
			const static int ExtraMoveSpeed = 5;
		};
		struct StrengthBuff
		{
			const static int Duration = 40000;
			const static int ExtraThrowDistance = 5;
		};
		struct TigerShoe
		{
			const static int ExtraMoveSpeed = 2;
		};
		struct TeleScope
		{
			const static int ExtraSightRange = 4;
		};
		struct Condiment
		{
			const static double ScoreParameter;
		};
		struct SpaceGate
		{
			const static int MaxDistance = 10;
		};
		struct ThrowHammer
		{
			const static int MaxDistance = 6;
		};
		struct Bow
		{
			const static int MaxDistance = 10;
		};
	};
	struct Talent
	{
		struct LuckyBoy
		{
			const static int RefreshTime = 30000;
		};
		struct Runner
		{
			const static int ExtraMoveSpeed = 3;
		};
		struct StrongMan
		{
			const static int ExtraThrowDistance = 5;
			struct ThrowHammer
			{
				const static int MaxDistance = 10;
			};
			struct Bow
			{
				const static int MaxDistance = 15;
			};
			struct Arrow
			{
				const static int Score = -40;
				const static int StunDuration = 1000;
			};
			struct Hammer
			{
				const static int StunDuration = 2000;
			};
		};
		struct Technician
		{
			struct TigerShoe
			{
				const static int ExtraMoveSpeed = 3;
			};
			struct SpeedBuff
			{
				const static int Duration = 30000;
			};
			struct StrengthBuff
			{
				const static int Duration = 60000;
			};
			struct TeleScope
			{
				const static int ExtraSightRange = 6;
			};
			struct SpaceGate
			{
				const static int MaxDistance = 15;
			};
			struct Mine
			{
				const static int Score = -100;
				const static int StunDuration = 2000;
			};
			struct Trap
			{
				const static int StunDuration = 8000;
			};
			struct Bomb
			{
				const static int StunDuration = 4000;
			};
		};
		struct Cook
		{
			struct Condiment
			{
				const static double ScoreParameter;
			};
		};
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
	{int(Protobuf::Milk), {0,0,0} },
	{int(Protobuf::Apple), {0,0,0} },
	{int(Protobuf::Banana), {0,0,0} },
	{int(Protobuf::DarkPlum), {0,0,0} },
	{int(Protobuf::Hawthorn), {0,0,0} },
	{int(Protobuf::Strawberry), {0,0,0} },
	{int(Protobuf::Beef), {20,0,0} },
	{int(Protobuf::Pork), {20,0,0} },
	{int(Protobuf::ChickenMeat), {20,0,0} },
	{int(Protobuf::Potato), {20,0,0} },
	{int(Protobuf::Lettuce), {20,0,0} },
	{int(Protobuf::Agaric), {20,0,0} },
	{int(Protobuf::NeedleMushroom), {20,0,0} },
	{int(Protobuf::Cabbage), {20,0,0} },
	{int(Protobuf::Flour), {0,10000,0} },
	{int(Protobuf::Noodle), {0,10000,0} },
	{int(Protobuf::Bread), {0,10000,0} },
	{int(Protobuf::CookedRice), {0,10000,0} },
	{int(Protobuf::Ketchup), {0,10000,0} },
	{int(Protobuf::Cream), {0,10000,0} },
	{int(Protobuf::TomatoFriedEgg), {50,10000,60000} },
	{int(Protobuf::TomatoFriedEggNoodle), {100,15000,90000} },
	{int(Protobuf::BeefNoodle), {80,20000,90000} },
	{int(Protobuf::OverRice), {90,20000,90000} },
	{int(Protobuf::YellowPheasant), {100,20000,90000} },
	{int(Protobuf::Barbecue), {55,20000,90000} },
	{int(Protobuf::FrenchFries), {60,15000,90000} },
	{int(Protobuf::PlumJuice), {50,10000,90000} },
	{int(Protobuf::Hamburger), {110,20000,100000} },
	{int(Protobuf::StrawberryIcecream), {60,10000,90000} },
	{int(Protobuf::PopcornChicken), {60,15000,90000} },
	{int(Protobuf::AgaricFriedEgg), {50,15000,90000} },
	{int(Protobuf::Cake), {160,30000,120000} },
	{int(Protobuf::SugarCoatedHaws), {20,10000,60000} },
	{int(Protobuf::FruitSalad), {100,20000,120000} },
	{int(Protobuf::SpicedPot), {0,60000,300000} },
	{int(Protobuf::DarkDish), {-10,60000,30000} },
	{int(Protobuf::OverCookedDish), {-10,60000,300000} },
	};
	const static std::unordered_map<int, std::list<Protobuf::DishType>> CookingTable =
	{
	{int(Protobuf::Flour), {Protobuf::Wheat} },
	{int(Protobuf::Noodle), {Protobuf::Flour} },
	{int(Protobuf::Bread), {Protobuf::Egg,Protobuf::Flour} },
	{int(Protobuf::CookedRice), {Protobuf::Rice} },
	{int(Protobuf::Ketchup), {Protobuf::Tomato} },
	{int(Protobuf::Cream), {Protobuf::Milk} },
	{int(Protobuf::TomatoFriedEgg), {Protobuf::Tomato,Protobuf::Egg} },
	{int(Protobuf::TomatoFriedEggNoodle), {Protobuf::Noodle,Protobuf::TomatoFriedEgg} },
	{int(Protobuf::BeefNoodle), {Protobuf::Beef,Protobuf::Noodle} },
	{int(Protobuf::OverRice), {Protobuf::Pork,Protobuf::Cabbage,Protobuf::Rice} },
	{int(Protobuf::YellowPheasant), {Protobuf::ChickenMeat,Protobuf::Potato,Protobuf::Rice} },
	{int(Protobuf::Barbecue), {Protobuf::Lettuce,Protobuf::Pork} },
	{int(Protobuf::FrenchFries), {Protobuf::Potato,Protobuf::Ketchup} },
	{int(Protobuf::PlumJuice), {Protobuf::DarkPlum,Protobuf::Hawthorn} },
	{int(Protobuf::Hamburger), {Protobuf::Beef,Protobuf::Lettuce,Protobuf::Bread} },
	{int(Protobuf::StrawberryIcecream), {Protobuf::Strawberry,Protobuf::Cream} },
	{int(Protobuf::PopcornChicken), {Protobuf::ChickenMeat,Protobuf::Ketchup} },
	{int(Protobuf::AgaricFriedEgg), {Protobuf::Egg,Protobuf::Agaric} },
	{int(Protobuf::Cake), {Protobuf::Egg,Protobuf::Strawberry,Protobuf::Flour,Protobuf::Cream} },
	{int(Protobuf::SugarCoatedHaws), {Protobuf::Hawthorn} },
	{int(Protobuf::FruitSalad), {Protobuf::Tomato,Protobuf::Apple,Protobuf::Banana,Protobuf::Strawberry} },
	};

}
#endif