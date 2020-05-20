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
using THUnity2D;
using Direction = Communication.Proto.Direction;


using GameObjectMessage = Communication.Proto.GameObject;
using GameObject = UnityEngine.GameObject;

public class Tools
{
    public static float deltaX = 25;
    public static float deltaZ = 25;

    static public Vector3 DirectionMessageToVector3(Direction direction)
    {
        Vector3 retVal;
        switch (direction)
        {
            case Direction.Down:
                retVal = new Vector3(0, 0, -1);
                break;
            case Direction.Up:
                retVal = new Vector3(0, 0, 1);
                break;
            case Direction.Left:
                retVal = new Vector3(-1, 0, 0);
                break;
            case Direction.Right:
                retVal = new Vector3(1, 0, 0);
                break;
            case Direction.LeftDown:
                retVal = new Vector3(-1, 0, -1).normalized;
                break;
            case Direction.LeftUp:
                retVal = new Vector3(-1, 0, 1).normalized;
                break;
            case Direction.RightDown:
                retVal = new Vector3(1, 0, -1).normalized;
                break;
            case Direction.RightUp:
                retVal = new Vector3(1, 0, 1).normalized;
                break;
            default:
                retVal = new Vector3();
                break;
        }
        return retVal;
    }

    static public Vector3 PositionMessageToVector3(float x, float y)
    {
        Vector3 retVal;
        retVal.x = (float)x - deltaX;
        retVal.y = 0;
        retVal.z = (float)y - deltaZ;
        return retVal;
    }
}