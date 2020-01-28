using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;

namespace THUnity2D
{
    public class Map : GameObject
    {
        protected MapCell[,] _grid;
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
        public readonly int FrameRate;
        public readonly int TimeInterval;

        public Map(int width, int height, int frameRate = 50) : base(width, height, null)
        {
            this.Debug("new Map : " + width.ToString() + " , " + height.ToString());
            this.FrameRate = frameRate;
            this.TimeInterval = 1 / this.FrameRate;
            this.Grid = new MapCell[Width, Height];
        }

        protected override void OnChildrenAdded(GameObject childrenObject)
        {
            childrenObject.Position = CorrectPosition(childrenObject);
            base.OnChildrenAdded(childrenObject);
            this.Grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].blockableObject = childrenObject;
            this.Debug("Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") add " + childrenObject.ID);
        }
        protected override void OnChildrenDelete(GameObject childrenObject)
        {
            base.OnChildrenDelete(childrenObject);
            if (childrenObject.Blockable)
            {
                _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].blockableObject = null;
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
            this.Debug("Children object position change");
            //base.OnChildrenPositionChanged(childrenGameObject, e, out eOut);
            if (childrenGameObject.Blockable)
            {
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].blockableObject = null;
                this.Debug("Grid (" + (int)e.previousPosition.x + "," + (int)e.previousPosition.y + ") delete blockable " + childrenGameObject.ID);
                if (!XYPositionIsLegal(childrenGameObject))
                {
                    XYPosition tempPosition = CorrectPosition(childrenGameObject);
                    eOut = new PositionChangeReturnEventArgs(true, tempPosition);
                    this._grid[(int)tempPosition.x, (int)tempPosition.y].blockableObject = childrenGameObject;
                }
                else
                {
                    eOut = new PositionChangeReturnEventArgs(true, e.position);
                    this._grid[(int)e.position.x, (int)e.position.y].blockableObject = childrenGameObject;
                }
            }
            else
            {
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].unblockableObjects.Remove(childrenGameObject);
                eOut = new PositionChangeReturnEventArgs(true, e.position);
                this._grid[(int)e.position.x, (int)e.position.y].unblockableObjects.Add(childrenGameObject);
            }
        }

        protected bool XIsLegal(GameObject childrenGameObject)
        {
            if (childrenGameObject.Position.x - childrenGameObject.Width / 2 < 0
                || childrenGameObject.Position.x + childrenGameObject.Width / 2 > this._width)
                return false;
            return true;
        }
        protected bool YIsLegal(GameObject childrenGameObject)
        {
            if (childrenGameObject.Position.y - childrenGameObject.Height / 2 < 0
                || childrenGameObject.Position.y + childrenGameObject.Height / 2 > this._height)
                return false;
            return true;
        }
        protected bool XIsLegal(double x)
        {
            if (x < 0 || x > this._width)
                return false;
            return true;
        }
        protected bool YIsLegal(double y)
        {
            if (y < 0 || y > this._height)
                return false;
            return true;
        }

        //检查childrenGameObject的位置是否合法
        //目前只能检查边长为1的方块
        protected bool XYPositionIsLegal(GameObject childrenGameObject)
        {
            this.DebugWithoutEndline("Checking child's position : " + childrenGameObject.ID.ToString() + " : " + childrenGameObject.Position.ToString());
            if (!childrenGameObject.Blockable)
            {
                this.DebugWithoutID("true");
                return true;
            }
            if (!XIsLegal(childrenGameObject) || !YIsLegal(childrenGameObject))
            {
                this.DebugWithoutID("false");
                return false;
            }
            else if (this._grid[(uint)(childrenGameObject.Position.x), (uint)(childrenGameObject.Position.y)].blockableObject != null)
            {
                this.DebugWithoutID("false");
                return false;
            }
            XYPosition centerPosition = new XYPosition((int)(childrenGameObject.Position.x) + 0.5, (int)(childrenGameObject.Position.y) + 0.5);
            for (int i = 0; i < (int)(Direction.Size); i++)
            {
                XYPosition toCheckPosition = centerPosition + EightUnitVector[(Direction)i];
                //Console.WriteLine("corner : " + item.Key.ToString() + " , " + toCheckPosition.ToString());
                if (toCheckPosition.x < 0 || toCheckPosition.x >= this._width
                     || toCheckPosition.y < 0 || toCheckPosition.y >= this._height)
                {
                    continue;
                }
                else if (_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].blockableObject != null)
                {
                    //Debug("Not null");
                    if (Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].blockableObject.Position.x - childrenGameObject.Position.x) < 1
                        && Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].blockableObject.Position.y - childrenGameObject.Position.y) < 1)
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
        protected XYPosition CorrectPosition(GameObject childrenGameObject)
        {
            Debug("Correcting Position : " + childrenGameObject.ID + " : " + childrenGameObject.Position.ToString());

            if (XYPositionIsLegal(childrenGameObject))
                return childrenGameObject.Position;

            XYPosition result = new XYPosition((uint)(childrenGameObject.Position.x) + 0.5, (uint)(childrenGameObject.Position.y) + 0.5);
            if (result.x < 0.5)
                result = new XYPosition(0.5, result.y);
            else if (result.x > this._width - 0.5)
                result = new XYPosition(this._width - 0.5, result.y);
            if (result.y < 0.5)
                result = new XYPosition(result.x, 0.5);
            else if (result.y > this._height - 0.5)
                result = new XYPosition(result.x, this._height - 0.5);

            GameObject testGameObject = new GameObject { Blockable = true, Position = new XYPosition(result.x, result.y) };
            for (int round = 1; round < Math.Max(this._width, this._height); round++)
            {
                //this.Debug("round : " + round);
                for (double ySearch = result.y - round; ySearch <= result.y + round + 0.1; ySearch += 2 * round)
                {
                    if (ySearch < 0 || ySearch > this._height)
                        continue;
                    for (double xSearch = result.x - round; xSearch <= result.x + round + 0.1; xSearch++)
                    {
                        if (xSearch < 0 || xSearch > this._width)
                            continue;
                        testGameObject.Position = new XYPosition(xSearch, ySearch);
                        if (XYPositionIsLegal(testGameObject))
                            return testGameObject.Position;
                    }
                }
                for (double xSearch = result.x - round; xSearch <= result.x + round + 0.1; xSearch += 2 * round)
                {
                    if (xSearch < 0 || xSearch > this._width)
                        continue;
                    for (double ySearch = result.y - (round - 1); ySearch <= result.y + (round - 1) + 0.1; ySearch++)
                    {
                        if (ySearch < 0 || ySearch > this._height)
                            continue;
                        testGameObject.Position = new XYPosition(xSearch, ySearch);
                        if (XYPositionIsLegal(testGameObject))
                            return testGameObject.Position;
                    }
                }
            }
            return new XYPosition(0, 0);//找不到合法位置，返回无效值
        }
        protected override void OnChildrenMove(GameObject childrenGameObject, MoveEventArgs e, out PositionChangeReturnEventArgs eOut)
        {
            this.Debug("Move Children : " + childrenGameObject.ID);
            base.OnChildrenMove(childrenGameObject, e, out eOut);

            if (!childrenGameObject.Blockable)
            {
                eOut = new PositionChangeReturnEventArgs(true, eOut.position);
                return;
            }

            double angle = CorrectAngle(e.angle);
            XYPosition aim = eOut.position;
            this.Debug("Move Children : " + childrenGameObject.ID + " aim : " + aim.ToString());
            Direction LeftOrRightBound = (angle >= Math.PI / 2 && angle < Math.PI * 3 / 2) ? Direction.Left : Direction.Right;
            Direction UpOrDownBound = (angle < Math.PI) ? Direction.Up : Direction.Down;

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

            HashSet<Tuple<int, int>> toCheckPositions = new HashSet<Tuple<int, int>>();

            for (int row = (int)childrenGameObject.Position.y - 1; row <= (int)childrenGameObject.Position.y + 1; row += 2)
                for (int column = (int)childrenGameObject.Position.x - 1; column <= (int)childrenGameObject.Position.x + 1; column++)
                {
                    toCheckPositions.Add(new Tuple<int, int>(column, row));
                }
            for (int column = (int)childrenGameObject.Position.x - 1; column <= (int)childrenGameObject.Position.x + 1; column += 2)
                for (int row = (int)childrenGameObject.Position.y - 1 + 1; row <= (int)childrenGameObject.Position.y + 1 - 1; row++)
                {
                    toCheckPositions.Add(new Tuple<int, int>(column, row));
                }

            for (double xToCheck = (int)(LeftRightExtends[0] - 0.5) + 1 + 0.5; xToCheck < LeftRightExtends[1]; xToCheck++)
            {
                double yDown = childrenGameObject.Position.y - 0.5 + (xToCheck - (childrenGameObject.Position.x + ((LeftOrRightBound == Direction.Left) ? -0.5 : 0.5))) * Math.Tan(e.angle);
                double yToCheck = (int)(yDown - 0.5) + 1 + 0.5;
                if (this._grid[(int)xToCheck, (int)yToCheck].blockableObject != childrenGameObject)
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (this._grid[xToAdd, yToAdd].blockableObject != childrenGameObject)
                        toCheckPositions.Add(new Tuple<int, int>(xToAdd, yToAdd));
                }
            }
            for (double yToCheck = (int)(UpDownExtends[0] - 0.5) + 1 + 0.5; yToCheck < UpDownExtends[1]; yToCheck++)
            {
                double xLeft = childrenGameObject.Position.x - 0.5 + (yToCheck - (childrenGameObject.Position.y + ((UpOrDownBound == Direction.Down) ? -0.5 : 0.5))) / Math.Tan(e.angle);
                double xToCheck = (int)(xLeft - 0.5) + 1 + 0.5;
                if (this._grid[(int)xToCheck, (int)yToCheck].blockableObject != childrenGameObject)
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (this._grid[xToAdd, yToAdd].blockableObject != childrenGameObject)
                        toCheckPositions.Add(new Tuple<int, int>(xToAdd, yToAdd));
                }
            }

            double resultDistance = e.distance;
            XYPosition deltaVector = aim - childrenGameObject.Position;
            Debug("initialize resultDistance : " + resultDistance);
            foreach (var toCheckPosition in toCheckPositions)
            {
                Debug("toCheckPosition : (" + toCheckPosition.Item1 + "," + toCheckPosition.Item2 + ")");
                double tempResultDistance = resultDistance;
                if (this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject == null
                    || this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject == childrenGameObject)
                {
                    continue;
                }

                DebugWithoutID("Not Null");

                double LeftRightDistance = resultDistance;
                double intervalX = (LeftOrRightBound == Direction.Left) ?
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x + 0.5) - (childrenGameObject.Position.x - 0.5)) :
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x - 0.5) - (childrenGameObject.Position.x + 0.5));
                if (intervalX < Math.Abs(deltaVector.x))
                {
                    double yDown = childrenGameObject.Position.y - 0.5 + intervalX * Math.Sign(deltaVector.x) * deltaVector.y / deltaVector.x;
                    if (yDown > this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y - 0.5 - 1
                        && yDown < this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y + 0.5)
                        LeftRightDistance = intervalX * resultDistance / Math.Abs(deltaVector.x);
                }

                double UpDownDistancce = resultDistance;
                double intervalY = (UpOrDownBound == Direction.Down) ?
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y + 0.5) - (childrenGameObject.Position.y - 0.5)) :
                    Math.Abs((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y - 0.5) - (childrenGameObject.Position.y + 0.5));
                if (intervalY < Math.Abs(deltaVector.y))
                {
                    double xLeft = childrenGameObject.Position.x - 0.5 + intervalY * Math.Sign(deltaVector.y) * deltaVector.x / deltaVector.y;
                    if (xLeft > this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x - 0.5 - 1
                        && xLeft < this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x + 0.5)
                        UpDownDistancce = intervalY * resultDistance / Math.Abs(deltaVector.y);
                }

                tempResultDistance = Math.Min(LeftRightDistance, UpDownDistancce);
                if (tempResultDistance < resultDistance)
                    resultDistance = tempResultDistance;
            }
            Debug("resultDistance : " + resultDistance);
            XYPosition resultPosition = childrenGameObject.Position + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle));
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].blockableObject = null;
            this._grid[(int)resultPosition.x, (int)resultPosition.y].blockableObject = childrenGameObject;
            eOut = new PositionChangeReturnEventArgs(true, resultPosition);
        }
    }
    public class MapCell
    {
        public readonly object publicLock = new object();
        public GameObject? blockableObject = null;
        public List<GameObject> unblockableObjects = new List<GameObject>();
    }
}
