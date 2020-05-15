using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class GameObject
    {
        public const double MinSpeed = 0.0001;

        //public double GlueExtraMoveSpeed = 0;
        //public bool IsTrigger = false;

        //private readonly Object privateLock = new Object();
        //public readonly Object publicLock = new Object();

        private readonly BlockingCollection<Action> Operations = new BlockingCollection<Action>();

        private static Int64 currentMaxID = 0;
        public readonly Int64 ID;


        //Parent
        protected delegate void ParentDeleteHandler();
        protected event ParentDeleteHandler? OnParentDelete;
        protected delegate void ParentAddHandler();
        protected event ParentAddHandler? OnParentAdd;
        private void AddParent(GameObject parent)
        {
            this._parent = parent;
            this._parent.OnChildrenAdded(this);
            OnParentAdd?.Invoke();
        }
        private GameObject? _parent;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                Operations.Add(
                    () =>
                    //lock (privateLock)
                    {
                        if (value != null && this._parent != null)
                        {
                            Debug(this, "Reset Parent to : " + value.ID);

                            this._parent.OnChildrenDelete(this);
                            this._parent = null;
                            if (Velocity.length > 0)
                                Velocity = new Vector(Velocity.angle, 0);
                            OnParentDelete?.Invoke();

                            AddParent(value);
                        }
                        else if (value != null && this._parent == null)
                        {
                            Debug(this, "Set new Parent : " + value.ID);
                            AddParent(value);
                        }
                        else if (value == null && this._parent != null)
                        {
                            Debug(this, "Delete Parent : " + this._parent.ID);


                            this._parent.OnChildrenDelete(this);
                            this._parent = null;
                            if (Velocity.length > 0)
                                Velocity = new Vector(Velocity.angle, 0);
                            OnParentDelete?.Invoke();
                        }
                    });
            }
        }

        //Children
        public ConcurrentDictionary<GameObject, byte> ChildrenGameObjectList { get; } = new ConcurrentDictionary<GameObject, byte>();
        protected virtual void OnChildrenAdded(GameObject childrenObject)
        {
            this.ChildrenGameObjectList.TryAdd(childrenObject, 0);
            childrenObject.OnPositionChanged += this.OnChildrenPositionChanged;
            childrenObject.OnMove += this.OnChildrenMove;
            childrenObject.FrameRate = this.FrameRate;
        }
        protected virtual void OnChildrenDelete(GameObject childrenObject)
        {
            byte tmp = 0; this.ChildrenGameObjectList.TryRemove(childrenObject, out tmp);
            childrenObject.OnPositionChanged -= this.OnChildrenPositionChanged;
            childrenObject.OnMove -= this.OnChildrenMove;
        }
        protected virtual void OnChildrenPositionChanged(GameObject gameObject, XYPosition previousPosition, XYPosition targetPosition)
        { }
        protected virtual void OnChildrenMove(GameObject gameObject, double angle, double distance, XYPosition previousPosition)
        { }
        //Children end

        //Position
        protected internal XYPosition _position = new XYPosition();
        public XYPosition Position
        {
            get => _position;
            set
            {
                Operations.Add(
                    () =>
                    //lock (privateLock)
                    {
                        Debug(this, "Prompt to change position to : " + value.ToString());
                        XYPosition previousPosition = _position;
                        _position = value;
                        OnPositionChanged?.Invoke(this, previousPosition, value);
                        Debug(this, "change position to : " + this._position.ToString());
                        PositionChangeComplete?.Invoke(this);
                    });
            }
        }
        internal delegate void PositionChangedHandler(GameObject sender, XYPosition previousPosition, XYPosition targetPosition);
        internal event PositionChangedHandler? OnPositionChanged; // 声明事件
        public delegate void PositionChangeCompleteHandler(GameObject sender);
        public event PositionChangeCompleteHandler? PositionChangeComplete;
        //Position end

        //Direction
        protected double _facingDirection = 0;
        public double FacingDirection
        {
            get => this._facingDirection;
            set { Operations.Add(() => { this._facingDirection = value; DirectionChanged?.Invoke(this, _facingDirection); }); }
        }
        public delegate void DirectionChangedHandler(GameObject sender, double direction);
        public event DirectionChangedHandler? DirectionChanged; // 声明事件
        //direction end


        //Velocity
        protected double _frameRate = 30;
        public double FrameRate
        {
            get => _frameRate;
            set
            {
                Operations.Add(
                    () =>
                    //lock (privateLock)
                    {
                        if (value < 1)//限制帧率最小为每秒一帧
                            this._frameRate = 1;
                        else if (value > 100)//限制帧率最大为每秒100帧
                            this._frameRate = 100;
                        else
                            this._frameRate = value;
                        foreach (var item in ChildrenGameObjectList.Keys)
                            item.FrameRate = this._frameRate;
                        return;
                    });
            }
        }
        bool canMove = false;
        System.Threading.Semaphore canMoveSema = new System.Threading.Semaphore(0, 1);
        private System.Threading.Thread? _movingThread;
        protected Vector _velocity = new Vector();
        protected object _velocityLock = new object();
        public Vector Velocity
        {
            get => _velocity;
            set
            {
                if (_movingThread == null)
                {
                    _movingThread = new System.Threading.Thread(
                        () =>
                        {
                            while (true)
                            {
                                while (!canMove)
                                {
                                    //Console.WriteLine(this);
                                    canMoveSema.WaitOne();
                                    //System.Threading.Thread.Sleep(500);
                                }
                                while (canMove)
                                {
                                    //lock (privateLock)
                                    //{
                                    //Console.Write(Environment.TickCount + ",");
                                    //int begin = Environment.TickCount;
                                    Move(_velocity.angle, _velocity.length / _frameRate);
                                    //int end = Environment.TickCount;
                                    //int delta = end - begin;
                                    //Console.Write(delta + ",");
                                    //Console.Write(1.0 / _frameRate + ",");
                                    //if (1000.0 / _frameRate > delta)
                                    System.Threading.Thread.Sleep((int)(1000.0 / _frameRate));
                                    //}
                                }
                            }
                        });
                    _movingThread.Start();
                }
                lock (_velocityLock)
                {
                    if (value.length < MinSpeed)
                    {
                        _velocity = new Vector(value.angle, 0);
                        //MovingTimer.Change(-1, -1);
                        canMove = false;
                        return;
                    }
                    //bool isStartMovingTimer = (_velocity.length < MinSpeed) ? true : false;
                    this._velocity = value;
                    //if (isStartMovingTimer)
                    //{
                    //MovingTimer.Change(0, (int)(1000.0 / this.FrameRate));
                    canMove = true;
                    try
                    {
                        canMoveSema.Release();
                    }
                    catch (System.Threading.SemaphoreFullException)
                    { }
                    //}
                }
            }
        }
        //Velocity end

        //GetLength
        //为了提高代码复用性，减少大量X，Y对称的代码，加入此函数。
        public int GetLength(int flag)
        {
            if (flag == 0)
                return _width;
            else
                return _height;
        }

        //Width
        private int _width = 1;
        public int Width
        {
            get => _width;
        }
        //Width end

        //Height
        private int _height = 1;
        public int Height
        {
            get => _height;
        }
        //Height end

        //Layer
        protected internal Layer _layer;
        public Layer Layer
        {
            get => _layer;
            set
            {
                LayerChange(_layer, value);
            }
        }
        protected internal delegate void LayerChangeHandler(GameObject sender, Layer previousLayer, Layer targetLayer);
        protected internal event LayerChangeHandler? OnLayerChange;
        protected internal virtual void LayerChange(Layer previousLayer, Layer targetLayer)
        {
            Operations.Add(
                () =>
                //lock (privateLock)
                {
                    Debug(this, "Prompt to change layer from " + _layer + " to " + targetLayer);
                    _layer = targetLayer;
                    OnLayerChange?.Invoke(this, previousLayer, targetLayer);
                    Debug(this, "Change layer to " + _layer);

                });
        }
        //Layer

        //Movable
        private bool _movable;
        public bool Movable
        {
            get => this._movable;
            set { Operations.Add(() => { this._movable = value; }); }
        }
        //Movable end

        public GameObject(GameObject? parent = null)
        {
            ID = currentMaxID;
            currentMaxID++;
            this.Parent = parent;
            //MovingTimer.Change(0, 0);
            new System.Threading.Thread(
                () =>
                {
                    while (true)
                    {
                        Operations.Take()();
                    }
                }
            )
            { IsBackground = true }.Start();
            Debug(this, "has been newed . ");
        }
        public GameObject(XYPosition position, GameObject? parent = null) : this(parent)
        {
            this.Position = position;
        }
        public GameObject(int width, int height, GameObject? parent = null) : this(parent)
        {
            this._width = width;
            this._height = height;
        }

        //Move
        public delegate void MoveHandler(GameObject sender, double angle, double distance, XYPosition previousPosition);
        internal event MoveHandler? OnMove;
        public delegate void MoveCompleteHandler(GameObject sender);
        public event MoveCompleteHandler? MoveComplete;
        public delegate void MoveStartHandler(GameObject sender);
        public event MoveStartHandler? MoveStart;
        protected int lastMoveTime = 0;
        public virtual void Move(double angle, double distance)
        {
            if (double.IsNaN(angle))
                angle = 0;
            if (double.IsNaN(distance))
                distance = 0;
            if (distance == 0)
                return;
            if ((Environment.TickCount - lastMoveTime) < 800.0 / _frameRate)
                return;
            lastMoveTime = Environment.TickCount;
            Operations.Add(
                () =>
                //lock (privateLock)
                {
                    if (!this._movable)
                        return;
                    MoveStart?.Invoke(this);
                    XYPosition previousPosition = new XYPosition(Math.Round(_position.x, 6), Math.Round(_position.y, 6));
                    _position = previousPosition + new XYPosition(distance * Math.Cos(angle), distance * Math.Sin(angle));
                    Debug(this, "Move from " + previousPosition.ToString() + " angle : " + angle + " distance : " + distance + " aim : " + _position.ToString());
                    OnMove?.Invoke(this, angle, distance, previousPosition);
                    Debug(this, "Move result poition : " + this._position.ToString());
                    MoveComplete?.Invoke(this);
                });
        }
        //Move end

        //Bouncable
        protected bool _bouncable = false;
        public bool Bouncable
        {
            get => _bouncable;
            set
            {
                _bouncable = value;
                Debug(this, "Set Bouncable " + _bouncable);
            }
        }
        //Bouncable

        //Collision
        public delegate void CollisionHandler(Direction collisionDirection, HashSet<GameObject>? collisionGameObjects);
        public event CollisionHandler? OnCollision;
        //private object collisionLock = new object();
        protected internal void Collide(Direction collisionDirection, HashSet<GameObject>? collisionGameObjects)
        {
            //lock (collisionLock)
            Operations.Add(
                () =>
            {
                DebugWithoutEndline(this, "Collide with : ");
                if (collisionGameObjects != null)
                    foreach (var gameObject in collisionGameObjects)
                    {
                        DebugWithoutIDEndline(this, gameObject.ID + "  ");
                    }
                DebugWithoutID(this, " Direction : " + collisionDirection);
                OnCollision?.Invoke(collisionDirection, collisionGameObjects);
            });
        }
        //Collision

        //Trigger
        public delegate void TriggerHandler(HashSet<GameObject> triggerGameObjects);
        public event TriggerHandler? OnTrigger;
        //private object triggerLock = new object();
        protected internal void Trigger(HashSet<GameObject> triggerGameObjects)
        {
            if (triggerGameObjects == null || triggerGameObjects.Count == 0)
                return;
            //Debug(this, "function Trigger : Attempt to enter lock");
            Operations.Add(
                () =>
                //lock (triggerLock)
                {
                    DebugWithoutEndline(this, "Trigger with : ");
                    foreach (var gameObject in triggerGameObjects)
                    {
                        DebugWithoutIDEndline(this, gameObject.ID + "  ");
                    }
                    DebugWithoutID(this, "");
                    OnTrigger?.Invoke(triggerGameObjects);
                });
            //Debug(this, "function Trigger : Release lock");
        }
        //Trigger

        ~GameObject()
        {
            this.Parent = null;
        }

        public static Action<GameObject, string> Debug = (gameObject, str) =>
        {
            Console.WriteLine(gameObject.GetType() + " " + gameObject.ID + " : " + str);
        };
        public static Action<GameObject, string> DebugWithoutEndline = (gameObject, str) =>
        {
            Console.Write(gameObject.GetType() + " " + gameObject.ID + " : " + str + " ");
        };
        public static Action<GameObject, string> DebugWithoutID = (gameObject, str) =>
        {
            Console.WriteLine(str);
        };
        public static Action<GameObject, string> DebugWithoutIDEndline = (gameObject, str) =>
        {
            Console.Write(str);
        };

        public void PrintChildrenList()
        {
            Console.WriteLine("========= " + this.GetType() + " : " + this.ID + " children GameObject list =========");
            foreach (var child in ChildrenGameObjectList.Keys)
            {
                Console.WriteLine(child.GetType() + " : " + child.ID + " (" + child.Position.x + "," + child.Position.y + ")");
            }
            Console.WriteLine("==============================================");
        }
    }
}
