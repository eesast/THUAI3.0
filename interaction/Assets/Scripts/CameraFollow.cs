using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

public class CameraFollow : MonoBehaviour
{

    public GameObject target;
    public ConcurrentQueue<GameObject> charactersQueue;//playback only
    public Vector3 offset;
    public float distanceFollow;//follow mode
    public float distanceFree;//free mode
    public float speed;
    public int playMode;
    public GameObject freeViewer;
    public float dragSpeed;

    private Vector3 pos;
    private float moveThreshold = 20;
    private GameObject[] characters;
    private int characterNum;
    private bool isFreeView = false;
    private float distance;
    private Vector3 mouseStart;
    private Vector3 mouseMove;

    void Awake()
    {
        playMode = 0;
        target = null;
        charactersQueue = new ConcurrentQueue<GameObject>();
        characters = new GameObject[8];
        characterNum = 0;
        offset = new Vector3(0, 1, -1);
        distanceFollow = 10f;
        distanceFree = 20f;
        dragSpeed = 0.1f;
    }

    void Start()
    {
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //switch view in playback mode
        if (playMode == 1)
        {
            while (!charactersQueue.IsEmpty)
            {
                bool isSuccessful = charactersQueue.TryDequeue(out GameObject c);
                if (isSuccessful)
                {
                    characters[characterNum] = c;
                    characterNum += 1;
                }
            }
            if (!isFreeView)
            {
                if (target == null)
                {
                    if (characters[0] != null)
                    {
                        target = characters[0];
                    }
                }
                else
                {
                    GetKeyInput(out int id);
                    if (id >= 0 && id < characterNum)
                    {
                        target = characters[id];
                    }
                }

                //adjust distance
                distanceFollow -= Input.GetAxis("Mouse ScrollWheel") * 5;
                distanceFollow = Mathf.Clamp(distanceFollow, 5, 40);

                //switch mode
                if (Input.GetKey(KeyCode.Y))
                {
                    isFreeView = !isFreeView;
                    target = freeViewer;
                }
            }
            else
            {
                //move 
                if (Input.GetMouseButtonDown(0))
                {
                    mouseStart = new Vector3(Input.mousePosition.x, 0,Input.mousePosition.y);
                }
                else if (Input.GetMouseButton(0))
                {
                    mouseMove = new Vector3((Input.mousePosition.x - mouseStart.x)/Screen.width, 0,(Input.mousePosition.y - mouseStart.z)/Screen.height);
                    mouseStart = new Vector3(Input.mousePosition.x,0, Input.mousePosition.y);
                    freeViewer.transform.position -= mouseMove*dragSpeed*distanceFree*distanceFree;
                }

                //adjust distance
                distanceFree -= Input.GetAxis("Mouse ScrollWheel") * 5;
                distanceFree = Mathf.Clamp(distanceFree, 5, 40);
                //switch mode
                GetKeyInput(out int id);
                if (id >= 0 && id < characterNum)
                {
                    target = characters[id];
                    isFreeView = !isFreeView;
                }
            }
        }


    }

    void GetKeyInput(out int id)
    {
        id = -1;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            id = 0;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            id = 1;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            id = 2;
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            id = 3;
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            id = 4;
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            id = 5;
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            id = 6;
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {
            id = 7;
        }
        else
        {
            id = -1;
        }
    }
    void LateUpdate()
    {
        if(isFreeView)
        {
            distance = distanceFree;
        }
        else
        {
            distance = distanceFollow;
        }
        if (target != null)
        {
            if ((target.transform.position + offset * distance - pos).magnitude > moveThreshold&&(!Input.GetMouseButton(0)))//dont move so fast when dragging
            {
                pos = target.transform.position + offset * distance;
                transform.position = pos;
            }
            else
            {
                //去差值平滑相机跟随
                pos.x = Mathf.Lerp(pos.x, target.transform.position.x + offset.x * distance, speed * Time.deltaTime);
                pos.y = Mathf.Lerp(pos.y, target.transform.position.y + offset.y * distance, speed * Time.deltaTime);
                pos.z = Mathf.Lerp(pos.z, target.transform.position.z + offset.z * distance, speed * Time.deltaTime);
                transform.position = pos;
            }
        }
    }
}
