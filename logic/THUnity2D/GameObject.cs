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
            get { return _parent; }
            set
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
        protected virtual void OnChildrenPositionChanged(GameObject gameObject, PositionChangedEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            eOut = new PositionChangeReturnEventArgs(false, new XYPosition());
        }
        protected virtual void OnChildrenMove(GameObject gameObject, MoveEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            eOut = new PositionChangeReturnEventArgs(false, gameObject.Position + new XYPosition(e.distance * Math.Cos(e.angle), e.distance * Math.Sin(e.angle)));
        }

        //Position
        protected XYPosition _position = new XYPosition();
        public XYPosition Position
        {
            get { return this._position; }
            set
            {
                this.Debug("Prompt to change position to : " + value.ToString());
                PositionChanged(new PositionChangedEventArgs(this._position, value));
                this.Debug("change position to : " + this._position.ToString());
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
        public event PositionChangedHandler OnPositionChanged; // 声明事件
        protected virtual void PositionChanged(PositionChangedEventArgs e)
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
        private bool _movable;
        public bool Movable { get { return this._movable; } set { this._movable = value; } }
        //Movable end

        public GameObject(int width = 1, int height = 1, GameObject? parent = null)
        {
            ID = currentMaxID;
            currentMaxID++;
            this.Parent = parent;
            this.Width = width;
            this.Height = height;
            this.Debug("has been newed . " + this._position.ToString());
        }
        public GameObject(XYPosition position, int width = 1, int height = 1, GameObject? parent = null) : this(width, height, parent)
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
        public delegate void MoveHandler(GameObject sender, MoveEventArgs e, out PositionChangeReturnEventArgs eOut);
        public event MoveHandler OnMove;
        public virtual void Move(MoveEventArgs e)
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
