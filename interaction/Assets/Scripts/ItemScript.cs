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

public class ItemScript : MonoBehaviour
{
    //public GameObject state1Object;
    //public GameObject state2Object;
    //public GameObject state3Object;
    //states can be duplicated if less than 3

    private Vector3 position;//debug
    private long lastReceiveCount;

    private float moveThreshold = 5;
    private float maxMoveSpeed = 5;


    //private Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        //initialize
        position = new Vector3();
        lastReceiveCount = ManagerScript.receiveCount;
    }

    // Update is called once per frame
    void Update()
    {
        //change one's state according to the change of its attributes
        Move();
    }
    void LateUpdate()
    {
        CheckDestroy();
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

    void ChangeState()
    {
        //a large if/else
    }
    void CheckDestroy()
    {
        if (lastReceiveCount < ManagerScript.receiveCount - 1)
        {
            Destroy(gameObject);
        }
    }

    //if DecodeMessage() 
    public void DecodeMessage(GameObjectMessage obj)
    {
        position = PositionMessageToVector3((float)obj.PositionX, (float)obj.PositionY);
        lastReceiveCount = ManagerScript.receiveCount;
    }
}