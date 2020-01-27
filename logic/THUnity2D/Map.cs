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
                if (!CheckXYPosition(childrenGameObject))
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

        //检查childrenGameObject的位置是否合法
        //目前只能检查边长为1的方块
        protected bool CheckXYPosition(GameObject childrenGameObject)
        {
            this.Debug("Checking child's position : " + childrenGameObject.ID.ToString() + " : " + childrenGameObject.Position.ToString());
            if (!childrenGameObject.Blockable)
                return true;
            if (childrenGameObject.Position.x - childrenGameObject.Width / 2 < 0
                || childrenGameObject.Position.x + childrenGameObject.Width / 2 > this._width
                || childrenGameObject.Position.y - childrenGameObject.Height / 2 < 0
                || childrenGameObject.Position.y + childrenGameObject.Height / 2 > this._height)
                return false;
            else if (this._grid[(uint)(childrenGameObject.Position.x), (uint)(childrenGameObject.Position.y)].blockableObject != null)
                return false;
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
                        return false;
                    }
                }
            }
            return true;
        }

        //调整方块到合法位置
        //目前只能调整边长为1的方块
        //此函数内不可调用childrenGameObject的Position Setter，否则会触发死循环
        protected XYPosition CorrectPosition(GameObject childrenGameObject)
        {
            Debug("Correcting Position : " + childrenGameObject.ID + " : " + childrenGameObject.Position.ToString());

            if (CheckXYPosition(childrenGameObject))
                return childrenGameObject.Position;

            XYPosition result = new XYPosition((uint)(childrenGameObject.Position.x) + 0.5, (uint)(childrenGameObject.Position.y) + 0.5);
            if (result.x < 0.5)
                result.x = 0.5;
            else if (result.x > this._width - 0.5)
                result.x = this._width - 0.5;
            if (result.y < 0.5)
                result.y = 0.5;
            else if (result.y > this._height - 0.5)
                result.y = this._height - 0.5;

            GameObject testGameObject = new GameObject { Blockable = true, Position = result };
            for (int round = 1; round < Math.Max(this._width, this._height); round++)
            {
                for (testGameObject.Position.y = result.y - round; testGameObject.Position.y <= result.y + round; testGameObject.Position.y += 2 * round)
                    for (testGameObject.Position.x = result.x - round; testGameObject.Position.x <= result.x + round; testGameObject.Position.x++)
                    {
                        if (CheckXYPosition(testGameObject))
                            return testGameObject.Position;
                    }
                for (testGameObject.Position.x = result.x - round; testGameObject.Position.x <= result.x + round; testGameObject.Position.x += 2 * round)
                    for (testGameObject.Position.y = result.y - (round - 1); testGameObject.Position.y <= result.y + (round - 1); testGameObject.Position.y++)
                    {
                        if (CheckXYPosition(testGameObject))
                            return testGameObject.Position;
                    }
            }

            return new XYPosition(0, 0);//找不到合法位置，返回无效值

            //XYPosition[] searchers = new XYPosition[8];
            //for (int i = 0; i < (int)Direction.Size; i++)
            //{
            //    searchers[i] = new XYPosition(result.x, result.y);
            //}

            //while (true)
            //{
            //    for (int i = 0; i < (int)Direction.Size; i++)
            //    {
            //        searchers[i] = searchers[i] + 2 * EightCornerVector[(Direction)i];
            //    }
            //    foreach (var searcher in searchers)
            //    {
            //        Debug("searcher : " + searcher.ToString());
            //        if (CheckXYPosition(new GameObject { Blockable = true, Position = searcher }))
            //        {
            //            return searcher;
            //        }
            //    }
            //}
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
            Direction LeftOrRightBound = (angle >= Math.PI / 2 && angle < Math.PI * 3 / 2) ? Direction.Left : Direction.Right;
            Direction UpOrDownBound = (angle < Math.PI) ? Direction.Up : Direction.Down;

            List<double> LeftRightExtends = new List<double>();
            List<double> UpDownExtends = new List<double>();

            if (LeftOrRightBound == Direction.Left)
            {
                LeftRightExtends.Add(eOut.position.x - 0.5);
                LeftRightExtends.Add(childrenGameObject.Position.x - 0.5);
            }
            else
            {
                LeftRightExtends.Add(childrenGameObject.Position.x + 0.5);
                LeftRightExtends.Add(eOut.position.x + 0.5);
            }
            if (UpOrDownBound == Direction.Down)
            {
                UpDownExtends.Add(eOut.position.y - 0.5);
                UpDownExtends.Add(childrenGameObject.Position.y - 0.5);
            }
            else
            {
                UpDownExtends.Add(childrenGameObject.Position.y + 0.5);
                UpDownExtends.Add(eOut.position.y + 0.5);
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
                if (LeftOrRightBound == Direction.Left)
                {
                    double yDown = childrenGameObject.Position.y - 0.5 + (xToCheck - (childrenGameObject.Position.x - 0.5)) * Math.Tan(e.angle);
                    double yToCheck = (int)(yDown - 0.5) + 1 + 0.5;
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    foreach (var item in EightCornerVector)
                    {
                        toCheckPositions.Add(new Tuple<int, int>((int)(xToCheck + 2 * item.Value.x), (int)(yToCheck + 2 * item.Value.y)));
                    }
                }
                else
                {
                    double yDown = childrenGameObject.Position.y - 0.5 + (xToCheck - (childrenGameObject.Position.x + 0.5)) * Math.Tan(e.angle);
                    double yToCheck = (int)(yDown - 0.5) + 1 + 0.5;
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    foreach (var item in EightCornerVector)
                    {
                        toCheckPositions.Add(new Tuple<int, int>((int)(xToCheck + 2 * item.Value.x), (int)(yToCheck + 2 * item.Value.y)));
                    }
                }
            }
            for (double yToCheck = (int)(UpDownExtends[0] - 0.5) + 1 + 0.5; yToCheck < UpDownExtends[1]; yToCheck++)
            {
                if (UpOrDownBound == Direction.Down)
                {
                    double xLeft = childrenGameObject.Position.x - 0.5 + (yToCheck - (childrenGameObject.Position.y - 0.5)) / Math.Tan(e.angle);
                    double xToCheck = (int)(xLeft - 0.5) + 1 + 0.5;
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    foreach (var item in EightCornerVector)
                    {
                        toCheckPositions.Add(new Tuple<int, int>((int)(xToCheck + 2 * item.Value.x), (int)(yToCheck + 2 * item.Value.y)));
                    }
                }
                else
                {
                    double xLeft = childrenGameObject.Position.x - 0.5 + (yToCheck - (childrenGameObject.Position.y + 0.5)) / Math.Tan(e.angle);
                    double xToCheck = (int)(xLeft - 0.5) + 1 + 0.5;
                    toCheckPositions.Add(new Tuple<int, int>((int)xToCheck, (int)yToCheck));
                    foreach (var item in EightCornerVector)
                    {
                        toCheckPositions.Add(new Tuple<int, int>((int)(xToCheck + 2 * item.Value.x), (int)(yToCheck + 2 * item.Value.y)));
                    }
                }
            }

            double resultDistance = e.distance;
            Debug("initialize resultDistance : " + resultDistance);
            foreach (var toCheckPosition in toCheckPositions)
            {
                Debug("toCheckPosition : " + toCheckPosition.Item1 + " , " + toCheckPosition.Item2);
                double tempResultDistance = resultDistance;
                if (this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject != null
                    && this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject != childrenGameObject)
                {
                    double LeftRightDistance = resultDistance;
                    double UpDownDistance = resultDistance;
                    if (LeftOrRightBound == Direction.Left)
                    {
                        if (childrenGameObject.Position.x - 0.5 <= this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x + 0.5)
                        {
                            LeftRightDistance = 0;
                        }
                        else
                        {
                            LeftRightDistance = Math.Min(
                                ((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x + 0.5) - (childrenGameObject.Position.x - 0.5)) / Math.Cos(e.angle)
                                , LeftRightDistance);
                        }
                    }
                    else
                    {
                        if (childrenGameObject.Position.x + 0.5 >= this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x - 0.5)
                        {
                            LeftRightDistance = 0;
                        }
                        else
                        {
                            LeftRightDistance = Math.Min(
                                ((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.x - 0.5) - (childrenGameObject.Position.x + 0.5)) / Math.Cos(e.angle)
                                , LeftRightDistance);
                        }
                    }

                    if (UpOrDownBound == Direction.Down)
                    {
                        if (childrenGameObject.Position.y - 0.5 <= this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y + 0.5)
                        {
                            UpDownDistance = 0;
                        }
                        else
                        {
                            UpDownDistance = Math.Min(
                                ((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y + 0.5) - (childrenGameObject.Position.y - 0.5)) / Math.Sin(e.angle)
                                , UpDownDistance);
                        }
                    }
                    else
                    {
                        if (childrenGameObject.Position.y + 0.5 >= this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y - 0.5)
                        {
                            UpDownDistance = 0;
                        }
                        else
                        {
                            UpDownDistance = Math.Min(
                                ((this._grid[toCheckPosition.Item1, toCheckPosition.Item2].blockableObject.Position.y - 0.5) - (childrenGameObject.Position.y + 0.5)) / Math.Sin(e.angle)
                                , UpDownDistance);
                        }
                    }

                    tempResultDistance = Math.Max(LeftRightDistance, UpDownDistance);
                }
                if (tempResultDistance < resultDistance)
                    resultDistance = tempResultDistance;
            }

            Debug("resultDistance : " + resultDistance);
            eOut = new PositionChangeReturnEventArgs(true, childrenGameObject.Position + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle)));
            //double tempAngle = CorrectAngle(e.angle);
            //Direction direction = (Direction)(int)(tempAngle / (Math.PI / 4));
            //XYPosition aim = childrenGameObject.Position + e.distance * EightUnitVector[direction];
            //double minInterval = e.distance;
            ////Console.WriteLine("init minInterval : " + minInterval.ToString());
            //bool isSuccessful = true;
            //if (this._grid[(uint)aim.x, (uint)aim.y].blockableObject != null
            //    && this._grid[(uint)aim.x, (uint)aim.y].blockableObject != childrenGameObject)
            //{
            //    isSuccessful = false;
            //    if (Convert.ToBoolean((byte)direction & 1))
            //    {
            //        Direction[] boundDirection = new Direction[2] {
            //            (Direction)(((int)direction - 1) % 8),
            //            (Direction)(((int)direction + 1) % 8)
            //        };
            //        minInterval = Math.Max(
            //            ((this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[boundDirection[0]] - (childrenGameObject.Position + EightCornerVector[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
            //            ((this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[boundDirection[1]] - (childrenGameObject.Position + EightCornerVector[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
            //            );
            //    }
            //    else
            //    {
            //        minInterval = (this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[direction] - (childrenGameObject.Position + EightCornerVector[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
            //    }
            //}
            //else
            //{
            //    for (int i = 5; i <= 11; i++)
            //    {
            //        XYPosition toCheck = new XYPosition((uint)aim.x + 0.5, (uint)aim.y + 0.5) + EightUnitVector[(Direction)(((byte)direction + i) % 8)];
            //        if (this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject != null
            //            && this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject != childrenGameObject)
            //        {
            //            if (Math.Abs(this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position.x - aim.x) < 1
            //                && Math.Abs(this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position.y - aim.y) < 1)
            //            {
            //                isSuccessful = false;
            //                double tempMinInterval = 1;
            //                if (Convert.ToBoolean((byte)direction & 1))
            //                {
            //                    Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
            //                    tempMinInterval = Math.Max(
            //                        ((this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[boundDirection[0]] - (childrenGameObject.Position + EightCornerVector[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
            //                        ((this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[boundDirection[1]] - (childrenGameObject.Position + EightCornerVector[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
            //                        );
            //                }
            //                else
            //                {
            //                    //Console.WriteLine("direction : " + ((int)direction).ToString());
            //                    //Console.WriteLine("Block : " + this._grid[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.ToString());
            //                    //Console.WriteLine("Corner : " + EightCornerVector[direction].ToString());

            //                    tempMinInterval = (this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[direction] - (childrenGameObject.Position + EightCornerVector[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
            //                    //Console.WriteLine("tempInInterval : " + tempMinInterval.ToString());
            //                }
            //                if (tempMinInterval < minInterval)
            //                {
            //                    minInterval = tempMinInterval;
            //                }
            //            }
            //        }
            //    }
            //}

            //if (!isSuccessful)
            //{
            //    aim = (minInterval * 2) * EightCornerVector[direction] + childrenGameObject.Position;
            //}

            //if ((uint)childrenGameObject.Position.x != (uint)aim.x || (uint)childrenGameObject.Position.y != (uint)aim.y)
            //{
            //    this._grid[(uint)aim.x, (uint)aim.y].blockableObject = this._grid[(uint)childrenGameObject.Position.x, (uint)childrenGameObject.Position.y].blockableObject;
            //    this._grid[(uint)childrenGameObject.Position.x, (uint)childrenGameObject.Position.y].blockableObject = null;
            //    //Console.WriteLine("new self , " + xyPosition.ToString());
            //}
            //else
            //{
            //    childrenGameObject.Position = aim;
            //}
        }
    }
    public class MapCell
    {
        public GameObject? blockableObject = null;
        public List<GameObject> unblockableObjects = new List<GameObject>();
    }
}
