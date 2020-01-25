using System;
using System.Collections.Generic;
using System.Text;

namespace THUnity2D
{
    public class GameObject
    {
        private readonly Object privateLock = new Object();
        public readonly Object publicLock = new Object();

        private static Int64 currentMaxID = 0;
        public readonly Int64 ID;

        protected GameObject _parent;
        public GameObject Parent
        {
            get { return _parent; }
            set
            {
                if (this._parent == null)
                {
                    this._parent = value;
                }
                else if (value != null)
                {
                    this._parent = value;
                    this._parent.OnChildrenAdded(this);
                }
                else
                {
                    this._parent.OnChildrenDelete(this);
                    this._parent = null;
                }
            }
        }

        protected Dictionary<Int64, GameObject> _childrenGameObjectList = new Dictionary<Int64, GameObject>();
        public Dictionary<Int64, GameObject> ChildrenGameObjectList { get { return this._childrenGameObjectList; } }
        protected virtual void OnChildrenAdded(GameObject childrenObject)
        {
            this.ChildrenGameObjectList.Add(childrenObject.ID, childrenObject);
            childrenObject.OnPositionChanged += this.OnChildrenPositionChanged;
            childrenObject.OnMove += this.OnChildrenMove;
        }
        protected virtual void OnChildrenDelete(GameObject childrenObject)
        {
            this.ChildrenGameObjectList.Remove(childrenObject.ID);
            childrenObject.OnPositionChanged -= this.OnChildrenPositionChanged;
            childrenObject.OnMove -= this.OnChildrenMove;
        }
        protected virtual void OnChildrenPositionChanged(GameObject gameObject, PositionChangedEventArgs e)
        {
            ;
        }
        protected virtual void OnChildrenMove(GameObject gameObject, MoveEventArgs e, out bool isMoved)
        {
            isMoved = true;
        }

        //Position
        protected XYPosition _position = new XYPosition();
        public XYPosition Position { get { return this._position; } set { this._position = value; PositionChanged(new PositionChangedEventArgs(this._position)); } }
        public class PositionChangedEventArgs : EventArgs
        {
            public readonly XYPosition position;
            public PositionChangedEventArgs(XYPosition position_t)
            {
                this.position = position_t;
            }
        }
        public delegate void PositionChangedHandler(GameObject sender, PositionChangedEventArgs e);
        public event PositionChangedHandler OnPositionChanged; // 声明事件
        protected virtual void PositionChanged(PositionChangedEventArgs e)
        {
            if (OnPositionChanged != null)
            {
                OnPositionChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //Position end

        //Direction
        protected double _facingDirection = 0;
        public double FacingDirection { get { return this._facingDirection; } set { this._facingDirection = value; OnDirectionChanged(new DirectionChangedEventArgs(this._facingDirection)); } }
        public class DirectionChangedEventArgs : EventArgs
        {
            public readonly double facingDirection;
            public DirectionChangedEventArgs(double facingDirection)
            {
                this.facingDirection = facingDirection;
            }
        }
        public delegate void DirectionChangedHandler(GameObject sender, DirectionChangedEventArgs e);
        public event DirectionChangedHandler DirectionChanged; // 声明事件
        protected virtual void OnDirectionChanged(DirectionChangedEventArgs e)
        {
            if (DirectionChanged != null)
            {
                DirectionChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //direction end

        //Width
        protected int _width;
        public int Width { get { return this._width; } set { this._width = value; } }
        public class WidthChangedEventArgs : EventArgs
        {
            public readonly int width;
            public WidthChangedEventArgs(int width_t)
            {
                this.width = width_t;
            }
        }
        public delegate void WidthChangedHandler(GameObject sender, WidthChangedEventArgs e);
        public event WidthChangedHandler WidthChanged; // 声明事件
        protected virtual void OnWidthChanged(WidthChangedEventArgs e)
        {
            if (WidthChanged != null)
            {
                WidthChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //Width end

        //Height
        protected int _height;
        public int Height { get { return this._height; } set { this._height = value; } }
        //Height end

        //Blockable
        protected bool _blockable;
        public bool Blockable { get { return this._blockable; } set { this._blockable = value; } }
        public class BlockableChangedEventArgs : EventArgs
        {
            public readonly bool blockable;
            public BlockableChangedEventArgs(bool blockable_t)
            {
                this.blockable = blockable_t;
            }
        }
        public delegate void BlockableChangedHandler(GameObject sender, BlockableChangedEventArgs e);
        public event BlockableChangedHandler BlockableChanged; // 声明事件
        protected virtual void OnBlockableChanged(BlockableChangedEventArgs e)
        {
            if (BlockableChanged != null)
            {
                BlockableChanged(this, e); // 调用所有注册对象的方法
            }
        }
        //Blockable end

        //Movable
        public bool _movable { get; }
        //Movable end

        public GameObject(int width = 1, int height = 1, GameObject parent = null)
        {
            Console.WriteLine("new GameObject");
            ID = currentMaxID;
            currentMaxID++;
            this.Parent = parent;
            this.Width = width;
            this.Height = height;
        }
        public GameObject(XYPosition position, int width = 1, int height = 1, GameObject parent = null) : this(width, height, parent)
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
                this.angle = angle_t;
                this.distance = distance_t;
            }
        }
        public delegate void MoveHandler(GameObject sender, MoveEventArgs e, out bool isMoved);
        public event MoveHandler OnMove;
        public virtual void Move(MoveEventArgs e)
        {
            lock (privateLock)
            {
                if (OnMove != null && this._movable)
                {
                    bool isMoved = false;
                    Delegate[] delegateList = OnMove.GetInvocationList();
                    foreach (var delegateItem in delegateList)
                    {
                        MoveHandler MoveMethod = (MoveHandler)delegateItem;
                        bool isMovedTemp = false;
                        MoveMethod(this, e, out isMovedTemp);
                        isMoved |= isMovedTemp;
                    }
                    if (!isMoved)
                    {
                        this._position = this._position + new XYPosition(e.distance * Math.Cos((float)e.angle), e.distance * Math.Sin((float)e.angle));
                    }
                }
            }
        }
        //Move end

        ~GameObject()
        {

        }
    }
}
