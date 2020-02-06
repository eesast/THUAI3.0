using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class GameObject
    {
        public const double MinSpeed = 0.0001;

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
        protected HashSet<GameObject> _childrenGameObjectList = new HashSet<GameObject>();
        public HashSet<GameObject> ChildrenGameObjectList { get { lock (privateLock) { return this._childrenGameObjectList; } } }
        protected virtual void OnChildrenAdded(GameObject childrenObject)
        {
            lock (privateLock)
            {
                this.ChildrenGameObjectList.Add(childrenObject);
                childrenObject.OnPositionChanged += this.OnChildrenPositionChanged;
                childrenObject.OnMove += this.OnChildrenMove;
                //childrenObject.OnBlockableChanged += this.OnChildrenBlockableChanged;
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
                //childrenObject.OnBlockableChanged -= this.OnChildrenBlockableChanged;
            }
        }
        protected virtual void OnChildrenPositionChanged(GameObject gameObject, PositionChangedEventArgs e)
        { }
        protected virtual void OnChildrenMove(GameObject gameObject, MoveEventArgs e, XYPosition previousPosition)
        { }
        //Children end

        //Position
        protected internal XYPosition _position = new XYPosition();
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

        public delegate void PositionChangedHandler(GameObject sender, PositionChangedEventArgs e);
        public event PositionChangedHandler? OnPositionChanged; // 声明事件
        public delegate void PositionChangeCompleteHandler(GameObject sender);
        public event PositionChangeCompleteHandler? PositionChangeComplete;
        protected virtual void PositionChanged(PositionChangedEventArgs e)
        {
            lock (privateLock)
            {
                _position = e.position;
                if (OnPositionChanged != null)
                {
                    OnPositionChanged(this, e);
                }
            }
            if (PositionChangeComplete != null)
                PositionChangeComplete(this);
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
                                Move(new MoveEventArgs(_velocity.angle, _velocity.length / _frameRate));
                            }
                        });
                }
                return _movingTimer;
            }
        }
        protected Vector _velocity = new Vector();
        public Vector Velocity
        {
            get { lock (privateLock) { return this._velocity; } }
            set
            {
                lock (privateLock)
                {
                    Debug("Dispose MovingTimer");
                    if (Math.Abs(value.length) < MinSpeed)
                    {
                        this._velocity = new Vector(value.angle, 0);
                        MovingTimer.Change(-1, -1);
                        return;
                    }
                    this._velocity = value;
                    MovingTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1 / this.FrameRate));
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

        //Layer
        protected internal int _layer = 0;
        public int Layer
        {
            get { lock (privateLock) { return _layer; } }
            set
            {
                lock (privateLock)
                {
                    this.Debug("Prompt to change layer from " + this._layer + " to " + value);
                    LayerChange(new LayerChangedEventArgs(this._layer, value));
                    this.Debug("Change layer to " + this._layer);
                }
            }
        }
        public class LayerChangedEventArgs : EventArgs
        {
            public readonly int previousLayer;
            public readonly int layer;
            public LayerChangedEventArgs(int previousLayer, int layer)
            {
                this.previousLayer = previousLayer;
                this.layer = layer;
            }
        }
        public delegate void LayerChangeHandler(GameObject sender, LayerChangedEventArgs e);
        public event LayerChangeHandler? OnLayerChange;
        protected virtual void LayerChange(LayerChangedEventArgs e)
        {
            lock (privateLock)
            {
                _layer = e.layer;
                if (OnLayerChange != null)
                {
                    OnLayerChange(this, e);
                }
            }
        }
        //Layer

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
                if (double.IsNaN(distance_t))
                    distance_t = 0;
                this.distance = distance_t;
            }
        }
        public delegate void MoveHandler(GameObject sender, MoveEventArgs e, XYPosition previousPosition);
        public event MoveHandler? OnMove;
        public delegate void MoveCompleteHandler(GameObject sender);
        public event MoveCompleteHandler? MoveComplete;
        public virtual void Move(MoveEventArgs e)
        {
            lock (privateLock)
            {
                Debug("Move from " + _position.ToString() + " angle : " + e.angle + " distance : " + e.distance);
                XYPosition previousPosition = _position;
                _position = _position + new XYPosition(e.distance * Math.Cos(e.angle), e.distance * Math.Sin(e.angle));
                if (OnMove != null && this._movable)
                {
                    OnMove(this, e, previousPosition);
                }
                Debug("Move result poition : " + this._position.ToString());
            }
            if (MoveComplete != null)
                MoveComplete(this);
        }
        //Move end

        //Bouncable
        protected bool _bouncable = false;
        public bool Bouncable
        {
            get { return _bouncable; }
            set
            {
                _bouncable = value;
                Debug("Set Bouncable " + _bouncable);
            }
        }
        //Bouncable

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
