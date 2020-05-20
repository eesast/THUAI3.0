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
using System.IO;
using Newtonsoft.Json;
using GameObjectMessage = Communication.Proto.GameObject;
using GameObject = UnityEngine.GameObject;

public class ManagerScript : MonoBehaviour
{
    public GameObject defaultCharacter;
    public GameObject runner;
    public GameObject strongMan;
    public GameObject technician;
    public GameObject cook;
    public GameObject luckBoy;

    public CameraFollow cameraFollow;
    public ItemManagerScript itemManager;

    public UserInterface userInterface;

    //Platform Prefab
    public GameObject platform;

    public static long receiveCount;

    private const string configPath = "ClientConfig.json";
    private string playbackPath = "";
    private Communication.CAPI.API clientCommunication;
    private int teamId;
    private long playerId;
    private bool isGameStarted;
    private System.Random rand;
    private int port;
    private PlaybackReader playbackReader;
    private ConcurrentDictionary<long, bool> isCharactersExisted;
    private ConcurrentDictionary<long, bool> isItemsExisted;
    private ConcurrentDictionary<long, bool> isPlatformsExisted;
    private ConcurrentDictionary<long, CharacterScript> characters;
    private ConcurrentDictionary<long, ItemScript> items;
    private ConcurrentDictionary<long, PlatformScript> platforms;
    private ConcurrentQueue<KeyValuePair<long, GameObjectMessage>> instantiateTaskQueue;
    private MessageToServer messageToServer;
    bool isNewMessageToServer;
    private System.Threading.Timer playbackTimer;
    private string taskString;
    private int playMode;
    private int debugLevel;
    private object playbackLock = new object();
    private MessageToClient messageToClient;//use in playback mode
    // Start is called before the first frame update
    void Start()
    {
        //initialize parameters
        Debug.Log("开始初始化参数");
        port = -1;
        playMode = 0;
        debugLevel = 0;
        if(!File.Exists(configPath))
        {
            Debug.LogError("路径ClientConfig.json不存在");
            Application.Quit();
        }
        FileStream fs = new FileStream(configPath, FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        string str = "";
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            str += line;
        }
        ClientConfiguration clientConfig = JsonConvert.DeserializeObject<ClientConfiguration>(str);
        port = clientConfig.agentPort;
        playMode = clientConfig.playMode;
        playbackPath = clientConfig.playbackPath;
        if (playMode==0)
        {
            if (port == -1)
            {
                Debug.LogError("端口读取失败");
                Application.Quit();
            }
            else
            {
                Debug.Log("读取端口号：" + port.ToString());
            }
        }
        else if(playMode==1)
        {
            if(playbackPath=="")
            {
                Debug.LogError("回放路径为空");
                Application.Quit();
            }
            else
            {
                Debug.Log("读取回放路径：" + playbackPath);
                if(!Directory.Exists(playbackPath))
                {
                    Debug.LogError("路径不存在");
                }
            }
        }
        debugLevel = clientConfig.debugLevel;
        receiveCount = 0;
        isGameStarted = false;
        playerId = -1;
        teamId = -1;
        characters = null;
        items = null;
        platforms = null;
        characters = new ConcurrentDictionary<long, CharacterScript>();
        items = new ConcurrentDictionary<long, ItemScript>();
        platforms = new ConcurrentDictionary<long, PlatformScript>();
        isCharactersExisted = new ConcurrentDictionary<long, bool>();
        isPlatformsExisted = new ConcurrentDictionary<long, bool>();
        isItemsExisted = new ConcurrentDictionary<long, bool>();
        instantiateTaskQueue = new ConcurrentQueue<KeyValuePair<long, GameObjectMessage>>();
        messageToServer = new MessageToServer();
        isNewMessageToServer = false;
        rand = new System.Random(System.DateTime.Now.Millisecond);
        Debug.Log("参数初始化完成");

        cameraFollow.playMode = playMode;
        switch(playMode)
        {
            case 0:
                Debug.Log("当前为实时对战模式");
                InitCommunication();
                userInterface.playMode = 0;
                break;
            case 1:
                Debug.Log("当前为回放模式");
                userInterface.playMode = 1;
                InitPlayback();
                break;
            default:
                Debug.Log("请检查playMode参数是否正确");
                Application.Quit();
                break;
        }
        
    }
    public void ChangeTimer(int interval)
    {
        if(playMode!=1)
        {
            return;
        }
        else
        {
            if(interval>0)
            {
                playbackTimer.Change(0, interval);
            }
            else
            {
                playbackTimer.Change(-1, -1);
            }
        }

    }
    
    void InitCommunication()
    {
        //initialize communication
        //communication initialization should be after parameter initialization
        Debug.Log("开始初始化通信");
        clientCommunication = new Communication.CAPI.API();
        //disable communication log
        if (debugLevel == 0)
        {
            Communication.Proto.Constants.Debug = new Communication.Proto.Constants.DebugFunc((_str) => { });
        }
        clientCommunication.Initialize();
        clientCommunication.ReceiveMessage += OnReceive;
        clientCommunication.ConnectServer(new IPEndPoint(IPAddress.Loopback, port));
        Debug.Log("通信初始化完成");
    }

