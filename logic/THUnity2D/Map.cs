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
        public MapCell[,] Grid { get { return _grid; } }

        public readonly int FrameRate;
        public readonly int TimeInterval;

        public Map(int width, int height, int frameRate = 50) : base(width, height, null)
        {
            Console.WriteLine("new Map");
            this.FrameRate = frameRate;
            this.TimeInterval = 1 / this.FrameRate;
            _grid = new MapCell[Width, Height];
        }

        protected override void OnChildrenAdded(GameObject childrenObject)
        {
            base.OnChildrenAdded(childrenObject);
            childrenObject.Position = CorrectPosition(childrenObject.Position);
        }
        protected override void OnChildrenDelete(GameObject childrenObject)
        {
            base.OnChildrenDelete(childrenObject);
            if(childrenObject.Blockable)
            {
                _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].blockableObject = null;
            }
            else
            {
                _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].unblockableObjects.Remove(childrenObject);
            }
        }
        protected override void OnChildrenPositionChanged(GameObject childrenGameObject, PositionChangedEventArgs e)
        {
            base.OnChildrenPositionChanged(childrenGameObject, e);
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].blockableObject = null;
            if (!CheckXYPosition(e.position))
            {
                childrenGameObject.Position = CorrectPosition(e.position);
                this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].blockableObject = childrenGameObject;
            }
            else
            {
                this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].blockableObject = childrenGameObject;
            }
        }

        protected bool CheckXYPosition(XYPosition toCheck)
        {
            if (toCheck.x - 0.5 < 0 || toCheck.x + 0.5 > this._width || toCheck.y - 0.5 < 0 || toCheck.y + 0.5 > this._height)
                return false;
            else if (this._grid[(uint)(toCheck.x), (uint)(toCheck.y)].blockableObject != null)
                return false;
            XYPosition centerPosition = new XYPosition(Convert.ToInt32(toCheck.x) + 0.5, Convert.ToInt32(toCheck.y) + 0.5);
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
                    //Console.WriteLine("not null");
                    if (Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].blockableObject.Position.x - toCheck.x) < 1
                        && Math.Abs(_grid[(uint)toCheckPosition.x, (uint)toCheckPosition.y].blockableObject.Position.y - toCheck.y) < 1)
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        protected XYPosition CorrectPosition(XYPosition toCorrectPosition)
        {
            XYPosition result = new XYPosition((uint)(toCorrectPosition.x), (uint)(toCorrectPosition.y));
            if (result.x < 0.5)
                result.x = 0.5;
            else if (result.x > this._width - 0.5)
                result.x = this._width - 0.5;
            if (result.y < 0.5)
                result.y = 0.5;
            else if (result.y > this._height - 0.5)
                result.y = this._height - 0.5;

            if (CheckXYPosition(result))
                return result;

            XYPosition[] searchers = new XYPosition[8];
            for (int i = 0; i < (int)Direction.Size; i++)
            {
                searchers[i] = new XYPosition(result.x, result.y);
            }

            while (true)
            {
                for (int i = 0; i < (int)Direction.Size; i++)
                {
                    searchers[i] = searchers[i] + 2 * EightCornerVector[(Direction)i];
                }
                foreach (var searcher in searchers)
                {
                    if (CheckXYPosition(searcher))
                    {
                        return searcher;
                    }
                }
            }
        }
        protected override void OnChildrenMove(GameObject childrenGameObject, MoveEventArgs e, out bool isMoved)
        {
            base.OnChildrenMove(childrenGameObject, e, out isMoved);

            //double angle = CorrectAngle(e.angle);
            //Direction LeftRightBound = (angle > Math.PI / 2 && angle < Math.PI * 3 / 2) ? Direction.Left : Direction.Right;
            //Direction UpDownBound = (angle < Math.PI) ? Direction.Up : Direction.Down;

            //SortedSet<double> LeftRightExtends = new SortedSet<double>();
            //SortedSet<double> UpDownExtends = new SortedSet<double>();

            //LeftRightExtends.Add()

            double tempAngle = CorrectAngle(e.angle);
            Direction direction = (Direction)(int)(tempAngle / (Math.PI / 4));
            XYPosition aim = childrenGameObject.Position + e.distance * EightUnitVector[direction];
            double minInterval = e.distance;
            //Console.WriteLine("init minInterval : " + minInterval.ToString());
            bool isSuccessful = true;
            if (this._grid[(uint)aim.x, (uint)aim.y].blockableObject != null
                && this._grid[(uint)aim.x, (uint)aim.y].blockableObject != childrenGameObject)
            {
                isSuccessful = false;
                if (Convert.ToBoolean((byte)direction & 1))
                {
                    Direction[] boundDirection = new Direction[2] {
                        (Direction)(((int)direction - 1) % 8),
                        (Direction)(((int)direction + 1) % 8)
                    };
                    minInterval = Math.Max(
                        ((this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[boundDirection[0]] - (childrenGameObject.Position + EightCornerVector[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
                        ((this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[boundDirection[1]] - (childrenGameObject.Position + EightCornerVector[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
                        );
                }
                else
                {
                    minInterval = (this._grid[(uint)aim.x, (uint)aim.y].blockableObject.Position - EightCornerVector[direction] - (childrenGameObject.Position + EightCornerVector[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
                }
            }
            else
            {
                for (int i = 5; i <= 11; i++)
                {
                    XYPosition toCheck = new XYPosition((uint)aim.x + 0.5, (uint)aim.y + 0.5) + EightUnitVector[(Direction)(((byte)direction + i) % 8)];
                    if (this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject != null
                        && this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject != childrenGameObject)
                    {
                        if (Math.Abs(this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position.x - aim.x) < 1
                            && Math.Abs(this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position.y - aim.y) < 1)
                        {
                            isSuccessful = false;
                            double tempMinInterval = 1;
                            if (Convert.ToBoolean((byte)direction & 1))
                            {
                                Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
                                tempMinInterval = Math.Max(
                                    ((this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[boundDirection[0]] - (childrenGameObject.Position + EightCornerVector[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
                                    ((this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[boundDirection[1]] - (childrenGameObject.Position + EightCornerVector[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
                                    );
                            }
                            else
                            {
                                //Console.WriteLine("direction : " + ((int)direction).ToString());
                                //Console.WriteLine("Block : " + this._grid[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.ToString());
                                //Console.WriteLine("Corner : " + EightCornerVector[direction].ToString());

                                tempMinInterval = (this._grid[(uint)toCheck.x, (uint)toCheck.y].blockableObject.Position - EightCornerVector[direction] - (childrenGameObject.Position + EightCornerVector[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
                                //Console.WriteLine("tempInInterval : " + tempMinInterval.ToString());
                            }
                            if (tempMinInterval < minInterval)
                            {
                                minInterval = tempMinInterval;
                            }
                        }
                    }
                }
            }

            if (!isSuccessful)
            {
                aim = (minInterval * 2) * EightCornerVector[direction] + childrenGameObject.Position;
            }

            if ((uint)childrenGameObject.Position.x != (uint)aim.x || (uint)childrenGameObject.Position.y != (uint)aim.y)
            {
                this._grid[(uint)aim.x, (uint)aim.y].blockableObject = this._grid[(uint)childrenGameObject.Position.x, (uint)childrenGameObject.Position.y].blockableObject;
                this._grid[(uint)childrenGameObject.Position.x, (uint)childrenGameObject.Position.y].blockableObject = null;
                //Console.WriteLine("new self , " + xyPosition.ToString());
            }
            else
            {
                childrenGameObject.Position = aim;
            }
        }

    }
    public class MapCell
    {
        public GameObject blockableObject = null;
        public List<GameObject> unblockableObjects = new List<GameObject>();
    }
}
