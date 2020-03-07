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
    {//不确定英文怎么说的直接用了拼音
        Empty = 0,//空
        //以下为食材
        Wheat,//麦子
        Rice,//水稻
        Tomato,//番茄
        Egg,//鸡蛋
        Milk,//牛奶
        Apple,//苹果
        Banana,//香蕉
        DarkPlum,//乌梅
        Hawthorn,//山楂
        Strawberry,//草莓
        Beef,//牛肉
        Pork,//猪肉
        ChickenMeat,//鸡肉
        Potato,//土豆
        Lettuce,//生菜
        Agaric,//木耳
        NeedleMushroom,//金针菇
        Cabbage,//包菜
        Size1,
        //以下为中间产物
        Flour,//面粉
        Noodle,//面条
        Bread,//面包片
        CookedRice,//米饭
        Ketchup,//番茄酱
        Cream,//奶油
        //以下为菜品
        TomatoFriedEgg,//番茄炒蛋
        TomatoFriedEggNoodle,//西红柿鸡蛋面
        BeefNoodle,//清青牛拉
        OverRice,//盖浇饭
        HuangMenJi,//黄焖鸡米饭
        Barbecue,//烤肉
        FrenchFries,//薯条
        PlumJuice,//酸梅汤
        Hamburger,//汉堡
        StrawberryIcecream,//草莓圣代
        PopcornChicken,//鸡米花
        AgaricFriedEgg,//木耳炒蛋
        Cake,//蛋糕
        BingTangHuLu,//冰糖葫芦
        FruitSalad,//水果色拉
        XiangGuo,
        XiangGuo_3,
        XiangGuo_4,
        XiangGuo_5,
        XiangGuo_6,
        XiangGuo_7,
        XiangGuo_8,//香锅
        Size2,
        //以下为垃圾
        OverCookedDish,
        DarkDish,//黑暗料理
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
        //Eye,//眼

        WaveGlue,//胶水
        LandMine,//地雷
        Trap,//陷阱
        //FlashBomb,//闪光弹
        //Hammer,//锤子
        //Brick,//砖头
        //Stealer,//分数偷取

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
        Runner,
        StrongMan,
        Cook,
        Technician,
        LuckyBoy,
        //DrunkMan
    }
}