    void InitPlayback()
    {
        Debug.Log("开始初始化回放");
        receiveCount = 1;
        playbackReader = new PlaybackReader(playbackPath);
        ReadPlayback();
        userInterface.change = ChangeTimer;
    }

    void ReadPlayback()
    {
        // 新建一个timer，每隔固定时间读一段消息
        IMessage<MessageToClient> message;
        playbackTimer = new System.Threading.Timer(
            (o) =>
            {
                lock (playbackLock)
                {
                    message = new MessageToClient();
                    if (playbackReader.Read(ref message))
                    {
                        OnReceive(message);
                    }
                    else
                    {
                        Debug.Log("回放结束");
                        playbackTimer.Dispose();
                    }
                }
            }, null, 0, 50);
    }

    void FixedUpdate()
    {
        // don't need input in playback mode
        if (playMode == 0)
        {
            if (isGameStarted)
            {
                GenerateMessage();

                if (isNewMessageToServer)
                {
                    //if (messageToServer.CommandType != CommandType.Move)
                    //{
                    //    Debug.Log("CommandType: " + messageToServer.CommandType.ToString());
                    //}
                    isNewMessageToServer = false;
                    //Debug.Log($"send{messageToServer.CommandType.ToString()},{messageToServer.MoveDirection.ToString()}");
                    clientCommunication.SendMessage(messageToServer);
                }
            }
        }
    }
    void Update()
    {
        if (!instantiateTaskQueue.IsEmpty)
        {
            FlushInstantiateTaskQueue();
        }
    }

