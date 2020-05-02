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
            set { lock (privateLock) { this._facingDirection = value; OnDirectionChanged(new DirectionChangedEventArgs(this._facingDirection)); } }
        }
        public class DirectionChangedEventArgs : EventArgs
        {
            public readonly double facingDirection;
            public DirectionChangedEventArgs(double facingDirection)
            {
                this.facingDirection = facingDirection;
            }
        }
        public delegate void DirectionChangedHandler(GameObject sender, DirectionChangedEventArgs e);
        public event DirectionChangedHandler? DirectionChanged; // 声明事件
        protected virtual void OnDirectionChanged(DirectionChangedEventArgs e)
        {
            lock (privateLock)
            {
                if (DirectionChanged != null)
                {
                    DirectionChanged(this, e); // 调用所有注册对象的方法
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
        private System.Threading.Timer? _movingTimer;
        protected System.Threading.Timer MovingTimer
        {
            get
            {
                if (_movingTimer == null)
                {
                    _movingTimer = new System.Threading.Timer(
                        (o) =>
                        {
                            lock (privateLock)
                            {
                                Move(_velocity.angle, _velocity.length / _frameRate);
                            }
                        });
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
                        MovingTimer.Change(-1, -1);
                        return;
                    }
                    bool isStartMovingTimer = (_velocity.length < MinSpeed) ? true : false;
                    this._velocity = value;
                    if (isStartMovingTimer)
                        MovingTimer.Change(0, (int)(1000.0 / this.FrameRate));
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
            lock (privateLock)
            {
                if (!this._movable)
                    return;
                if ((DateTime.Now - lastMoveTime).TotalSeconds < 0.7 / _frameRate)
                    return;
                lastMoveTime = DateTime.Now;
                MoveStart?.Invoke(this);
                XYPosition previousPosition = _position;
                _position = _position + new XYPosition(distance * Math.Cos(angle), distance * Math.Sin(angle));
                Debug(this, "Move from " + previousPosition.ToString() + " angle : " + angle + " distance : " + distance + " aim : " + _position.ToString());
                OnMove?.Invoke(this, angle, distance, previousPosition);
                Debug(this, "Move result poition : " + this._position.ToString());
                MoveComplete?.Invoke(this);
            }
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
        protected internal void Collide(Direction collisionDirection, HashSet<GameObject>? collisionGameObjects)
        {
            lock (privateLock)
            {
                DebugWithoutEndline(this, "Collide with : ");
                if (collisionGameObjects != null)
                    foreach (var gameObject in collisionGameObjects)
                    {
                        DebugWithoutIDEndline(this, gameObject.ID + "  ");
                    }
                DebugWithoutID(this, " Direction : " + collisionDirection);
                OnCollision?.Invoke(collisionDirection, collisionGameObjects);
            }
        }
        //Collision

        //Trigger
        public delegate void TriggerHandler(HashSet<GameObject> triggerGameObjects);
        public event TriggerHandler? OnTrigger;
        protected internal void Trigger(HashSet<GameObject> triggerGameObjects)
        {
            if (triggerGameObjects == null || triggerGameObjects.Count == 0)
                return;
            lock (privateLock)
            {
                DebugWithoutEndline(this, "Trigger with : ");
                foreach (var gameObject in triggerGameObjects)
                {
                    DebugWithoutIDEndline(this, gameObject.ID + "  ");
                }
                DebugWithoutID(this, "");
                OnTrigger?.Invoke(triggerGameObjects);
            }
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
