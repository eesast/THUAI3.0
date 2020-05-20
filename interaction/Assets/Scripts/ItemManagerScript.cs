using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Constant;
using static Logic.Constant.Constant;
using Communication.Proto;
using Google.Protobuf;
using UnityEngine.UI;
using System.Net;
using static THUnity2D.Tools;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

using GameObjectMessage = Communication.Proto.GameObject;
using GameObject = UnityEngine.GameObject;

public class ItemManagerScript : MonoBehaviour
{
    //Item Prefabs
    //dish
    public GameObject wheat;
    public GameObject rice;
    public GameObject tomato;
    public GameObject egg;
    public GameObject milk;
    public GameObject apple;
    public GameObject banana;
    public GameObject darkPlum;
    public GameObject hawthorn;
    public GameObject strawberry;
    public GameObject beef;
    public GameObject pork;
    public GameObject chickenMeat;
    public GameObject potato;
    public GameObject lettuce;
    public GameObject agaric;
    public GameObject needleMushroom;
    public GameObject cabbage;
    public GameObject flour;
    public GameObject noodle;
    public GameObject bread;
    public GameObject cookedRice;
    public GameObject ketchup;
    public GameObject cream;
    public GameObject tomatoFriedEgg;
    public GameObject tomatoFriedEggNoodle;
    public GameObject beefNoodle;
    public GameObject overRice;
    public GameObject yellowPheasant;
    public GameObject barbecue;
    public GameObject frenchFries;
    public GameObject plumJuice;
    public GameObject hamburger;
    public GameObject strawberryIcecream;
    public GameObject popcornChicken;
    public GameObject agaricFriedEgg;
    public GameObject cake;
    public GameObject sugarCoatedHaws;
    public GameObject fruitSalad;
    public GameObject spicedPot;
    public GameObject darkDish;//or overcooked

    //tool
    public GameObject tigerShoes;
    public GameObject speedBuff;
    public GameObject strengthBuff;
    public GameObject teleScope;
    public GameObject condiment;
    public GameObject fertilizer;
    public GameObject breastPlate;
    public GameObject spaceGate;
    public GameObject waveGlueBottle;
    public GameObject landMine;
    public GameObject trapTool;
    public GameObject flashBomb;
    public GameObject throwHammer;
    public GameObject bow;
    public GameObject stealer;

