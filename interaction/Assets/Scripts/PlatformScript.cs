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
using static Tools;

using System.Collections.Concurrent;

using GameObjectMessage = Communication.Proto.GameObject;
using GameObject = UnityEngine.GameObject;
public class PlatformScript : MonoBehaviour
{
    //public GameObject state1Object;
    //public GameObject state2Object;
    //public GameObject state3Object;
    //states can be duplicated if less than 3
    public ItemManagerScript itemManager;
    public float foodPointHeight;
    public float cookerHeight;


    private Vector3 position;//debug

    private float moveThreshold = 0.1f;
    private DishType dishType;
    private DishType oldDishType;
    //private Transform transform;
    private GameObject dish;
    private GameObjectMessage msg;
    private Vector3 offset;
    private float radius = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //initialize
        cookerHeight = 2.2f;
        position = new Vector3();
        dish = null;
        dishType = DishType.DishEmpty;
        oldDishType = DishType.DishEmpty;
        msg = new GameObjectMessage
        {
            ObjType = ObjType.Dish
        };
    }

    // Update is called once per frame
    void Update()
    {
        //change one's state according to the change of its attributes
        Move();

    }
    void LateUpdate()
    {
        //CheckDestroy();
        if (dishType == DishType.DishEmpty)
        {
            if (oldDishType != DishType.DishEmpty)
            {
                DestroyDish();
            }
        }
        else
        {
            if (oldDishType == DishType.DishEmpty)
            {
                InstantiateDish();
            }
            else if (oldDishType == dishType)
            {
                UpdateDish();
            }
            else
            {
                DestroyDish();
                InstantiateDish();
            }
        }
    }
    void Move()
    {
        if ((position - transform.position).magnitude > moveThreshold)
        {
            transform.position = position;
        }
    }

    void UpdateDish()
    {
        dish.transform.position = this.transform.position + offset;
        //Debug.Log("update dish called:" + dish.name.ToString() + dish.transform.position.ToString()); ;
    }

    void DestroyDish()
    {
        //Debug.Log("destroy dish called");
        Destroy(dish);
        dish = null;
        oldDishType = DishType.DishEmpty;
    }

    void InstantiateDish()
    {
        //Debug.Log("instantiate dish called");
        msg.DishType = dishType;
        dish = Instantiate(itemManager.ItemMap(msg));
        Vector2 var = radius * Random.insideUnitCircle;
        offset = new Vector3(var.x, 0, var.y);
        //to stop them from destroying themselves!!!
        dish.GetComponent<ItemScript>().enabled = false;
        oldDishType = dishType;
    }


    void ChangeState()
    {
        //a large if/else
    }

    //if DecodeMessage() 
    public void DecodeMessage(GameObjectMessage obj)
    {
        position = PositionMessageToVector3((float)obj.PositionX, (float)obj.PositionY);
        if (obj.BlockType == BlockType.Cooker)
        {
            position.y = cookerHeight;
        }
        dishType = obj.DishType;
    }
}
