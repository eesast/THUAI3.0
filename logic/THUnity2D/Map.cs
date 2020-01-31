using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class Map : GameObject
    {
        protected MapCell[,] _grid = new MapCell[0, 0];
        public MapCell[,] Grid
        {
            get { return _grid; }
            set
            {
                this.Debug("Set Grid : " + value.GetLength(0).ToString() + " , " + value.GetLength(1).ToString());
                this._grid = value;
                for (int row = 0; row < this._grid.GetLength(0); row++)
                    for (int column = 0; column < this._grid.GetLength(1); column++)
                    {
                        this._grid[row, column] = new MapCell();
                    }
            }
        }

        public Map(int width, int height) : base(null)
        {
            this.Debug("new Map : " + width.ToString() + " , " + height.ToString());
            this.Width = width;
            this.Height = height;
            this.Grid = new MapCell[Width, Height];
        }

        protected override void OnChildrenAdded(GameObject childrenObject)
        {
            childrenObject.Position = CorrectPosition(childrenObject.Position, childrenObject.Width, childrenObject.Height, childrenObject.Blockable);
            base.OnChildrenAdded(childrenObject);
            if (childrenObject.Blockable)
                this.Grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].BlockableObject = childrenObject;
            else
                this.Grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].unblockableObjects.AddLast(childrenObject);
            this.Debug("Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") add " + childrenObject.ID);
        }
        protected override void OnChildrenDelete(GameObject childrenObject)
        {
            base.OnChildrenDelete(childrenObject);
            if (childrenObject.Blockable)
            {
                _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].BlockableObject = null;
                this.Debug("Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") delete blockable " + childrenObject.ID);
            }
            else
            {
                _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].unblockableObjects.Remove(childrenObject);
                this.Debug("Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") delete unBlockable " + childrenObject.ID);
            }
        }
        protected override void OnChildrenPositionChanged(GameObject childrenGameObject, PositionChangedEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            this.Debug("Children object " + childrenGameObject.ID + " position change. from : " + e.previousPosition.ToString() + " aim : " + e.position.ToString());
            //base.OnChildrenPositionChanged(childrenGameObject, e, out eOut);

            if (XIsLegal((int)e.previousPosition.x) && YIsLegal((int)e.previousPosition.y)
                && childrenGameObject.Blockable
                && this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].BlockableObject == childrenGameObject)
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].BlockableObject = null;//如果需要把childrenGameObject从Grid上拿掉且可以拿掉

            XYPosition newPosition = CorrectPosition(e.position, childrenGameObject.Width, childrenGameObject.Height, childrenGameObject.Blockable);
            eOut = new PositionChangeReturnEventArgs(true, newPosition);

            if (childrenGameObject.Blockable)
                this._grid[(int)newPosition.x, (int)newPosition.y].BlockableObject = childrenGameObject;
            else if ((int)e.previousPosition.x != (int)newPosition.x || (int)e.previousPosition.y != (int)newPosition.y)
            {
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].unblockableObjects.Remove(childrenGameObject);
                this._grid[(int)newPosition.x, (int)newPosition.y].unblockableObjects.AddLast(childrenGameObject);
            }

        }
        protected bool YIsLegal(int y)
        {
            if (y < 0 || y >= this._height)
                return false;
            return true;
        }


        //protected bool XIsLegal(GameObject childrenGameObject)
        //{
        //    if (childrenGameObject.Position.x - (double)childrenGameObject.Width / 2 < 0
        //        || childrenGameObject.Position.x + (double)childrenGameObject.Width / 2 > (double)this._width)
        //        return false;
        //    return true;
        //}
        //protected bool YIsLegal(GameObject childrenGameObject)
        //{
        //    if (childrenGameObject.Position.y - (double)childrenGameObject.Height / 2 < 0
        //        || childrenGameObject.Position.y + (double)childrenGameObject.Height / 2 > (double)this._height)
        //        return false;
        //    return true;
        //}
        protected bool XIsLegal(double x, int objectWidth = 0)
        {
            if (x - (double)objectWidth / 2 < 0 || x + (double)objectWidth / 2 > (double)this._width)
                return false;
            return true;
        }
        protected bool YIsLegal(double y, int objectHeight = 0)
        {
            if (y - (double)objectHeight / 2 < 0 || y + (double)objectHeight / 2 > (double)this._height)
                return false;
            return true;
        }
        protected bool XIsLegal(int x)
        {
            if (x < 0 || x >= this._width)
                return false;
            return true;
        }
        protected bool YIsLegal(int y)
        {
            if (y < 0 || y >= this._height)
                return false;
            return true;
        }


        //检查childrenGameObject的位置是否合法
        //目前只能检查边长为1的方块
        //检查规则：任何方块的任何部分不能超出地图边界
        //如果方块为可碰撞的，则方块不能与其他可碰撞方块有重叠部分。
        protected bool XYPositionIsLegal(XYPosition position, int objectWidth = 0, int objectHeight = 0, bool objectBlockable = false)
        {
            this.DebugWithoutEndline("Checking position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " blockable : " + objectBlockable);

            if (!XIsLegal(position.x, objectWidth) || !YIsLegal(position.y, objectHeight))
            {
                this.DebugWithoutID("false");
                return false;
            }
            if (!objectBlockable)
            {
                this.DebugWithoutID("true");
                return true;
            }
            if (this._grid[(uint)(position.x), (uint)(position.y)].BlockableObject != null)
            {
                this.DebugWithoutID("false");
                return false;
            }
            XYPosition centerPosition = new XYPosition((int)(position.x) + 0.5, (int)(position.y) + 0.5);
            for (int i = 0; i < (int)(Direction.Size); i++)
            {
                XYPosition toCheckPosition = centerPosition + EightUnitVector[(Direction)i];
                //Console.WriteLine("corner : " + item.Key.ToString() + " , " + toCheckPosition.ToString());
                if (toCheckPosition.x < 0 || toCheckPosition.x >= this._width
                     || toCheckPosition.y < 0 || toCheckPosition.y >= this._height)
                {
                    continue;
                }
                else if (_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].BlockableObject != null)
                {
                    //Debug("Not null");
                    if (Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].BlockableObject.Position.x - position.x) < 1
                        && Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].BlockableObject.Position.y - position.y) < 1)
                    {
                        this.DebugWithoutID("false");
                        return false;
                    }
                }
            }
            this.DebugWithoutID("true");
            return true;
        }

        //调整方块到合法位置
        //目前只能调整边长为1的方块
        //此函数内不可调用childrenGameObject的Position Setter，否则会触发死循环
        //输入：一个GameObject（不一定是地图的子GameObject）
        protected XYPosition CorrectPosition(XYPosition position, int objectWidth = 1, int objectHeight = 1, bool objectBlockable = false)
        {
            Debug("Correcting Position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " blockable : " + objectBlockable);

            if (XYPositionIsLegal(position, objectWidth, objectHeight, objectBlockable))
                return position;

            XYPosition newCenterPosition = new XYPosition((int)(position.x) + 0.5, (int)(position.y) + 0.5);
            if (newCenterPosition.x < 0.5)
                newCenterPosition = new XYPosition(0.5, newCenterPosition.y);
            else if (newCenterPosition.x > (double)this._width - 0.5)
                newCenterPosition = new XYPosition((double)this._width - 0.5, newCenterPosition.y);
            if (newCenterPosition.y < 0.5)
                newCenterPosition = new XYPosition(newCenterPosition.x, 0.5);
            else if (newCenterPosition.y > (double)this._height - 0.5)
                newCenterPosition = new XYPosition(newCenterPosition.x, (double)this._height - 0.5);

            if (XYPositionIsLegal(newCenterPosition, objectWidth, objectHeight, objectBlockable))
                return newCenterPosition;

            XYPosition testPosition = newCenterPosition;
            for (int round = 1; round < Math.Max(this._width, this._height); round++)
            {
                //this.Debug("round : " + round);
                for (double ySearch = newCenterPosition.y - (double)round; ySearch <= newCenterPosition.y + (double)round + 0.1; ySearch += 2 * (double)round)
                {
                    if (!YIsLegal(ySearch))
                        continue;
                    for (double xSearch = newCenterPosition.x - (double)round; xSearch <= newCenterPosition.x + (double)round + 0.1; xSearch++)
                    {
                        if (!XIsLegal(xSearch))
                            continue;
                        testPosition = new XYPosition(xSearch, ySearch);
                        if (XYPositionIsLegal(testPosition, objectWidth, objectHeight, objectBlockable))
                            return testPosition;
                    }
                }
                for (double xSearch = newCenterPosition.x - round; xSearch <= newCenterPosition.x + round + 0.1; xSearch += 2 * round)
                {
                    if (!XIsLegal(xSearch))
                        continue;
                    for (double ySearch = newCenterPosition.y - (round - 1); ySearch <= newCenterPosition.y + (round - 1) + 0.1; ySearch++)
                    {
                        if (!YIsLegal(ySearch))
                            continue;
                        testPosition = new XYPosition(xSearch, ySearch);
                        if (XYPositionIsLegal(testPosition, objectWidth, objectHeight, objectBlockable))
                            return testPosition;
                    }
                }
            }
            return new XYPosition(0, 0);//找不到合法位置，返回无效值
        }

        protected void CorrectMoveEventArgs(GameObject childrenGameObject, ref MoveEventArgs e, out XYPosition aim)
        {
            double deltaX = e.distance * Math.Cos(e.angle);
            double initX = deltaX;
            double deltaY = e.distance * Math.Sin(e.angle);
            if (deltaX + childrenGameObject.Position.x < (double)childrenGameObject.Width / 2)
            {
                deltaY = deltaY * ((double)childrenGameObject.Width / 2 - childrenGameObject.Position.x) / deltaX;
                deltaX = (double)childrenGameObject.Width / 2 - childrenGameObject.Position.x;
            }
            else if (deltaX + childrenGameObject.Position.x > (double)this._width - (double)childrenGameObject.Width / 2)
            {
                deltaY = deltaY * ((double)this._width - (double)childrenGameObject.Width / 2 - childrenGameObject.Position.x) / deltaX;
                deltaX = (double)this._width - (double)childrenGameObject.Width / 2 - childrenGameObject.Position.x;
            }
            if (deltaY + childrenGameObject.Position.y < (double)childrenGameObject.Height / 2)
            {
                deltaX = deltaX * ((double)childrenGameObject.Height / 2 - childrenGameObject.Position.y) / deltaY;
                deltaY = (double)childrenGameObject.Height / 2 - childrenGameObject.Position.y;
            }
            else if (deltaY + childrenGameObject.Position.y > (double)this._height - (double)childrenGameObject.Height / 2)
            {
                deltaX = deltaX * ((double)this._height - (double)childrenGameObject.Height / 2 - childrenGameObject.Position.y) / deltaY;
                deltaY = (double)this._height - (double)childrenGameObject.Height / 2 - childrenGameObject.Position.y;
            }
            e = new MoveEventArgs(e.angle, deltaX * e.distance / initX);
            aim = new XYPosition(childrenGameObject.Position.x + deltaX, childrenGameObject.Position.y + deltaY);
        }

        protected override void OnChildrenMove(GameObject childrenGameObject, MoveEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            this.Debug("Attempting to move Children : " + childrenGameObject.ID);
            //base.OnChildrenMove(childrenGameObject, e, out eOut);
            XYPosition aim;
            CorrectMoveEventArgs(childrenGameObject, ref e, out aim);
            eOut = new PositionChangeReturnEventArgs(true, aim);

            if (!childrenGameObject.Blockable)//如果childrenGameObject不可碰撞，无需检测，直接返回
            {
                if ((int)aim.x != (int)childrenGameObject.Position.x || (int)aim.y != (int)childrenGameObject.Position.y)
                {
                    this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].unblockableObjects.Remove(childrenGameObject);
                    this._grid[(int)aim.x, (int)aim.y].unblockableObjects.AddLast(childrenGameObject);
                }
                return;
            }

            XYPosition deltaVector = aim - childrenGameObject.Position;
            this.Debug("Move Children : " + childrenGameObject.ID + " from : " + childrenGameObject.Position.ToString() + " aim : " + aim.ToString());
            Direction LeftOrRightBound = (e.angle >= Math.PI / 2 && e.angle < Math.PI * 3 / 2) ? Direction.Left : Direction.Right;
            Direction UpOrDownBound = (e.angle < Math.PI) ? Direction.Up : Direction.Down;
            this.Debug("Move children : " + childrenGameObject.ID + " direction : " + LeftOrRightBound + " , " + UpOrDownBound);

            List<double> LeftRightExtends = new List<double>();
            List<double> UpDownExtends = new List<double>();

            if (LeftOrRightBound == Direction.Left)
            {
                LeftRightExtends.Add(aim.x - 0.5);
                LeftRightExtends.Add(childrenGameObject.Position.x - 0.5);
            }
            else
            {
                LeftRightExtends.Add(childrenGameObject.Position.x + 0.5);
                LeftRightExtends.Add(aim.x + 0.5);
            }
            if (UpOrDownBound == Direction.Down)
            {
                UpDownExtends.Add(aim.y - 0.5);
                UpDownExtends.Add(childrenGameObject.Position.y - 0.5);
            }
            else
            {
                UpDownExtends.Add(childrenGameObject.Position.y + 0.5);
                UpDownExtends.Add(aim.y + 0.5);
            }

            HashSet<Tuple<int, int>> toCheckPositions = new HashSet<Tuple<int, int>>();//这个列表里的Position都是有可能与childrenGameObject发生碰撞的

            for (int row = (int)childrenGameObject.Position.y - 1; row <= (int)childrenGameObject.Position.y + 1; row += 2)
            {
                if (!YIsLegal(row))
                    continue;
                for (int column = (int)childrenGameObject.Position.x - 1; column <= (int)childrenGameObject.Position.x + 1; column++)
                {
                    if (!XIsLegal(column))
                        continue;
                    if (this._grid[column, row].BlockableObject != null)
                    {
                        toCheckPositions.Add(new Tuple<int, int>(column, row));
                        this.Debug("toCheckPosition : (" + column + "," + row + ")  Added");
                    }
                    else
                        this.Debug("toCheckPosition : (" + column + "," + row + ")  Ignored");
                }
            }
            for (int column = (int)childrenGameObject.Position.x - 1; column <= (int)childrenGameObject.Position.x + 1; column += 2)
            {
                if (!XIsLegal(column))
                    continue;
                for (int row = (int)childrenGameObject.Position.y - 1 + 1; row <= (int)childrenGameObject.Position.y + 1 - 1; row++)
                {
                    if (!YIsLegal(row))
                        continue;
                    if (this._grid[column, row].BlockableObject != null)
                    {
                        toCheckPositions.Add(new Tuple<int, int>(column, row));
                        this.Debug("toCheckPosition : (" + column + "," + row + ")  Added");
                    }
                    else
                        this.Debug("toCheckPosition : (" + column + "," + row + ")  Ignored");
                }
            }

            for (double xToCheck = (int)(LeftRightExtends[0] - 0.5) + 1 + 0.5; xToCheck < LeftRightExtends[1]; xToCheck++)
            {
                double yDown = childrenGameObject.Position.y - 0.5 + (xToCheck - (childrenGameObject.Position.x + ((LeftOrRightBound == Direction.Left) ? -0.5 : 0.5))) * Math.Tan(e.angle);
                double yToCheck = (int)(yDown - 0.5) + 1 + 0.5;
                if (YIsLegal((int)yToCheck) && XIsLegal((int)xToCheck)
                    && this._grid[(int)xToCheck, (int)yToCheck].BlockableObject != null
                    && this._grid[(int)xToCheck, (int)yToCheck].BlockableObject != childrenGameObject)
                {
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    this.Debug("toCheckPosition : (" + (int)xToCheck + "," + (int)yToCheck + ")  Added");
                }
                else
                    this.Debug("toCheckPosition : (" + (int)xToCheck + "," + (int)yToCheck + ")  Ignored");
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (YIsLegal(yToAdd) && XIsLegal(xToAdd)
                        && this._grid[xToAdd, yToAdd].BlockableObject != null
                        && this._grid[xToAdd, yToAdd].BlockableObject != childrenGameObject)
                    {
                        toCheckPositions.Add(new Tuple<int, int>(xToAdd, yToAdd));
                        this.Debug("toCheckPosition : (" + xToAdd + "," + yToAdd + ")  Added");
                    }
                    else
                        this.Debug("toCheckPosition : (" + xToAdd + "," + yToAdd + ")  Ignored");
                }
            }
            for (double yToCheck = (int)(UpDownExtends[0] - 0.5) + 1 + 0.5; yToCheck < UpDownExtends[1]; yToCheck++)
            {
                double xLeft = childrenGameObject.Position.x - 0.5 + (yToCheck - (childrenGameObject.Position.y + ((UpOrDownBound == Direction.Down) ? -0.5 : 0.5))) / Math.Tan(e.angle);
                double xToCheck = (int)(xLeft - 0.5) + 1 + 0.5;
                if (YIsLegal((int)yToCheck) && XIsLegal((int)xToCheck)
                    && this._grid[(int)xToCheck, (int)yToCheck].BlockableObject != null
                    && this._grid[(int)xToCheck, (int)yToCheck].BlockableObject != childrenGameObject)
                {
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    this.Debug("toCheckPosition : (" + (int)xToCheck + "," + (int)yToCheck + ")  Added");
                }
                else
                    this.Debug("toCheckPosition : (" + (int)xToCheck + "," + (int)yToCheck + ")  Ignored");
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (YIsLegal(yToAdd) && XIsLegal(xToAdd)
                        && this._grid[xToAdd, yToAdd].BlockableObject != null
                        && this._grid[xToAdd, yToAdd].BlockableObject != childrenGameObject)
                    {
                        toCheckPositions.Add(new Tuple<int, int>(xToAdd, yToAdd));
                        this.Debug("toCheckPosition : (" + xToAdd + "," + yToAdd + ")  Added");
                    }
                    else
                        this.Debug("toCheckPosition : (" + xToAdd + "," + yToAdd + ")  Ignored");
                }
            }

            double resultDistance = e.distance;
            Debug("initialize resultDistance : " + resultDistance);
            foreach (var toCheckPosition in toCheckPositions)
            {
                double tempResultDistance = resultDistance;
                double LeftRightDistance = resultDistance;
                double intervalX = (LeftOrRightBound == Direction.Left) ?
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.x + 0.5) - (childrenGameObject.Position.x - 0.5)) :
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.x - 0.5) - (childrenGameObject.Position.x + 0.5));
                if (intervalX < Math.Abs(deltaVector.x))
                {
                    double yDown = childrenGameObject.Position.y - 0.5 + intervalX * Math.Sign(deltaVector.x) * deltaVector.y / deltaVector.x;
                    if (yDown > this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.y - 0.5 - 1
                        && yDown < this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.y + 0.5)
                    {
                        //this.Debug("intervalX : " + intervalX);
                        LeftRightDistance = intervalX * resultDistance / Math.Abs(deltaVector.x);
                    }
                }
                //this.Debug("LeftRightDistance : " + LeftRightDistance);

                double UpDownDistance = resultDistance;
                double intervalY = (UpOrDownBound == Direction.Down) ?
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.y + 0.5) - (childrenGameObject.Position.y - 0.5)) :
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.y - 0.5) - (childrenGameObject.Position.y + 0.5));
                if (intervalY < Math.Abs(deltaVector.y))
                {
                    double xLeft = childrenGameObject.Position.x - 0.5 + intervalY * Math.Sign(deltaVector.y) * deltaVector.x / deltaVector.y;
                    if (xLeft > this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.x - 0.5 - 1
                        && xLeft < this._grid[toCheckPosition.Item1, toCheckPosition.Item2].BlockableObject.Position.x + 0.5)
                        UpDownDistance = intervalY * resultDistance / Math.Abs(deltaVector.y);
                }
                //this.Debug("UpDownDistance : " + UpDownDistance);

                tempResultDistance = Math.Min(LeftRightDistance, UpDownDistance);
                Debug("tempResultDistance : " + tempResultDistance);

                if (tempResultDistance < resultDistance)
                {
                    deltaVector = new XYPosition(tempResultDistance / resultDistance * deltaVector.x, tempResultDistance / resultDistance * deltaVector.y);
                    resultDistance = tempResultDistance;
                }
            }
            Debug("resultDistance : " + resultDistance);
            XYPosition resultPosition = childrenGameObject.Position + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle));
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].BlockableObject = null;
            this._grid[(int)resultPosition.x, (int)resultPosition.y].BlockableObject = childrenGameObject;
            eOut = new PositionChangeReturnEventArgs(true, resultPosition);
        }
        protected override void OnChildrenBlockableChanged(GameObject childrenGameObject, BlockableChangedEventArgs e)
        {
            base.OnChildrenBlockableChanged(childrenGameObject, e);
            if (e.previousBlockable != e.blockable
                && XIsLegal((int)childrenGameObject.Position.x) && YIsLegal((int)childrenGameObject.Position.y))
            {
                if (e.previousBlockable
                    && this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].BlockableObject == childrenGameObject)
                {
                    this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].BlockableObject = null;
                    this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].unblockableObjects.AddLast(childrenGameObject);
                }
                else
                {
                    this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].unblockableObjects.Remove(childrenGameObject);
                    childrenGameObject.Position = CorrectPosition(childrenGameObject.Position, childrenGameObject.Width, childrenGameObject.Height, childrenGameObject.Blockable);
                    this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].BlockableObject = childrenGameObject;
                }
            }
        }
    }
    public class MapCell
    {
        public readonly object publicLock = new object();
        protected readonly object privateLock = new object();
        protected GameObject? _blockableObject = null;
        public GameObject? BlockableObject
        {
            get { lock (privateLock) { return this._blockableObject; } }
            set { lock (privateLock) { this._blockableObject = value; } }
        }
        public LinkedList<GameObject> unblockableObjects = new LinkedList<GameObject>();
    }
}
