using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class GameObject
    {
        private readonly Object privateLock = new Object();
        public readonly Object publicLock = new Object();

        private static Int64 currentMaxID = 0;
        public readonly Int64 ID;

        protected GameObject? _parent;
        public GameObject? Parent
        {
            get { lock (privateLock) { return _parent; } }
            set
            {
                lock (privateLock)
                {
                    if (value != null && this._parent != null)
                    {
                        this.Debug("Reset Parent to : " + value.ID);
                        this._parent.OnChildrenDelete(this);
                        this._parent = value;
                        this._parent.OnChildrenAdded(this);
                    }
                    else if (value != null && this._parent == null)
                    {
                        this.Debug("Set new Parent : " + value.ID);
                        this._parent = value;
                        this._parent.OnChildrenAdded(this);
                    }
                    else if (value == null && this._parent != null)
                    {
                        this.Debug("Delete Parent : " + this._parent.ID);
                        this._parent.OnChildrenDelete(this);
                        this._parent = null;
                    }
                }
            }
        }

        //Children
        protected Dictionary<Int64, GameObject> _childrenGameObjectList = new Dictionary<Int64, GameObject>();
        public Dictionary<Int64, GameObject> ChildrenGameObjectList { get { lock (privateLock) { return this._childrenGameObjectList; } } }
        protected virtual void OnChildrenAdded(GameObject childrenObject)
        {
            lock (privateLock)
            {
                this.ChildrenGameObjectList.Add(childrenObject.ID, childrenObject);
                childrenObject.OnPositionChanged += this.OnChildrenPositionChanged;
                childrenObject.OnMove += this.OnChildrenMove;
                childrenObject.OnBlockableChanged += this.OnChildrenBlockableChanged;
                childrenObject.FrameRate = this.FrameRate;
            }
        }
        protected virtual void OnChildrenDelete(GameObject childrenObject)
        {
            lock (privateLock)
            {
                this.ChildrenGameObjectList.Remove(childrenObject.ID);
                childrenObject.OnPositionChanged -= this.OnChildrenPositionChanged;
                childrenObject.OnMove -= this.OnChildrenMove;
                childrenObject.OnBlockableChanged -= this.OnChildrenBlockableChanged;
            }
        }
        protected virtual void OnChildrenPositionChanged(GameObject gameObject, PositionChangedEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            eOut = new PositionChangeReturnEventArgs(false, e.position);
        }
        protected virtual void OnChildrenMove(GameObject gameObject, MoveEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            eOut = new PositionChangeReturnEventArgs(false, gameObject.Position + new XYPosition(e.distance * Math.Cos(e.angle), e.distance * Math.Sin(e.angle)));
        }
        protected virtual void OnChildrenBlockableChanged(GameObject gameObject, BlockableChangedEventArgs e)
        {
            ;
        }
        //Children end

        //Position
        protected XYPosition _position = new XYPosition();
        public XYPosition Position
        {
            get { lock (privateLock) { return this._position; } }
            set
            {
                lock (privateLock)
                {
                    this.Debug("Prompt to change position to : " + value.ToString());
                    PositionChanged(new PositionChangedEventArgs(this._position, value));
                    this.Debug("change position to : " + this._position.ToString());
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

        //PositionChangeReturnEventArgs类
        //用于表明位置是否被此对象以外的对象重设
        //若isReset == true，表明位置被重设，此对象需要把自己的Position重设为返回的position
        public class PositionChangeReturnEventArgs : EventArgs
        {
            public readonly bool isReset;
            public readonly XYPosition position;
            public PositionChangeReturnEventArgs(bool isReset, XYPosition position)
            {
                this.isReset = isReset;
                this.position = position;
            }
            public PositionChangeReturnEventArgs(PositionChangeReturnEventArgs e)
            {
                this.isReset = e.isReset;
                this.position = e.position;
            }
        }
        public delegate void PositionChangedHandler(GameObject sender, PositionChangedEventArgs e, out PositionChangeReturnEventArgs eOut);
        public event PositionChangedHandler? OnPositionChanged; // 声明事件
        protected virtual void PositionChanged(PositionChangedEventArgs e)
        {
            lock (privateLock)
            {
                if (OnPositionChanged != null)
                {
                    Delegate[] delegates = OnPositionChanged.GetInvocationList();
                    PositionChangeReturnEventArgs positionChangeReturnEventArgs = new PositionChangeReturnEventArgs(false, e.position);
                    foreach (var delegateItem in delegates)
                    {
                        PositionChangeReturnEventArgs tempPositionChangeReturnEventArgs = new PositionChangeReturnEventArgs(false, e.position);
                        PositionChangedHandler PositionChangedMethod = (PositionChangedHandler)delegateItem;
                        PositionChangedMethod(this, e, out tempPositionChangeReturnEventArgs);
                        if (tempPositionChangeReturnEventArgs.isReset)
                            positionChangeReturnEventArgs = tempPositionChangeReturnEventArgs;
                    }
                    this._position = positionChangeReturnEventArgs.position;
                }
                else
                {
                    this._position = e.position;
                }
            }
        }
        //Position end

        //Direction
        protected double _facingDirection = 0;
        public double FacingDirection
        {
            get { lock (privateLock) { return this._facingDirection; } }
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
            get { lock (privateLock) { return this._frameRate; } }
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
                        item.Value.FrameRate = this._frameRate;
                    return;
                }
            }
        }
        System.Threading.Timer MovingTimer = new System.Threading.Timer(new System.Threading.TimerCallback((o) => { }));
        protected Vector _velocity = new Vector();
        public Vector Velocity
        {
            get { lock (privateLock) { return this._velocity; } }
            set
            {
                lock (privateLock)
                {
                    if (Math.Abs(value.length) < 0.0001)
                    {
                        this._velocity = new Vector(value.angle, 0);
                        MovingTimer.Dispose();
                        return;
                    }
                    this._velocity = value;
                    MovingTimer = new System.Threading.Timer(
                        (o) =>
                        {
                            if (o is MoveEventArgs)
                                Move((MoveEventArgs)o);
                        },
                        new MoveEventArgs(value.angle, value.length / this.FrameRate),
                        TimeSpan.FromSeconds(0),
                        TimeSpan.FromSeconds(1 / this.FrameRate));
                }
            }
        }
        //Velocity end


        //Width
        protected int _width = 1;
        public int Width
        {
            get { lock (privateLock) { return this._width; } }
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
            get { lock (privateLock) { return this._height; } }
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

        //Blockable
        protected bool _blockable;
        public bool Blockable
        {
            get { lock (privateLock) { return this._blockable; } }
            set
            {
                lock (privateLock)
                {
                    bool temp = this._blockable;
                    this._blockable = value;
                    BlockableChanged(new BlockableChangedEventArgs(temp, value));
                }
            }
        }
        public class BlockableChangedEventArgs : EventArgs
        {
            public readonly bool previousBlockable;
            public readonly bool blockable;
            public BlockableChangedEventArgs(bool previousBlockable_t, bool blockable_t)
            {
                this.previousBlockable = previousBlockable_t;
                this.blockable = blockable_t;
            }
        }
        public delegate void BlockableChangedHandler(GameObject sender, BlockableChangedEventArgs e);
        public event BlockableChangedHandler? OnBlockableChanged; // 声明事件
        protected virtual void BlockableChanged(BlockableChangedEventArgs e)
        {
            if (OnBlockableChanged != null)
            {
                OnBlockableChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //Blockable end

        //Movable
        private bool _movable;
        public bool Movable
        {
            get { lock (privateLock) { return this._movable; } }
            set { lock (privateLock) { this._movable = value; } }
        }
        //Movable end

        public GameObject(GameObject? parent = null)
        {
            ID = currentMaxID;
            currentMaxID++;
            this.Parent = parent;
            this.Debug("has been newed . ");
        }
        public GameObject(XYPosition position, GameObject? parent = null) : this(parent)
        {
            this.Position = position;
        }

        //Move
        public class MoveEventArgs : EventArgs
        {
            public readonly double angle;
            public readonly double distance;
            public MoveEventArgs(double angle_t, double distance_t) // angle is radian
            {
                this.angle = CorrectAngle(angle_t);
                this.distance = distance_t;
            }
        }
        public delegate void MoveHandler(GameObject sender, MoveEventArgs e, out PositionChangeReturnEventArgs eOut);
        public event MoveHandler? OnMove;
        public virtual void Move(MoveEventArgs e)
        {
            lock (privateLock)
            {
                Debug("Move : angle : " + e.angle + " distance : " + e.distance);
                if (OnMove != null && this._movable)
                {
                    Delegate[] delegates = OnMove.GetInvocationList();
                    PositionChangeReturnEventArgs positionChangeReturnEventArgs = new PositionChangeReturnEventArgs(false, this.Position + new XYPosition(e.distance * Math.Cos(e.angle), e.distance * Math.Sin(e.angle)));
                    foreach (var delegateItem in delegates)
                    {
                        PositionChangeReturnEventArgs tempPositionChangeReturnEventArgs = new PositionChangeReturnEventArgs(positionChangeReturnEventArgs);
                        MoveHandler OnMoveMethod = (MoveHandler)delegateItem;
                        OnMoveMethod(this, e, out tempPositionChangeReturnEventArgs);
                        if (tempPositionChangeReturnEventArgs.isReset)
                            positionChangeReturnEventArgs = tempPositionChangeReturnEventArgs;
                    }
                    this._position = positionChangeReturnEventArgs.position;
                }
                Debug("Move result poition : " + this._position.ToString());
            }
        }
        //Move end

        ~GameObject()
        {
            this.Parent = null;
        }

        public void Debug(string str)
        {
            Console.WriteLine(this.GetType() + " " + this.ID + " : " + str);
        }
        public void DebugWithoutEndline(string str)
        {
            Console.Write(this.GetType() + " " + this.ID + " : " + str + " ");
        }
        public void DebugWithoutID(string str)
        {
            Console.WriteLine(str);
        }
    }
}
