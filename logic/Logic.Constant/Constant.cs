using System;
using System.Collections;
using System.Collections.Generic;
using Communication.Proto;
using Communication.Server;
using THUnity2D;
using static THUnity2D.Tools;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
namespace Logic.Constant
{
    public static class Constant
    {
        public const double MoveSpeed = 5;
        public const double FrameRate = 20;
        public const double TimeInterval = 1 / FrameRate;
        public const double MoveDistancePerFrame = MoveSpeed / FrameRate;

        private static JObject _configs;
        public static JObject Configs
        {
            get
            {
                _configs = _configs ?? (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(@"Config\Config.json")));
                return _configs;
            }
        }
    }
    public enum ObjType
    {
        People,
        Block,
        Dish,
        Tools,
        Trigger,
        Size
    }
    public enum BlockType
    {//标1的物品扔出碰到会反弹，标0的会穿过去
        Wall,//1
        Table,//0
        FoodPoint,//1
        Cooker,//0
        RubbishBin,//0
        TaskPoint,//1
        Size
    }
    public enum DishType
    {
        Empty = 0,//空
                  //以下为食材
        Apple,
        Banana,
        Potato,
        Tomato,
        Egg,
        Flour,
        Chicken,
        Pork,
        Beef,
        Honey,
        Butter,
        Romaine,
        Size1,
        //以下为菜品

        Bread,
        BasicHamburger,
        GoodHamburger,
        ApplePie,
        BananaPie,
        TomatoEgg,
        MashedPotato,
        Size2,
        //以下为垃圾

        OverCookedDish,
        DarkDish,
        Size3
    }
    public enum ToolType
    {
        Empty = 0,
        TigerShoes,//虎头鞋
        SpeedBuff,//极速buff
        StrenthBuff,//力量buff
        TeleScope,//望远镜
        Condiment,//调料
        Fertilizer,//肥料
        BreastPlate,//护心镜
        SpaceGate,//传送门
        Eye,//眼

        WaveGlue,//胶水
        LandMine,//地雷
        Trap,//陷阱
        FlashBomb,//闪光弹
        Hammer,//锤子
        Brick,//砖头
        Stealer,//分数偷取

        Size
    }
    public enum TriggerType
    {
        WaveGlue,
        Trap,
        Mine,
        Size
    }
    public enum CommandType
    {
        Move = 0,
        Pick,
        Put,
        Use,
        Stop,
        Speak,
        Size
    }

    public enum TALENT
    {
        None,
        Run,
        Strenth,
        Cook,
        Technology,
        Luck,
        Bag,
        Drunk
    }
}