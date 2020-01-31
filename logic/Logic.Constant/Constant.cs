using System;
using System.Collections;
using System.Collections.Generic;
using Communication.Proto;
using Communication.Server;
using THUnity2D;
using static THUnity2D.Tools;
using System.Configuration;
namespace Logic.Constant
{
    public static class Constant
    {
        public const double MoveSpeed = 5;
        public const int FrameRate = 20;
        public const double TimeInterval = 1 / FrameRate;
        public const double MoveDistancePerFrame = MoveSpeed / FrameRate;
        public const char messageSpiltSeperation = ',';
    }
    public enum ObjType
    {
        Air = 0,
        People,
        Block,
        Dish,
        Tools,
        Trigger,
        Size
    }
    public enum BlockType
    {
        Wall,
        Table,
        FoodPoint,
        Cooker,
        RubbishBin,
        TaskPoint,
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
        Size1,
        //以下为菜品

        DarkDish,
        Size2
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