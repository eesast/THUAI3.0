using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
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

        protected override void OnChildrenAdded(GameObject childrenGameObject)
        {
            if (!this._layerSet.ContainsKey(childrenGameObject.Layer))
                throw new Exception("Map does not contains layer !");
            childrenGameObject.Position = CorrectPosition(childrenGameObject.Position, childrenGameObject.Width, childrenGameObject.Height, childrenGameObject.Layer);
            base.OnChildrenAdded(childrenGameObject);
            childrenGameObject.OnLayerChange += this.OnChildrenLayerChange;
            //AddToGameObjectListByLayer(childrenObject);
            childrenGameObject.Layer.GameObjectList.TryAdd(childrenGameObject, 0);
            lock (_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock)
                this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            Debug(this, "Grid (" + (int)childrenGameObject.Position.x + "," + (int)childrenGameObject.Position.y + ") add " + childrenGameObject.ID);
            TryToTrigger(childrenGameObject);
        }
        protected override void OnChildrenDelete(GameObject childrenGameObject)
        {
            base.OnChildrenDelete(childrenGameObject);
            childrenGameObject.OnLayerChange -= this.OnChildrenLayerChange;
            //DeleteFromGameObjectListByLayer(childrenObject);
            byte temp = 0;
            childrenGameObject.Layer.GameObjectList.TryRemove(childrenGameObject, out temp);
            lock (_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock)
                _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].DeleteGameObject(childrenGameObject);
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
        public bool XYPositionIsLegal(XYPosition position, int objectWidth = 0, int objectHeight = 0, Layer? objectLayer = null)
        {
            DebugWithoutEndline(this, "Checking position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " layer : " + objectLayer);

            if (!XIsLegal(position.x, objectWidth) || !YIsLegal(position.y, objectHeight))
            {
                DebugWithoutID(this, "false");
                return false;
            }
            if (objectLayer != null && !_layerSet.ContainsKey(objectLayer))
                throw new Exception("Does not contain layer");
            for (int x = (int)position.x - 1; x <= (int)position.x + 1; x++)
                for (int y = (int)position.y - 1; y <= (int)position.y + 1; y++)
                {
                    if (!XIsLegal(x) || !YIsLegal(y))
                    {
                        continue;
                    }
                    if (objectLayer != null)
                        foreach (var layer in objectLayer.CollisionLayers.Keys)
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
        protected XYPosition CorrectPosition(XYPosition position, int objectWidth = 1, int objectHeight = 1, Layer? objectLayer = null)
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
                    if (IsInCloseOpenInterval(toCheck[i], startPoint[i], infinityBound[i])
                        && delta[i] != 0)//如果平行四边形是一条线，就跳过
                    {
                        //Debug(this,"delta[" + i + "] : " + delta[i]);
                        double temp = previousPosition.GetProperty(j) - 0.5 + (toCheck[i] - startPoint[i]) * delta[j] / delta[i];
                        //Debug(this, "temp : " + temp);
                        if (IsInOpenInterval(temp, gameObject.Position.GetProperty(j) - 0.5 - 1, gameObject.Position.GetProperty(j) + 0.5))
                        {
                            //Debug(this, "!");
                            Distance[i] = (toCheck[i] - startPoint[i]) * resultDistance / delta[i];
                        }
                    }
                    //Debug(this, "Distance[" + i + "] : " + Distance[i]);
                }
                double maxReachDistance = Math.Min(Distance[0], Distance[1]);
                {
                    //Debug(this, "toCheck[0]=" + toCheck[0]);
                    //Debug(this, "startPoint[0]=" + startPoint[0]);
                    //Debug(this, "delta[0]=" + delta[0]);
                    //Debug(this, "toCheck[1]=" + toCheck[1]);
                    //Debug(this, "startPoint[1]=" + startPoint[1]);
                    //Debug(this, "delta[1]=" + delta[1]);
                    if (Math.Abs((toCheck[0] - startPoint[0]) * delta[1] - (toCheck[1] - startPoint[1]) * delta[0]) < 1E-10)
                    {
                        if (Math.Abs(toCheck[0] - startPoint[0]) < 1E-10 && Math.Abs(toCheck[1] - startPoint[1]) < 1E-10
                            && delta[0] != 0 && delta[1] != 0)
                            maxReachDistance = 0;
                        else if (Math.Abs(toCheck[0] - startPoint[0]) >= 1E-10 && Math.Abs(toCheck[1] - startPoint[1]) >= 1E-10)
                            maxReachDistance = resultDistance * (toCheck[0] - startPoint[0]) / delta[0];
                    }
                }
                Debug(this, "maxReachDistance : " + maxReachDistance);
                return maxReachDistance;
            }
            //=================


            //核心代码，检查(x,y)位置的方块与childrenGameObject的关系，并检查碰撞
            LinkedList<object> lockList = new LinkedList<object>();
            void CheckBlockAndAdjustPosition(int x, int y)
            {
                if (!(XIsLegal(x) && YIsLegal(y)))
                    return;
                Monitor.Enter(_grid[x, y].publicLock);
                lockList.AddLast(_grid[x, y].publicLock);
                if (x != (int)previousPosition.x || y != (int)previousPosition.y)
                    foreach (var layer in childrenGameObject.Layer.CollisionLayers.Keys)
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
                foreach (var layer in childrenGameObject.Layer.TriggerLayers.Keys)
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
                            Debug(this, "Checking : (" + (i == 0 ? Search[i] : z) + "," + (i == 0 ? z : Search[i]) + ")");
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

            //调整位置
            Debug(this, "resultDistance : " + resultDistance);
            childrenGameObject._position = previousPosition + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle));
            this._grid[(int)previousPosition.x, (int)previousPosition.y].DeleteGameObject(childrenGameObject);
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            //调整位置 End

            //解除锁的占用
            while (lockList.First != null)
            {
                object o = lockList.First.Value;
                lockList.RemoveFirst();
                Monitor.Exit(o);
            }
            //解除锁的占用 End

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

            //Collide
            if (resultDistance < e.distance)
            {
                childrenGameObject.Collide(new CollisionEventArgs(collisionDirection, CollisionGameObjects));
                if (CollisionGameObjects != null)
                    foreach (var gameObject in CollisionGameObjects)
                    {
                        gameObject.Collide(new CollisionEventArgs((Direction)(((int)collisionDirection + 4) % 8), new HashSet<GameObject> { childrenGameObject }));
                    }

                //Check Bouncable
                //检查是否可反弹
                if (childrenGameObject.Bouncable)
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
                else
                {
                    if (childrenGameObject.Velocity.length > 0)
                        childrenGameObject.Velocity = new Vector(e.angle, 0);
                }
                //Check Bouncable End

            }
            //Collide End

        }

        //Layer
        public int LayerCount
        {
            get { return _layerSet.Count; }
        }
        public Layer AddLayer()
        {
            Debug(this, "Add Layer");
            Layer layer = new Layer();
            _layerSet.TryAdd(layer, 0);
            return layer;
        }
        public void DeleteLayer(Layer layer)
        {
            if (!_layerSet.ContainsKey(layer))
                return;
            Debug(this, "Remove Layer");
            foreach (var gameObject in new HashSet<GameObject>(layer.GameObjectList.Keys))
            {
                gameObject.Parent = null;
            }
            foreach (var l in _layerSet.Keys)
            {
                byte tmp = 0;
                l.CollisionLayers.TryRemove(layer, out tmp);
                l.TriggerLayers.TryRemove(layer, out tmp);
            }
            byte temp = 0;
            _layerSet.TryRemove(layer, out temp);
        }
        protected ConcurrentDictionary<Layer, byte> _layerSet = new ConcurrentDictionary<Layer, byte>();
        public void SetLayerCollisionTrue(Layer layer1, Layer layer2)
        {
            if (!_layerSet.ContainsKey(layer1) || !_layerSet.ContainsKey(layer2))
                throw new Exception("Does not contain layer");
            Debug(this, "Enable " + layer1 + " and " + layer2 + " collision");
            layer1.CollisionLayers.TryAdd(layer2, 0);
            layer2.CollisionLayers.TryAdd(layer1, 0);
            foreach (var gameObject in new HashSet<GameObject>(layer2.GameObjectList.Keys))
            {
                gameObject.Layer = gameObject.Layer;
            }
        }
        public void SetLayerCollisionFalse(Layer layer1, Layer layer2)
        {
            if (!_layerSet.ContainsKey(layer1) || !_layerSet.ContainsKey(layer2))
                throw new Exception("Does not contain layer");
            Debug(this, "Disable " + layer1 + " and " + layer2 + " collision");
            byte temp = 0;
            layer1.CollisionLayers.TryRemove(layer2, out temp);
            layer2.CollisionLayers.TryRemove(layer1, out temp);
        }
        public void SetLayerTriggerTrue(Layer layer1, Layer layer2)
        {
            if (!_layerSet.ContainsKey(layer1) || !_layerSet.ContainsKey(layer2))
                throw new Exception("Does not contain layer");
            Debug(this, "Enable " + layer1 + " and " + layer2 + " trigger");
            layer1.TriggerLayers.TryAdd(layer2, 0);
            layer2.TriggerLayers.TryAdd(layer1, 0);
            foreach (var gameObject in new HashSet<GameObject>(layer2.GameObjectList.Keys))
            {
                gameObject.Layer = gameObject.Layer;
            }
        }
        public void SetLayerTriggerFalse(Layer layer1, Layer layer2)
        {
            if (!_layerSet.ContainsKey(layer1) || !_layerSet.ContainsKey(layer2))
                throw new Exception("Does not contain layer");
            Debug(this, "Disable " + layer1 + " and " + layer2 + " collision");
            byte temp = 0;
            layer1.TriggerLayers.TryRemove(layer2, out temp);
            layer2.TriggerLayers.TryRemove(layer1, out temp);
        }

        protected void OnChildrenLayerChange(GameObject childrenGameObject, LayerChangeEventArgs e)
        {
            childrenGameObject._layer = e.previousLayer;
            byte temp = 0;
            childrenGameObject.Layer.GameObjectList.TryRemove(childrenGameObject, out temp);
            //DeleteFromGameObjectListByLayer(childrenGameObject);
            Monitor.Enter(_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].DeleteGameObject(childrenGameObject);
            Monitor.Exit(_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock);
            childrenGameObject._layer = e.targetLayer;
            childrenGameObject._position = CorrectPosition(childrenGameObject.Position, childrenGameObject.Width, childrenGameObject.Height, e.targetLayer);
            childrenGameObject.Layer.GameObjectList.TryAdd(childrenGameObject, 0);
            //AddToGameObjectListByLayer(childrenGameObject);
            Monitor.Enter(_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            Monitor.Exit(_grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].publicLock);
            TryToTrigger(childrenGameObject);
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
                    foreach (var layer in childrenGameObject.Layer.TriggerLayers.Keys)
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
