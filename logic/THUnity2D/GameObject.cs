using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class GameObject
    {
        public const double MinSpeed = 0.0001;

        //public double GlueExtraMoveSpeed = 0;
        //public bool IsTrigger = false;

        private readonly Object privateLock = new Object();
        public readonly Object publicLock = new Object();

        private static Int64 currentMaxID = 0;
        public readonly Int64 ID;


        //Parent
        protected delegate void ParentDeleteHandler();
        protected event ParentDeleteHandler? OnParentDelete;
        private void DeleteParent()
        {
            this._parent.OnChildrenDelete(this);
            this._parent = null;
            if (Velocity.length > 0)
                Velocity = new Vector(Velocity.angle, 0);
            OnParentDelete?.Invoke();
        }
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
                lock (privateLock)
                {
                    if (value != null && this._parent != null)
                    {
                        Debug(this, "Reset Parent to : " + value.ID);
                        DeleteParent();
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
                        DeleteParent();
                    }
                }
            }
        }
        //Parent End

        //Children
        protected HashSet<GameObject> _childrenGameObjectList = new HashSet<GameObject>();
        public HashSet<GameObject> ChildrenGameObjectList { get => _childrenGameObjectList; }
        protected virtual void OnChildrenAdded(GameObject childrenObject)
        {
            lock (privateLock)
            {
                this.ChildrenGameObjectList.Add(childrenObject);
                childrenObject.OnPositionChanged += this.OnChildrenPositionChanged;
                childrenObject.OnMove += this.OnChildrenMove;
                childrenObject.FrameRate = this.FrameRate;
            }
        }
        protected virtual void OnChildrenDelete(GameObject childrenObject)
        {
            lock (privateLock)
            {
                this.ChildrenGameObjectList.Remove(childrenObject);
                childrenObject.OnPositionChanged -= this.OnChildrenPositionChanged;
                childrenObject.OnMove -= this.OnChildrenMove;
            }
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
                lock (privateLock)
                {
                    Debug(this, "Prompt to change position to : " + value.ToString());
                    PositionChanged(_position, value);
                    Debug(this, "change position to : " + this._position.ToString());
                }
            }
        }
        public class PositionChangedEventArgs : EventArgs
        {
            public readonly XYPosition previousPosition;
            public readonly XYPosition position;
            public PositionChangedEventArgs(XYPosition previousPosition, XYPosition position)
            {
                this.previousPosition = previousPosition;
                this.position = position;
            }
        }

        internal delegate void PositionChangedHandler(GameObject sender, XYPosition previousPosition, XYPosition targetPosition);
        internal event PositionChangedHandler? OnPositionChanged; // 声明事件
        public delegate void PositionChangeCompleteHandler(GameObject sender);
        public event PositionChangeCompleteHandler? PositionChangeComplete;
        protected virtual void PositionChanged(XYPosition previousPosition, XYPosition targetPosition)
        {
            lock (privateLock)
            {
                _position = targetPosition;
                OnPositionChanged?.Invoke(this, previousPosition, targetPosition);
            }
            PositionChangeComplete?.Invoke(this);
        }
        //Position end

        //Direction
        protected double _facingDirection = 0;
        public double FacingDirection
        {
            get => this._facingDirection;
            set
            {
                int tmp = Tools.Random.Next();
                //Debug(this, "function FacingDirection : Attempt to lock " + tmp);
                lock (privateLock)
                {
                    //Debug(this, "function FacingDirection : Enter lock " + tmp);
                    this._facingDirection = value; OnDirectionChanged(_facingDirection);
                    //Debug(this, "function FacingDirection : Attempt to release " + tmp);
                }
                //Debug(this, "function FacingDirection : Release lock " + tmp);
            }
        }
        public delegate void DirectionChangedHandler(GameObject sender, double direction);
        public event DirectionChangedHandler? DirectionChanged; // 声明事件
        protected virtual void OnDirectionChanged(double direction)
        {
            lock (privateLock)
            {
                if (DirectionChanged != null)
                {
                    DirectionChanged(this, direction); // 调用所有注册对象的方法
                }
            }
        }
        //direction end


        //Velocity
        protected double _frameRate = 30;
        public double FrameRate
        {
            get => _frameRate;
            set
            {
                lock (privateLock)
                {
                    if (value < 1)//限制帧率最小为每秒一帧
                        this._frameRate = 1;
                    else if (value > 100)//限制帧率最大为每秒100帧
                        this._frameRate = 100;
                    else
                        this._frameRate = value;
                    foreach (var item in ChildrenGameObjectList)
                        item.FrameRate = this._frameRate;
                    return;
                }
            }
        }
        bool canMove = false;
        System.Threading.Semaphore canMoveSema = new System.Threading.Semaphore(0, 1);
        private System.Threading.Thread? _movingThread;
        protected Vector _velocity = new Vector();
        protected object _velocityLock = new object();
        public Vector Velocity
        {
            get
            {
                if (_movingThread == null)
                {
                    _movingThread = new System.Threading.Thread(
                        () =>
                        {
                            lock (privateLock)
                            {
                                while (!canMove)
                                {
                                    //Console.WriteLine(this);
                                    canMoveSema.WaitOne();
                                    //System.Threading.Thread.Sleep(500);
                                }
                                while (canMove)
                                {
                                    int tmp = Tools.Random.Next();
                                    //Debug(this, "function velocity : Attempt to lock " + tmp);
                                    lock (privateLock)
                                    {
                                        //Debug(this, "function velocity : Enter lock " + tmp);
                                        //Console.Write(Environment.TickCount + ",");
                                        int begin = Environment.TickCount;
                                        Move(_velocity.angle, _velocity.length / _frameRate);
                                        int end = Environment.TickCount;
                                        int delta = end - begin;
                                        //Console.Write(delta + ",");
                                        //Console.Write(1.0 / _frameRate + ",");
                                        if (1000.0 / _frameRate > delta)
                                            System.Threading.Thread.Sleep((int)(1000.0 / _frameRate - delta));
                                        //Debug(this, "function velocity : Attempt to release " + tmp);
                                    }
                                    //Debug(this, "function velocity : Release lock " + tmp);
                                }
                            }
                        });
                    _movingThread.Start();
                }
                return _movingTimer;
            }
        }
        protected Vector _velocity = new Vector();
        protected object _velocityLock = new object();
        public Vector Velocity
        {
            get => _velocity;
            set
            {
                lock (_velocityLock)
                {
                    if (value.length < MinSpeed)
                    {
                        _velocity = new Vector(value.angle, 0);
                        canMove = false;
                        return;
                    }
                    this._velocity = value;
                    canMove = true;
                    try
                    {
                        canMoveSema.Release();
                    }
                    catch (System.Threading.SemaphoreFullException)
                    { }
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
        protected int _width = 1;
        public int Width
        {
            get => _width;
            set
            {
                lock (privateLock)
                {
                    if (value < 1)
                        this._width = 1;
                    else
                        this._width = value;
                }
            }
        }
        public class WidthChangedEventArgs : EventArgs
        {
            public readonly int width;
            public WidthChangedEventArgs(int width_t)
            {
                this.width = width_t;
            }
        }
        public delegate void WidthChangedHandler(GameObject sender, WidthChangedEventArgs e);
        public event WidthChangedHandler? WidthChanged; // 声明事件
        protected virtual void OnWidthChanged(WidthChangedEventArgs e)
        {
            if (WidthChanged != null)
            {
                WidthChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //Width end

        //Height
        protected int _height = 1;
        public int Height
        {
            get => _height;
            set
            {
                lock (privateLock)
                {
                    if (value < 1)
                        this._height = 1;
                    else
                        this._height = value;
                }
            }
        }
        //Height end

        //Layer
        protected internal Layer _layer;
        public Layer Layer
        {
            get => _layer;
            set
            {
                lock (privateLock)
                {
                    Debug(this, "Prompt to change layer from " + _layer + " to " + value);
                    LayerChange(_layer, value);
                    Debug(this, "Change layer to " + _layer);
                }
            }
        }
        protected internal delegate void LayerChangeHandler(GameObject sender, Layer previousLayer, Layer targetLayer);
        protected internal event LayerChangeHandler? OnLayerChange;
        protected internal virtual void LayerChange(Layer previousLayer, Layer targetLayer)
        {
            lock (privateLock)
            {
                _layer = targetLayer;
                OnLayerChange?.Invoke(this, previousLayer, targetLayer);
            }
        }
        //Layer

        //Movable
        private bool _movable;
        public bool Movable
        {
            get => this._movable;
            set { lock (privateLock) { this._movable = value; } }
        }
        //Movable end

        public GameObject(GameObject? parent = null)
        {
            ID = currentMaxID;
            currentMaxID++;
            this.Parent = parent;
            Debug(this, "has been newed . ");
        }
        public GameObject(XYPosition position, GameObject? parent = null) : this(parent)
        {
            this.Position = position;
        }

        //Move
        public delegate void MoveHandler(GameObject sender, double angle, double distance, XYPosition previousPosition);
        public event MoveHandler? OnMove;
        public delegate void MoveCompleteHandler(GameObject sender);
        public event MoveCompleteHandler? MoveComplete;
        public delegate void MoveStartHandler(GameObject sender);
        public event MoveStartHandler? MoveStart;
        protected DateTime lastMoveTime = DateTime.MinValue;
        public virtual void Move(double angle, double distance)
        {
            angle = CorrectAngle(angle);
            if (double.IsNaN(distance))
                distance = 0;
            if (distance == 0)
                return;
            int tmp = Tools.Random.Next();
            //Debug(this, "function Move : Attempt to lock " + tmp);
            lock (privateLock)
            {
                //Debug(this, "function Move : Enter lock " + tmp);
                if (!this._movable)
                    return;
                if ((DateTime.Now - lastMoveTime).TotalSeconds < 0.7 / _frameRate)
                    return;
                lastMoveTime = DateTime.Now;
                MoveStart?.Invoke(this);
                XYPosition previousPosition = new XYPosition(Math.Round(_position.x, 6), Math.Round(_position.y, 6));
                _position = previousPosition + new XYPosition(distance * Math.Cos(angle), distance * Math.Sin(angle));
                Debug(this, "Move from " + previousPosition.ToString() + " angle : " + angle + " distance : " + distance + " aim : " + _position.ToString());
                OnMove?.Invoke(this, angle, distance, previousPosition);
                Debug(this, "Move result poition : " + this._position.ToString());
                MoveComplete?.Invoke(this);
                //Debug(this, "function Move : Attempt to release " + tmp);
            }
            //Debug(this, "function Move : Release lock " + tmp);
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
        private object collisionLock = new object();
        public delegate void CollisionHandler(Direction collisionDirection, HashSet<GameObject>? collisionGameObjects);
        public event CollisionHandler? OnCollision;
        protected internal void Collide(Direction collisionDirection, HashSet<GameObject>? collisionGameObjects)
        {
            int tmp = Tools.Random.Next();
            //Debug(this, "function Collide : Attempt to lock " + tmp);
            lock (collisionLock)
            {
                //Debug(this, "function Collide : Enter lock " + tmp);
                DebugWithoutEndline(this, "Collide with : ");
                if (collisionGameObjects != null)
                    foreach (var gameObject in collisionGameObjects)
                    {
                        DebugWithoutIDEndline(this, gameObject.ID + "  ");
                    }
                DebugWithoutID(this, " Direction : " + collisionDirection);
                OnCollision?.Invoke(collisionDirection, collisionGameObjects);
                //Debug(this, "function Collide : Attempt to release " + tmp);
            }
            //Debug(this, "function Collide : Release lock " + tmp);
        }
        //Collision

        //Trigger
        private object triggerLock = new object();
        public delegate void TriggerHandler(HashSet<GameObject> triggerGameObjects);
        public event TriggerHandler? OnTrigger;
        protected internal void Trigger(HashSet<GameObject> triggerGameObjects)
        {
            if (triggerGameObjects == null || triggerGameObjects.Count == 0)
                return;
            int tmp = Tools.Random.Next();
            //Debug(this, "function Trigger : Attempt to lock " + tmp);
            lock (triggerLock)
            {
                //Debug(this, "function Trigger : Enter lock " + tmp);
                DebugWithoutEndline(this, "Trigger with : ");
                foreach (var gameObject in triggerGameObjects)
                {
                    DebugWithoutIDEndline(this, gameObject.ID + "  ");
                }
                DebugWithoutID(this, "");
                OnTrigger?.Invoke(triggerGameObjects);
                //Debug(this, "function Trigger : Attempt to release " + tmp);
            }
            //Debug(this, "function Trigger : Release lock " + tmp);
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
            foreach (var child in ChildrenGameObjectList)
            {
                Console.WriteLine(child.GetType() + " : " + child.ID + " (" + child.Position.x + "," + child.Position.y + ")");
            }
            Console.WriteLine("==============================================");
        }
    }
}
