#ifndef CONSTANT_H
#define CONSTANT_H
#include <unordered_map>
#include <map>
#include "MessageToClient.pb.h"
#include <list>

namespace Constant
{
	const static int SendTimeLimit = 20;
	struct Player
	{
		const static int InitMoveSpeed = 4;
		const static int InitThrowDistance = 10;
		const static int InitSightRange = 6;
	};
	const static int ItemMoveSpeed = 10;
	const static int TaskRefreshTime = 8000;
	const static int ToolRefreshTime = 10000;
	const static int FoodPointInitRefreshTime = 15000;
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
			const static int Score = -30;
			const static int StunDuration = 500;
			const static int Speed = 15;
		};
		struct Hammer
		{
			const static int StunDuration = 3000;
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
			const static int ExtraMoveSpeed = 3;
		};
		struct TeleScope
		{
			const static int ExtraSightRange = 6;
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
			const static int ExtraMoveSpeed = 5;
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
				const static int Score = -50;
				const static int StunDuration = 1000;
			};
			struct Hammer
			{
				const static int StunDuration = 4000;
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
}
#endif