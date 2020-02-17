using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
                Debug(this, "Set Grid : " + value.GetLength(0).ToString() + " , " + value.GetLength(1).ToString());
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
            Debug(this, "new Map : " + width.ToString() + " , " + height.ToString());
            this.Width = width;
            this.Height = height;
            this.Grid = new MapCell[Width, Height];
        }

        protected override void OnChildrenAdded(GameObject childrenObject)
        {
            if (childrenObject.Layer >= _layerCount)
                childrenObject._layer = 0;
            childrenObject._position = CorrectPosition(childrenObject.Position, childrenObject.Width, childrenObject.Height, childrenObject.Layer);
            base.OnChildrenAdded(childrenObject);
            childrenObject.OnLayerChange += this.OnChildrenLayerChange;
            AddToGameObjectListByLayer(childrenObject);
            this.Grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].AddGameObject(childrenObject);
            Debug(this, "Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") add " + childrenObject.ID);
            TryToTrigger(childrenObject);
        }
        protected override void OnChildrenDelete(GameObject childrenObject)
        {
            base.OnChildrenDelete(childrenObject);
            childrenObject.OnLayerChange -= this.OnChildrenLayerChange;
            DeleteFromGameObjectListByLayer(childrenObject);
            _grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].DeleteGameObject(childrenObject);
        }
        protected override void OnChildrenPositionChanged(GameObject childrenGameObject, PositionChangedEventArgs e)
        {
            Debug(this, "Children object " + childrenGameObject.ID + " position change. from : " + e.previousPosition.ToString() + " aim : " + e.position.ToString());
            //base.OnChildrenPositionChanged(childrenGameObject, e, out eOut);

            if (XIsLegal((int)e.previousPosition.x) && YIsLegal((int)e.previousPosition.y))
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].DeleteGameObject(childrenGameObject);//如果需要把childrenGameObject从Grid上拿掉且可以拿掉
            childrenGameObject._position = CorrectPosition(e.position, childrenGameObject.Width, childrenGameObject.Height, childrenGameObject.Layer);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            TryToTrigger(childrenGameObject);
        }


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
        protected bool XYPositionIsLegal(XYPosition position, int objectWidth = 0, int objectHeight = 0, int objectLayer = 0)
        {
            DebugWithoutEndline(this, "Checking position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " layer : " + objectLayer);

            if (!XIsLegal(position.x, objectWidth) || !YIsLegal(position.y, objectHeight))
            {
                DebugWithoutID(this, "false");
                return false;
            }
            if (objectLayer >= _layerCount)
            {
                DebugWithoutID(this, "false");
                return true;
            }
            for (int x = (int)position.x - 1; x <= (int)position.x + 1; x++)
                for (int y = (int)position.y - 1; y <= (int)position.y + 1; y++)
                {
                    if (!XIsLegal(x) || !YIsLegal(y))
                    {
                        continue;
                    }
                    foreach (var layer in _layerCollisionMatrix[objectLayer][true])
                    {
                        if (!_grid[x, y].Layers.ContainsKey(layer))
                            continue;
                        foreach (var gameObject in _grid[x, y].Layers[layer].Keys)
                        {
                            if (Math.Abs(gameObject.Position.x - position.x) < 1
                                && Math.Abs(gameObject.Position.y - position.y) < 1)
                            {
                                DebugWithoutID(this, "false");
                                return false;
                            }
                        }
                    }
                }
            DebugWithoutID(this, "true");
            return true;
        }

        //调整方块到合法位置
        //目前只能调整边长为1的方块
        //此函数内不可调用childrenGameObject的Position Setter，否则会触发死循环
        //输入：一个GameObject（不一定是地图的子GameObject）
        protected XYPosition CorrectPosition(XYPosition position, int objectWidth = 1, int objectHeight = 1, int objectLayer = 0)
        {
            Debug(this, "Correcting Position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " layer : " + objectLayer);

            if (XYPositionIsLegal(position, objectWidth, objectHeight, objectLayer))
                return position;

            XYPosition newCenterPosition = position.GetMid();
            if (newCenterPosition.x < 0.5)
                newCenterPosition = new XYPosition(0.5, newCenterPosition.y);
            else if (newCenterPosition.x > (double)this._width - 0.5)
                newCenterPosition = new XYPosition((double)this._width - 0.5, newCenterPosition.y);
            if (newCenterPosition.y < 0.5)
                newCenterPosition = new XYPosition(newCenterPosition.x, 0.5);
            else if (newCenterPosition.y > (double)this._height - 0.5)
                newCenterPosition = new XYPosition(newCenterPosition.x, (double)this._height - 0.5);

            if (XYPositionIsLegal(newCenterPosition, objectWidth, objectHeight, objectLayer))
                return newCenterPosition;

            XYPosition testPosition = newCenterPosition;
            for (int round = 1; round < Math.Max(this._width, this._height); round++)
            {
                //Debug(this,"round : " + round);
                for (double ySearch = newCenterPosition.y - (double)round; ySearch <= newCenterPosition.y + (double)round + 0.1; ySearch += 2 * (double)round)
                {
                    if (!YIsLegal(ySearch))
                        continue;
                    for (double xSearch = newCenterPosition.x - (double)round; xSearch <= newCenterPosition.x + (double)round + 0.1; xSearch++)
                    {
                        if (!XIsLegal(xSearch))
                            continue;
                        testPosition = new XYPosition(xSearch, ySearch);
                        if (XYPositionIsLegal(testPosition, objectWidth, objectHeight, objectLayer))
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
                        if (XYPositionIsLegal(testPosition, objectWidth, objectHeight, objectLayer))
                            return testPosition;
                    }
                }
            }
            return new XYPosition(0, 0);//找不到合法位置，返回无效值
        }

        //为提高代码复用性，此函数内大量采用长度为2的数组表示X和Y，0表示X，1表示Y
        protected override void OnChildrenMove(GameObject childrenGameObject, MoveEventArgs e, XYPosition previousPosition)
        {
            Debug(this, "Attempting to move Children : " + childrenGameObject.ID);
            //base.OnChildrenMove(childrenGameObject, e, out eOut);
            //XYPosition aim;
            double resultDistance = e.distance;
            double[] delta = new double[2];
            delta[0] = e.distance * Math.Cos(e.angle);
            if (Math.Abs(delta[0]) < 1E-8)
                delta[0] = 0;
            delta[1] = e.distance * Math.Sin(e.angle);
            if (Math.Abs(delta[1]) < 1E-8)
                delta[1] = 0;
            bool[] IsDirectionPositive = { (delta[0] < 0) ? false : true, (delta[1] < 0) ? false : true };
            HashSet<GameObject>? CollisionGameObjects = null;
            SortedDictionary<double, HashSet<GameObject>> triggerDistanceAndGameObjects = new SortedDictionary<double, HashSet<GameObject>>();
            Direction collisionDirection = Direction.Size;
            void RefreshCollisionDirection(double[] Distance)
            {
                if (Distance[0] < Distance[1])
                {
                    collisionDirection = IsDirectionPositive[0] ? Direction.Right : Direction.Left;
                }
                else if (Distance[1] < Distance[0])
                {
                    collisionDirection = IsDirectionPositive[1] ? Direction.Up : Direction.Down;
                }
                else
                {
                    if (IsDirectionPositive[0])
                        collisionDirection = IsDirectionPositive[1] ? Direction.RightUp : Direction.RightDown;
                    else
                        collisionDirection = IsDirectionPositive[1] ? Direction.LeftUp : Direction.LeftDown;
                }
            }

            Debug(this, "initialize resultDistance : " + resultDistance);

            double ClosestGreater0_5(double d)
            {
                return (int)(d - 0.5) + 1.5;
            }
            double ClosestSmaller0_5(double d)
            {
                return Math.Ceiling(d + 0.5) - 1.5;
            }

            double[] initial = new double[2];
            double[] middleInitial = new double[2];
            double[] BeginToSearch = new double[2];
            for (int i = 0; i < 2; i++)
                if (IsDirectionPositive[i])
                {
                    initial[i] = previousPosition.GetProperty(i) - (((double)childrenGameObject.GetLength(i) / 2) + 1);
                    middleInitial[i] = initial[i] + 3;
                    BeginToSearch[i] = ClosestGreater0_5(initial[i]);
                }
                else
                {
                    initial[i] = previousPosition.GetProperty(i) + (((double)childrenGameObject.GetLength(i) / 2) + 1);
                    middleInitial[i] = initial[i] - 3;
                    BeginToSearch[i] = ClosestSmaller0_5(initial[i]);
                }
            double[] end = new double[2];
            double[] middleEnd = new double[2];
            double[] Search = new double[2];
            for (int i = 0; i < 2; i++)
            {
                end[i] = middleInitial[i] + delta[i];
                middleEnd[i] = initial[i] + delta[i];
                Search[i] = BeginToSearch[i];
            }

            void RefreshResultDistance(double newResultDistance)
            {
                if (newResultDistance >= resultDistance || newResultDistance < 0)
                {
                    return;
                }
                Debug(this, "Refresh resultDistance : " + newResultDistance);
                for (int i = 0; i < 2; i++)
                {
                    delta[i] = DivisionWithoutNaN(delta[i] * newResultDistance, resultDistance);
                    end[i] = middleInitial[i] + delta[i];
                    middleEnd[i] = initial[i] + delta[i];
                }
                resultDistance = newResultDistance;
            }

            {//这段代码检查是否准备移动到地图外，若是，把它移回地图内
                double[] aim = { previousPosition.x + delta[0], previousPosition.y + delta[1] };
                double[] Distance = { resultDistance, resultDistance };
                double[] HalfLength = { (double)childrenGameObject.Width / 2.0, (double)childrenGameObject.Height / 2.0 };
                for (int i = 0; i < 2; i++)
                    if (aim[i] < HalfLength[i])
                        Distance[i] = (HalfLength[i] - previousPosition.GetProperty(i)) / delta[i] * resultDistance;
                    else if (aim[i] > this.GetLength(i) - HalfLength[i])
                        Distance[i] = (this.GetLength(i) - HalfLength[i] - previousPosition.GetProperty(i)) / delta[i] * resultDistance;
                RefreshResultDistance(Math.Min(Distance[0], Distance[1]));
                RefreshCollisionDirection(Distance);
            }

            //======================
            //这段代码返回一个距离，childrenGameObject需要运行多远才能“碰到”gameObject
            double[] toCheckAdd = { IsDirectionPositive[0] ? -0.5 : 0.5, IsDirectionPositive[1] ? -0.5 : 0.5 };
            double[] startPointAdd = { -toCheckAdd[0], -toCheckAdd[1] };
            double[] infinityBound = { IsDirectionPositive[0] ? 1.0 / 0.0 : -1.0 / 0.0, IsDirectionPositive[1] ? 1.0 / 0.0 : -1.0 / 0.0 };
            double GameObjectMaxReachDistance(GameObject gameObject, ref double[] Distance)
            {
                Distance[0] = 1.0 / 0.0; Distance[1] = 1.0 / 0.0;
                double[] toCheck = new double[2];
                double[] startPoint = new double[2];

                for (int i = 0; i < 2; i++)//分别检查X和Y
                {
                    int j = Convert.ToInt32(!Convert.ToBoolean(i));
                    toCheck[i] = gameObject.Position.GetProperty(i) + toCheckAdd[i];
                    startPoint[i] = previousPosition.GetProperty(i) + startPointAdd[i];
                    if (IsInCloseOpenInterval(toCheck[i], startPoint[i], infinityBound[i]))
                    {
                        //Debug(this,"delta[" + i + "] : " + delta[i]);
                        double temp = previousPosition.GetProperty(j) - 0.5 + (toCheck[i] - startPoint[i]) * delta[j] / delta[i];
                        if (IsInOpenInterval(temp, gameObject.Position.GetProperty(j) - 0.5 - 1, gameObject.Position.GetProperty(j) + 0.5))
                        {
                            Distance[i] = (toCheck[i] - startPoint[i]) * resultDistance / delta[i];
                        }
                    }
                    //Debug(this,"Distance[" + i + "] : " + Distance[i]);
                }
                double maxReachDistance = Math.Min(Distance[0], Distance[1]);
                {
                    double[] coefficient = { (toCheck[0] - startPoint[0]) / delta[0], (toCheck[1] - startPoint[1]) / delta[1] };
                    if (double.IsNaN(coefficient[0]) && double.IsNaN(coefficient[1]))
                        maxReachDistance = 0;
                    else if (coefficient[0] == coefficient[1])
                        maxReachDistance = coefficient[0] * resultDistance;
                }
                Debug(this, "maxReachDistance : " + maxReachDistance);
                return maxReachDistance;
            }
            //=================

            void CheckBlockAndAdjustPosition(int x, int y)
            {
                if (!(XIsLegal(x) && YIsLegal(y)))
                    return;
                if (x != (int)childrenGameObject.Position.x || y != (int)childrenGameObject.Position.y)
                    foreach (var layer in _layerCollisionMatrix[childrenGameObject.Layer][true])
                    {
                        if (!_grid[x, y].ContainsLayer(layer))
                            continue;
                        foreach (var toCheckGameObject in _grid[x, y].Layers[layer].Keys)
                        {
                            double[] Distance = new double[2];
                            double maxReachDistance = GameObjectMaxReachDistance(toCheckGameObject, ref Distance);

                            //检查是否与toCheckGameObject碰撞，若是，更新resultDistance并获取碰撞方向
                            //当与多个GameObject碰撞时会出现不合逻辑的地方，暂时无法解决
                            if (maxReachDistance < resultDistance)
                            {
                                RefreshResultDistance(maxReachDistance);
                                RefreshCollisionDirection(Distance);
                                if (CollisionGameObjects == null)
                                    CollisionGameObjects = new HashSet<GameObject>();
                                CollisionGameObjects.Clear();
                                CollisionGameObjects.Add(toCheckGameObject);
                                //collisionDistance = resultDistance;
                            }
                            else if (maxReachDistance == resultDistance)
                            {
                                if (CollisionGameObjects != null)
                                    CollisionGameObjects.Add(toCheckGameObject);
                            }
                            //检查是否与toCheckGameObject碰撞，若是，更新resultDistance并获取碰撞方向

                        }
                    }
                foreach (var layer in _layerTriggerMatrix[childrenGameObject.Layer][true])
                {
                    if (!_grid[x, y].ContainsLayer(layer))
                        continue;
                    foreach (var gameObject in _grid[x, y].Layers[layer].Keys)
                    {
                        double[] Distance = new double[2];
                        double maxReachDistance;
                        if (Math.Abs(gameObject.Position.x - previousPosition.x) < 1.0
                            && Math.Abs(gameObject.Position.y - previousPosition.y) < 1.0)
                            maxReachDistance = 0;
                        else
                            maxReachDistance = GameObjectMaxReachDistance(gameObject, ref Distance);
                        if (maxReachDistance >= resultDistance)
                            continue;
                        if (!triggerDistanceAndGameObjects.ContainsKey(maxReachDistance))
                            triggerDistanceAndGameObjects.Add(maxReachDistance, new HashSet<GameObject>());
                        triggerDistanceAndGameObjects[maxReachDistance].Add(gameObject);
                    }
                }
            }

            {//这段代码检查是否准备移动到地图外，若是，把它移回地图内
                double[] aim = { previousPosition.x + delta[0], previousPosition.y + delta[1] };
                double[] Distance = { resultDistance, resultDistance };
                double[] HalfLength = { (double)childrenGameObject.Width / 2.0, (double)childrenGameObject.Height / 2.0 };
                for (int i = 0; i < 2; i++)
                    if (aim[i] < HalfLength[i])
                        Distance[i] = (HalfLength[i] - previousPosition.GetProperty(i)) / delta[i] * resultDistance;
                    else if (aim[i] > this.GetLength(i) - HalfLength[i])
                        Distance[i] = (this.GetLength(i) - HalfLength[i] - previousPosition.GetProperty(i)) / delta[i] * resultDistance;
                RefreshResultDistance(Math.Min(Distance[0], Distance[1]));
                RefreshCollisionDirection(Distance);
            }

            //搜索可能碰撞的GameObject
            //建议不要尝试读懂这段代码
            bool[] SearchCompleted = { false, false };
            Func<double, double, bool>[] Compare = new Func<double, double, bool>[2];
            Func<double, double, double>[] MinOrMax = new Func<double, double, double>[2];
            double[] Step = new double[2];
            for (int i = 0; i < 2; i++)
            {
                if (IsDirectionPositive[i])
                {
                    Compare[i] = (d1, d2) => { return d1 < d2; };
                    Step[i] = 1;
                    MinOrMax[i] = (d1, d2) => { return Math.Min(d1, d2); };
                }
                else
                {
                    Compare[i] = (d1, d2) => { return d1 > d2; };
                    Step[i] = -1;
                    MinOrMax[i] = (d1, d2) => { return Math.Max(d1, d2); };
                }
            };
            for (; ; )
            {
                for (int i = 0; i < 2; i++)
                {
                    int j = Convert.ToInt32(!Convert.ToBoolean(i));
                    if (Compare[i](Search[i], end[i]))
                    {
                        for (double z = Compare[i](Search[i], middleInitial[i]) ?
                                            BeginToSearch[j] :
                                            IsDirectionPositive[j] ?
                                                ClosestGreater0_5(initial[j] + DivisionWithoutNaN((Search[i] - middleInitial[i]) * delta[j], delta[i])) :
                                                ClosestSmaller0_5(initial[j] + DivisionWithoutNaN((Search[i] - middleInitial[i]) * delta[j], delta[i]));
                            Compare[j](z,
                                MinOrMax[j](BeginToSearch[j] + Step[j] * (Math.Abs(Search[i] - BeginToSearch[i]) + (i == 0 ? 0.1 : -0.1)),
                                            Compare[i](Search[i], middleEnd[i]) ?
                                        end[j] + DivisionWithoutNaN(Search[i] - middleEnd[i], delta[i]) * delta[j] :
                                        end[j]));
                             z = z + Step[j])
                        {
                            //Debug(this, "Checking : (" + (i == 0 ? Search[i] : z) + "," + (i == 0 ? z : Search[i]) + ")");
                            CheckBlockAndAdjustPosition(i == 0 ? (int)Search[i] : (int)z, i == 0 ? (int)z : (int)Search[i]);
                        }
                        Search[i] = Search[i] + Step[i];
                    }
                    else
                        SearchCompleted[i] = true;
                }
                if (SearchCompleted[0] && SearchCompleted[1])
                    break;
            }
            //搜索可能碰撞的GameObject
            //建议不要尝试读懂这段代码

            Debug(this, "resultDistance : " + resultDistance);
            childrenGameObject._position = previousPosition + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle));
            this._grid[(int)previousPosition.x, (int)previousPosition.y].DeleteGameObject(childrenGameObject);
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);

            //Collide
            if (resultDistance < e.distance)
            {
                childrenGameObject.Collide(new CollisionEventArgs(collisionDirection, CollisionGameObjects));
                if (CollisionGameObjects != null)
                    foreach (var gameObject in CollisionGameObjects)
                    {
                        gameObject.Collide(new CollisionEventArgs((Direction)(((int)collisionDirection + 4) % 8), new HashSet<GameObject> { childrenGameObject }));
                    }
            }
            //Collide End

            //Trigger
            HashSet<GameObject> triggerGameObjects = new HashSet<GameObject>();
            foreach (var item in triggerDistanceAndGameObjects)
            {
                if (item.Key >= resultDistance)
                    break;
                foreach (var gameObject in item.Value)
                {
                    triggerGameObjects.Add(gameObject);
                    gameObject.Trigger(new HashSet<GameObject> { childrenGameObject });
                }
            }
            childrenGameObject.Trigger(triggerGameObjects);
            //Trigger End

            //Check Bouncable
            //检查是否可反弹
            if (childrenGameObject.Bouncable && resultDistance < e.distance)
            {
                double bounceAngle;
                switch (collisionDirection)
                {
                    case Direction.Left:
                    case Direction.Right: bounceAngle = Math.PI - e.angle; break;
                    case Direction.Up:
                    case Direction.Down: bounceAngle = -e.angle; break;
                    default: bounceAngle = Math.PI + e.angle; break;
                }
                childrenGameObject.Move(new MoveEventArgs(bounceAngle, e.distance - resultDistance));
                childrenGameObject.Velocity = new Vector(bounceAngle, childrenGameObject.Velocity.length);
            }
            //Check Bouncable End
        }

        //Layer
        protected int _layerCount = 1;
        public int LayerCount
        {
            get { return this._layerCount; }
            set
            {
                Debug(this, "set LayCount to " + LayerCount);
                if (value > this._layerCount)
                {
                    for (int i = this._layerCount; i < value; i++)
                        AddLayer();
                }
                else if (value < this._layerCount)
                {
                    for (int i = value; i < this._layerCount; i++)
                        DeleteLayer();
                }
            }
        }
        public void AddLayer()
        {
            Debug(this, "Add Layer");
            this._layerCollisionMatrix.Add(this._layerCount, new Dictionary<bool, HashSet<int>>
            {
                { true, new HashSet<int>() },
                { false, new HashSet<int>() }
            });
            this._layerTriggerMatrix.Add(this._layerCount, new Dictionary<bool, HashSet<int>>
            {
                { true, new HashSet<int>() },
                { false, new HashSet<int>() }
            });
            for (int i = 0; i < this._layerCount; i++)
            {
                this._layerCollisionMatrix[this._layerCount][false].Add(i);
                this._layerTriggerMatrix[this._layerCount][false].Add(i);
            }
            for (int i = 0; i <= this._layerCount; i++)
            {
                this._layerCollisionMatrix[i][false].Add(this._layerCount);
                this._layerTriggerMatrix[i][false].Add(this._layerCount);
            }
            this._layerCount++;
        }
        public void DeleteLayer()
        {
            foreach (var gameObject in new HashSet<GameObject>(_gameObjectListByLayer[this._layerCount - 1]))
            {
                gameObject.Parent = null;
            }
            _layerCollisionMatrix.Remove(this._layerCount - 1);
            _layerTriggerMatrix.Remove(this._layerCount - 1);
            for (int i = 0; i < this._layerCount - 1; i++)
            {
                _layerCollisionMatrix[i][true].Remove(this._layerCount - 1);
                _layerCollisionMatrix[i][false].Remove(this._layerCount - 1);
                _layerTriggerMatrix[i][true].Remove(this._layerCount - 1);
                _layerTriggerMatrix[i][false].Remove(this._layerCount - 1);
            }
            _layerCount--;
        }
        protected Dictionary<int, Dictionary<bool, HashSet<int>>> _layerCollisionMatrix = new Dictionary<int, Dictionary<bool, HashSet<int>>>
        {
            { 0, new Dictionary<bool, HashSet<int>>{
                { true, new HashSet<int>() },
                { false, new HashSet<int>{ 0 } }
            } }
        };
        protected Dictionary<int, Dictionary<bool, HashSet<int>>> _layerTriggerMatrix = new Dictionary<int, Dictionary<bool, HashSet<int>>>
        {
            { 0, new Dictionary<bool, HashSet<int>>{
                { true, new HashSet<int>() },
                { false, new HashSet<int>{ 0 } }
            } }
        };
        public bool SetLayerCollisionTrue(int layer1, int layer2)
        {
            if (layer1 >= this._layerCount || layer2 >= this._layerCount)
                return false;
            Debug(this, "Enable " + layer1 + " and " + layer2 + " collision");
            this._layerCollisionMatrix[layer1][false].Remove(layer2);
            this._layerCollisionMatrix[layer1][true].Add(layer2);
            this._layerCollisionMatrix[layer2][false].Remove(layer1);
            this._layerCollisionMatrix[layer2][true].Add(layer1);
            if (!_gameObjectListByLayer.ContainsKey(layer2))
                return true;
            foreach (var gameObject in new HashSet<GameObject>(_gameObjectListByLayer[layer2]))
            {
                gameObject.Layer = gameObject.Layer;
            }
            return true;
        }
        public bool SetLayerCollisionFalse(int layer1, int layer2)
        {
            if (layer1 >= this._layerCount || layer2 >= this._layerCount)
                return false;
            Debug(this, "Disable " + layer1 + " and " + layer2 + " collision");
            this._layerCollisionMatrix[layer1][true].Remove(layer2);
            this._layerCollisionMatrix[layer1][false].Add(layer2);
            this._layerCollisionMatrix[layer2][true].Remove(layer1);
            this._layerCollisionMatrix[layer2][false].Add(layer1);
            return true;
        }
        public bool SetLayerTriggerTrue(int layer1, int layer2)
        {
            if (layer1 >= this._layerCount || layer2 >= this._layerCount)
                return false;
            Debug(this, "Enable " + layer1 + " and " + layer2 + " trigger");
            this._layerTriggerMatrix[layer1][false].Remove(layer2);
            this._layerTriggerMatrix[layer1][true].Add(layer2);
            this._layerTriggerMatrix[layer2][false].Remove(layer1);
            this._layerTriggerMatrix[layer2][true].Add(layer1);
            if (!_gameObjectListByLayer.ContainsKey(layer2))
                return true;
            foreach (var gameObject in new HashSet<GameObject>(_gameObjectListByLayer[layer2]))
            {
                gameObject.Layer = gameObject.Layer;
            }
            return true;
        }
        public bool SetLayerTriggerFalse(int layer1, int layer2)
        {
            if (layer1 >= this._layerCount || layer2 >= this._layerCount)
                return false;
            Debug(this, "Disable " + layer1 + " and " + layer2 + " collision");
            this._layerTriggerMatrix[layer1][true].Remove(layer2);
            this._layerTriggerMatrix[layer1][false].Add(layer2);
            this._layerTriggerMatrix[layer2][true].Remove(layer1);
            this._layerTriggerMatrix[layer2][false].Add(layer1);
            return true;
        }

        protected Dictionary<int, HashSet<GameObject>> _gameObjectListByLayer = new Dictionary<int, HashSet<GameObject>> { { 0, new HashSet<GameObject>() } };
        protected void AddToGameObjectListByLayer(GameObject gameObject)
        {
            if (!_gameObjectListByLayer.ContainsKey(gameObject.Layer))
                _gameObjectListByLayer.Add(gameObject.Layer, new HashSet<GameObject>());
            _gameObjectListByLayer[gameObject.Layer].Add(gameObject);
        }
        protected void DeleteFromGameObjectListByLayer(GameObject gameObject)
        {
            if (!_gameObjectListByLayer.ContainsKey(gameObject.Layer))
                return;
            _gameObjectListByLayer[gameObject.Layer].Remove(gameObject);
            if (_gameObjectListByLayer[gameObject.Layer].Count <= 0)
                _gameObjectListByLayer.Remove(gameObject.Layer);
        }
        protected void OnChildrenLayerChange(GameObject childrenGameObject, LayerChangedEventArgs e)
        {
            if (e.layer >= _layerCount)
            {
                childrenGameObject._layer = e.previousLayer;
                return;
            }
            DeleteFromGameObjectListByLayer(childrenGameObject);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].DeleteGameObject(childrenGameObject);
            childrenGameObject._layer = e.layer;
            childrenGameObject._position = CorrectPosition(childrenGameObject.Position, childrenGameObject.Width, childrenGameObject.Height, e.layer);
            AddToGameObjectListByLayer(childrenGameObject);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            TryToTrigger(childrenGameObject);
            return;
        }
        //Layer

        protected void TryToTrigger(GameObject childrenGameObject)
        {
            HashSet<GameObject> triggerGameObjects = new HashSet<GameObject>();
            for (int x = (int)childrenGameObject.Position.x - 1; x <= (int)childrenGameObject.Position.x + 1; x++)
                for (int y = (int)childrenGameObject.Position.y - 1; y <= (int)childrenGameObject.Position.y + 1; y++)
                {
                    if (!(XIsLegal(x) && YIsLegal(y)))
                        continue;
                    foreach (var layer in _layerTriggerMatrix[childrenGameObject.Layer][true])
                    {
                        if (!_grid[x, y].ContainsLayer(layer))
                            continue;
                        foreach (var gameObject in _grid[x, y].Layers[layer].Keys)
                        {
                            if (gameObject == childrenGameObject)
                                continue;
                            if (Math.Abs(gameObject.Position.x - childrenGameObject.Position.x) < 1.0
                                && Math.Abs(gameObject.Position.y - childrenGameObject.Position.y) < 1.0)
                                triggerGameObjects.Add(gameObject);
                        }
                    }
                }
            childrenGameObject.Trigger(triggerGameObjects);
            foreach (var gameObject in triggerGameObjects)
            {
                gameObject.Trigger(new HashSet<GameObject> { childrenGameObject });
            }

        }

        public void PrintGrid()
        {
            Console.WriteLine("=========== " + this.GetType() + " : " + this.ID + " Grid =========");
            for (int i = 0; i < this.Grid.GetLength(0); i++)
                for (int j = 0; j < this.Grid.GetLength(1); j++)
                {
                    if (!this.Grid[i, j].IsEmpty())
                    {
                        Console.Write("({0},{1}) : ", i, j);
                        foreach (var layer in this.Grid[i, j].Layers)
                        {
                            Console.Write("Layer:" + layer.Key + " ");
                            foreach (var gameObject in layer.Value.Keys)
                            {
                                Console.Write(gameObject.ID + ":(" + gameObject.Position.x + "," + gameObject.Position.y + ") ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            Console.WriteLine("=======================================");

        }
    }
}
