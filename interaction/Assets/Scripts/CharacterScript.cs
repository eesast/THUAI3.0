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

using GameObjectMessage = Communication.Proto.GameObject;
using GameObject = UnityEngine.GameObject;

public class CharacterScript : MonoBehaviour
{
    public float turnSpeed = 20f;
    public float maxMoveSpeed;

    public ItemManagerScript itemManager;

    public GameObject dishPack;
    public GameObject toolPack;

    public GameObject teamSign;

    public Vector3 position;//debug
    private Vector3 direction;
    private bool isMoving;
    private Animator animator;
    private long lastReceiveCount;
    private float moveThreshold = 5;
    //private Transform transform;
    private DishType dishType;
    private ToolType toolType;
    private DishType oldDishType;
    private ToolType oldToolType;
    private GameObject dish;
    private GameObject tool;
    private GameObjectMessage msg;
    private Vector3 scale;
    private int teamId=-1;
    private Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };
    bool isTeamSignShowed=false;

    // Start is called before the first frame update
    void Start()
    {
        //initialize
        animator = GetComponentInParent<Animator>();
        position = new Vector3();
        direction = new Vector3();
        isMoving = false;
        lastReceiveCount = ManagerScript.receiveCount;
        dish = tool = null;
        dishType = oldDishType = DishType.DishEmpty;
        toolType = oldToolType = ToolType.ToolEmpty;
        msg = new GameObjectMessage();
        scale = new Vector3(0.4f, 0.4f, 0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        //change one's state according to the change of its attributes
        //Debug.Log(position.ToString());
        Move();
        Rotate();
        SetAnimator();
        UpdatePack();
        if (!isTeamSignShowed)
        {
            if (teamId >= 0)
            {
                teamSign.GetComponent<Renderer>().material.color = colors[teamId];
                isTeamSignShowed = true;
            }
        }
    }

    void LateUpdate()
    {
        CheckDestroy();
    }

    void UpdatePack()
    {
        //update dish
        if (dishType == DishType.DishEmpty)
        {
            if (oldDishType != DishType.DishEmpty)
            {
                DestroyItem(true);
            }
        }
        else
        {
            if (oldDishType == DishType.DishEmpty)
            {
                InstantiateItem(true,(int)dishType);
            }
            else if (oldDishType != dishType)
            {
                DestroyItem(true);
                InstantiateItem(true, (int)dishType);
            }
        }
        //update tool
        if (toolType == ToolType.ToolEmpty)
        {
            if (oldToolType != ToolType.ToolEmpty)
            {
                DestroyItem(false);
            }
        }
        else
        {
            if (oldToolType == ToolType.ToolEmpty)
            {
                InstantiateItem(false, (int)toolType);
            }
            else if (oldToolType != toolType)
            {
                DestroyItem(false);
                InstantiateItem(true, (int)toolType);
            }
        }
    }

    void DestroyItem(bool isDish)
    {
        if(isDish)
        {
            Destroy(dish);
            dishPack.GetComponent<PackScript>().SetActive(false);
            dish = null;
            oldDishType = DishType.DishEmpty;
        }
        else
        {
            Destroy(tool);
            toolPack.GetComponent<PackScript>().SetActive(false);
            tool = null;
            oldToolType = ToolType.ToolEmpty;
        }
    }

    void InstantiateItem(bool isDish,int type)
    {
        if(isDish)
        {
            msg.ObjType = ObjType.Dish;
            msg.DishType = (DishType)type;
            dish = Instantiate(itemManager.ItemMap(msg), dishPack.transform,false);
            dishPack.GetComponent<PackScript>().SetActive(true);
            dish.GetComponent<ItemScript>().enabled = false;
            dish.transform.localScale = scale;
            oldDishType = (DishType)type;
        }
        else
        {
            msg.ObjType = ObjType.Tool;
            msg.ToolType = (ToolType)type;
            tool = Instantiate(itemManager.ItemMap(msg), toolPack.transform, false);
            toolPack.GetComponent<PackScript>().SetActive(true);
            tool.GetComponent<ItemScript>().enabled = false;
            tool.transform.localScale = scale;
            oldToolType = (ToolType)type;
        }
    }

    void Move()
    {
        if ((position - transform.position).magnitude > moveThreshold)
        {
            transform.position = position;
        }
        else
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, position.x, maxMoveSpeed * Time.deltaTime);
            pos.y = Mathf.Lerp(pos.y, position.y, maxMoveSpeed * Time.deltaTime);
            pos.z = Mathf.Lerp(pos.z, position.z, maxMoveSpeed * Time.deltaTime);
            transform.position = pos;
        }
    }
    void Rotate()
    {
        //smart rotate
        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(desiredForward);
    }

    void SetAnimator()
    {
        //set animator isMoving true/false
        animator.SetBool("IsWalking", isMoving);
    }

    void CheckDestroy()
    {
        if(lastReceiveCount<ManagerScript.receiveCount-1)
        {
            Destroy(gameObject);
        }
    }


    public void DecodeMessage(GameObjectMessage obj)
    {
        if(teamId<0)
        {
            teamId = obj.Team;
        }
        //Debug.Log("-----decode-----");
        position = PositionMessageToVector3((float)obj.PositionX,(float)obj.PositionY);
        //Debug.Log($"*****xposition change to:{position.x}");
        direction = DirectionMessageToVector3(obj.Direction);
        //z?float
        isMoving = obj.IsMoving;
        dishType = obj.DishType;
        toolType = obj.ToolType;
        lastReceiveCount = ManagerScript.receiveCount;
    }
}