    void GenerateMessage()
    {
        CheckKeyInput(out Direction direction, out bool isMoving, out bool isFPressed, out bool isRPressed, out bool isTPressed, out int dist, out bool isUPressed, out bool isIPressed);
        //if...

        if (isMoving)//move
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Move;
            messageToServer.MoveDirection = (Direction)direction;
            messageToServer.MoveDuration = 100;
        }
        else if (isFPressed)//pick dish / tool
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Pick;
            messageToServer.IsPickSelfPosition = true;
            //messageToServer.PickDishOrToolType = -1;
        }
        else if (isRPressed)//throw dish
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Put;
            messageToServer.ThrowDistance = dist;
            messageToServer.IsThrowDish = true;
        }
        else if (isTPressed)//throw tool
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Put;
            messageToServer.ThrowDistance = dist;
            messageToServer.IsThrowDish = false;
        }
        else if (isUPressed)//use tool
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Use;
            messageToServer.UseType = 1;
        }
        else if (isIPressed)//use cooker
        {
            isNewMessageToServer = true;
            messageToServer.CommandType = CommandType.Use;
            messageToServer.UseType = 0;
        }
    }

    void CheckKeyInput(out Direction direction, out bool isMoving, out bool isFPressed, out bool isRPressed, out bool isTPressed, out int dist, out bool isUPressed, out bool isIPressed)
    {
        //check move
        isMoving = true;
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.A))
                direction = Direction.LeftUp;
            else if (Input.GetKey(KeyCode.D))
                direction = Direction.RightUp;
            else
                direction = Direction.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.A))
                direction = Direction.LeftDown;
            else if (Input.GetKey(KeyCode.D))
                direction = Direction.RightDown;
            else
                direction = Direction.Down;
        }
        else if (Input.GetKey(KeyCode.A))
            direction = Direction.Left;
        else if (Input.GetKey(KeyCode.D))
            direction = Direction.Right;
        else
        {
            isMoving = false;
            direction = Direction.Up;//not necessary
        }
        //check cmd
        isFPressed = Input.GetKey(KeyCode.F);
        isRPressed = Input.GetKey(KeyCode.R);
        isTPressed = Input.GetKey(KeyCode.T);
        isUPressed = Input.GetKey(KeyCode.U);
        isIPressed = Input.GetKey(KeyCode.I);
        dist = 0;//means don't throw
        if (isRPressed || isTPressed)
        {
            if (Input.GetKey(KeyCode.Alpha1))
                dist = 1;
            else if (Input.GetKey(KeyCode.Alpha2))
                dist = 2;
            else if (Input.GetKey(KeyCode.Alpha3))
                dist = 3;
            else if (Input.GetKey(KeyCode.Alpha4))
                dist = 4;
            else
                dist = 0;
        }

        //Debug.Log($"【键盘输入】direction:{direction.ToString()},isMoving:{isMoving.ToString()}");
    }

    GameObject RandomCharacter()
    {
        int i = rand.Next() % 6;
        GameObject retVal;
        switch(i)
        {
            case 0:
                retVal = defaultCharacter;
                break;
            case 1:
                retVal = cook;
                break;
            case 2:
                retVal = luckBoy;
                break;
            case 3:
                retVal = runner;
                break;
            case 4:
                retVal = strongMan;
                break;
            case 5:
                retVal = technician;
                break;
            default:
                retVal = defaultCharacter;
                break;
        }
        return retVal;
    }

    public void OnReceive(IMessage message)
    {
        //Debug.Log($"【收到消息】receiveCount:{receiveCount}");
        //process and distribute message
        //change attributes of CharacterScript and ObjectScript instances
        MessageToClient msg = message as MessageToClient;

        //initialize playerId after receiving the first message
        if (receiveCount == 0)
        {
            //contains only one obj
            foreach (var obj in msg.GameObjectList)
            {
                this.playerId = obj.Key;
                messageToServer.ID = this.playerId;
                teamId = obj.Value.Team;
            }
            receiveCount++;
            return;
        }

        //get playerIds when receiving the second message
        //instantiate characters and some other objects
        if (receiveCount == 1)
        {
            foreach (var obj in msg.GameObjectList)
            {
                _Update(obj.Key, obj.Value, obj.Value.ObjType);
            }
            receiveCount++;
            return;
        }
        //game start when receiving the third message
        if (receiveCount >= 2 && receiveCount <= 10)
        {
            //game start?
            if (!isGameStarted)
            {
                isGameStarted = true;
                Debug.Log("游戏开始");
            }
        }
        //code here
        foreach (var obj in msg.GameObjectList)
        {
            _Update(obj.Key, obj.Value, obj.Value.ObjType);
        }
        taskString = "Tasks:";
        foreach (var task in msg.Tasks)
        {
            taskString += " " + task.ToString();
        }
        userInterface.taskString = this.taskString;
        for(int i=0;i<4;i++)
        {
            if (msg.Scores.ContainsKey(i))
                userInterface.scores[i] = msg.Scores[i];
        }
        receiveCount++;
    }

    void OnDestroy()
    {
        //clean up threads
        typeof(Communication.Proto.Constants).Assembly.GetType("Communication.Proto.IDClient").GetMethod("Disconnect").Invoke(
            typeof(Communication.CAPI.API).GetField("client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(clientCommunication), new object[] { }
            );
        UnityEngine.Debug.Log("退出游戏");
    }
    void FlushInstantiateTaskQueue()
    {
        while (!instantiateTaskQueue.IsEmpty)
        {
            bool isSuccessful = instantiateTaskQueue.TryDequeue(out KeyValuePair<long, GameObjectMessage> res);
            if (isSuccessful)
            {
                _Instantiate(res.Key, res.Value);
            }
        }
    }

    //instantiate different types of objects in the game
    //warning: this function can be called in the main thread only
    void _Instantiate(long objKey, GameObjectMessage objValue)
    {
        //Debug.Log("试图创建物体" + objValue.ObjType.ToString()+','+objKey.ToString());
        if (objValue.ObjType == ObjType.People)
        {
            var c = Instantiate(RandomCharacter());
            //let the camera follow the player, not others
            if (objKey == this.playerId)
            {
                cameraFollow.target = c;//-1 when playback, so ok
            }
            if (playMode == 1)
            {
                cameraFollow.charactersQueue.Enqueue(c);
            }
            characters.TryAdd(objKey, c.GetComponent<CharacterScript>());
            characters[objKey].DecodeMessage(objValue);
        }
        else if (objValue.ObjType == ObjType.Block)
        {
            var p = Instantiate(this.platform);
            platforms.TryAdd(objKey, p.GetComponent<PlatformScript>());
            platforms[objKey].DecodeMessage(objValue);
        }
        else
        {
            var i = Instantiate(itemManager.ItemMap(objValue));
            items.TryAdd(objKey, i.GetComponent<ItemScript>());
            items[objKey].DecodeMessage(objValue);
        }
        //Debug.Log("已创建物体" + objValue.ObjType.ToString() + ',' + objKey.ToString());
    }


    //update infos of an object
    void _Update(long objKey, GameObjectMessage objValue, ObjType type)
    {
        if (type == ObjType.People)
        {
            if (characters.ContainsKey(objKey))
            {
                //Debug.Log("更新角色");
                characters[objKey].DecodeMessage(objValue);
                //Debug.Log("更新完成");
            }
            else
            {
                if (!isCharactersExisted.ContainsKey(objKey))
                {
                    isCharactersExisted.TryAdd(objKey, true);
                    instantiateTaskQueue.Enqueue(new KeyValuePair<long, GameObjectMessage>(objKey, objValue));
                }
            }
            //Debug.Log("更新角色"+objKey.ToString());
        }
        else if (objValue.ObjType == ObjType.Block)
        {
            if (platforms.ContainsKey(objKey))
            {
                platforms[objKey].DecodeMessage(objValue);
            }
            else
            {
                if (!isPlatformsExisted.ContainsKey(objKey))
                {
                    isPlatformsExisted.TryAdd(objKey, true);
                    instantiateTaskQueue.Enqueue(new KeyValuePair<long, GameObjectMessage>(objKey, objValue));
                }
            }
        }
        else
        {
            if (items.ContainsKey(objKey))
            {
                items[objKey].DecodeMessage(objValue);
            }
            else
            {
                if (!isItemsExisted.ContainsKey(objKey))
                {
                    isItemsExisted.TryAdd(objKey, true);
                    instantiateTaskQueue.Enqueue(new KeyValuePair<long, GameObjectMessage>(objKey, objValue));
                }
            }
        }
        //Debug.Log("更新物体" + type.ToString());
    }
}