    private static GameObjectMessage msg;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject ItemMap(GameObjectMessage objValue)
    {
        //Debug.Log(objValue.ObjType.ToString() + ":" + objValue.DishType.ToString());
        GameObject retVal;
        if (objValue.ObjType == ObjType.Dish)
        {
            switch (objValue.DishType)
            {
                //case DishType.Apple:
                //    retVal = apple;
                //    break;
                //case DishType.Banana:
                //    retVal = banana;
                //    break;
                case DishType.Tomato:
                    retVal = tomato;
                    break;
                case DishType.Potato:
                    retVal = potato;
                    break;
                //case DishType.Agaric:
                //    retVal = agaric;
                //    break;
                //case DishType.AgaricFriedEgg:
                //    retVal = agaricFriedEgg;
                //    break;
                case DishType.Barbecue:
                    retVal = barbecue;
                    break;
                case DishType.Beef:
                    retVal = beef;
                    break;
                case DishType.BeefNoodle:
                    retVal = beefNoodle;
                    break;
                case DishType.Bread:
                    retVal = bread;
                    break;
                //case DishType.Cabbage:
                //    retVal = cabbage;
                //    break;
                //case DishType.Cake:
                //    retVal = cake;
                //    break;
                //case DishType.ChickenMeat:
                //    retVal = chickenMeat;
                //    break;
                case DishType.CookedRice:
                    retVal = cookedRice;
                    break;
                //case DishType.Cream:
                //    retVal = cream;
                //    break;
                case DishType.DarkDish:
                    retVal = darkDish;
                    break;
                //case DishType.DarkPlum:
                //    retVal = darkPlum;
                //    break;
                case DishType.Egg:
                    retVal = egg;
                    break;
                case DishType.Flour:
                    retVal = flour;
                    break;
                case DishType.FrenchFries:
                    retVal = frenchFries;
                    break;
                //case DishType.FruitSalad:
                //    retVal = fruitSalad;
                //    break;
                case DishType.Hamburger:
                    retVal = hamburger;
                    break;
                //case DishType.Hawthorn:
                //    retVal = hawthorn;
                //    break;
                case DishType.Ketchup:
                    retVal = ketchup;
                    break;
                case DishType.Lettuce:
                    retVal = lettuce;
                    break;
                //case DishType.Milk:
                //    retVal = milk;
                //    break;
                //case DishType.NeedleMushroom:
                //    retVal = needleMushroom;
                //    break;
                case DishType.Noodle:
                    retVal = noodle;
                    break;
                case DishType.OverCookedDish:
                    retVal = darkDish;
                    break;
                case DishType.OverRice:
                    retVal = overRice;
                    break;
                //case DishType.PlumJuice:
                //    retVal = plumJuice;
                //    break;
                //case DishType.PopcornChicken:
                //    retVal = popcornChicken;
                //    break;
                case DishType.Pork:
                    retVal = pork;
                    break;
                case DishType.Rice:
                    retVal = rice;
                    break;
                case DishType.SpicedPot:
                    retVal = spicedPot;
                    break;
                case DishType.SpicedPot3:
                    retVal = spicedPot;
                    break;
                case DishType.SpicedPot4:
                    retVal = spicedPot;
                    break;
                case DishType.SpicedPot5:
                    retVal = spicedPot;
                    break;
                case DishType.SpicedPot6:
                    retVal = spicedPot;
                    break;
                //case DishType.SpicedPot7:
                //    retVal = spicedPot;
                //    break;
                //case DishType.SpicedPot8:
                //    retVal = spicedPot;
                //    break;
                //case DishType.Strawberry:
                //    retVal = strawberry;
                //    break;
                //case DishType.StrawberryIcecream:
                //    retVal = strawberryIcecream;
                //    break;
                //case DishType.SugarCoatedHaws:
                //    retVal = sugarCoatedHaws;
                //    break;
                case DishType.TomatoFriedEgg:
                    retVal = tomatoFriedEgg;
                    break;
                case DishType.TomatoFriedEggNoodle:
                    retVal = tomatoFriedEggNoodle;
                    break;
                case DishType.Wheat:
                    retVal = wheat;
                    break;
                //case DishType.YellowPheasant:
                //    retVal = yellowPheasant;
                //    break;
                default:
                    retVal = darkDish;
                    break;
            }
        }
        else if (objValue.ObjType == ObjType.Tool)
        {
            switch (objValue.ToolType)
            {
                case ToolType.Bow:
                    retVal = bow;
                    break;
                case ToolType.BreastPlate:
                    retVal = breastPlate;
                    break;
                case ToolType.Condiment:
                    retVal = condiment;
                    break;
                case ToolType.Fertilizer:
                    retVal = fertilizer;
                    break;
                case ToolType.FlashBomb:
                    retVal = flashBomb;
                    break;
                case ToolType.LandMine:
                    retVal = landMine;
                    break;
                case ToolType.SpaceGate:
                    retVal = spaceGate;
                    break;
                case ToolType.SpeedBuff:
                    retVal = speedBuff;
                    break;
                case ToolType.Stealer:
                    retVal = stealer;
                    break;
                case ToolType.StrengthBuff:
                    retVal = strengthBuff;
                    break;
                case ToolType.TeleScope:
                    retVal = teleScope;
                    break;
                case ToolType.ThrowHammer:
                    retVal = throwHammer;
                    break;
                case ToolType.TigerShoes:
                    retVal = tigerShoes;
                    break;
                case ToolType.TrapTool:
                    retVal = trapTool;
                    break;
                case ToolType.WaveGlueBottle:
                    retVal = waveGlueBottle;
                    break;
                default:
                    retVal = darkDish;
                    break;
            }
        }
        else
        {
            retVal = null;
        }
        if (retVal == null)
        {
            retVal = darkDish;
        }
        return retVal;
    }

}
