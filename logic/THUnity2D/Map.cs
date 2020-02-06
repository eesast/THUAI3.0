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
            if (!_layerCollisionMatrix.ContainsKey(childrenObject.Layer))
                childrenObject._layer = 0;
            childrenObject._position = CorrectPosition(childrenObject.Position, childrenObject.Width, childrenObject.Height, childrenObject.Layer);
            base.OnChildrenAdded(childrenObject);
            childrenObject.OnLayerChange += this.OnChildrenLayerChange;
            AddToGameObjectListByLayer(childrenObject);
            this.Grid[(int)childrenObject.Position.x, (int)childrenObject.Position.y].AddGameObject(childrenObject);
            this.Debug("Grid (" + (int)childrenObject.Position.x + "," + (int)childrenObject.Position.y + ") add " + childrenObject.ID);
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
            this.Debug("Children object " + childrenGameObject.ID + " position change. from : " + e.previousPosition.ToString() + " aim : " + e.position.ToString());
            //base.OnChildrenPositionChanged(childrenGameObject, e, out eOut);

            if (XIsLegal((int)e.previousPosition.x) && YIsLegal((int)e.previousPosition.y))
                this._grid[(int)e.previousPosition.x, (int)e.previousPosition.y].DeleteGameObject(childrenGameObject);//如果需要把childrenGameObject从Grid上拿掉且可以拿掉
            childrenGameObject._position = CorrectPosition(e.position, childrenGameObject.Width, childrenGameObject.Height, childrenGameObject.Layer);
            _grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);
            return;
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
            this.DebugWithoutEndline("Checking position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " layer : " + objectLayer);

            if (!XIsLegal(position.x, objectWidth) || !YIsLegal(position.y, objectHeight))
            {
                this.DebugWithoutID("false");
                return false;
            }
            if (!this._layerCollisionMatrix.ContainsKey(objectLayer))
            {
                this.DebugWithoutID("false");
                return true;
            }
            foreach (var layer in this._layerCollisionMatrix[objectLayer][true])
            {
                if (this._grid[(uint)(position.x), (uint)(position.y)].Layers.ContainsKey(layer))
                {
                    this.DebugWithoutID("false");
                    return false;
                }
            }
            XYPosition centerPosition = new XYPosition((int)(position.x) + 0.5, (int)(position.y) + 0.5);
            for (int i = 0; i < (int)(Direction.Size); i++)
            {
                XYPosition toCheckPosition = centerPosition + EightUnitVector[(Direction)i];
                //Console.WriteLine("corner : " + item.Key.ToString() + " , " + toCheckPosition.ToString());
                if (!XIsLegal((int)toCheckPosition.x) || !YIsLegal((int)toCheckPosition.y))
                {
                    continue;
                }
                foreach (var layer in _layerCollisionMatrix[objectLayer][true])
                {
                    if (!_grid[(int)toCheckPosition.x, (int)toCheckPosition.y].Layers.ContainsKey(layer))
                        continue;
                    foreach (var gameObject in _grid[(int)toCheckPosition.x, (int)toCheckPosition.y].Layers[layer])
                    {
                        if (Math.Abs(gameObject.Position.x - position.x) < 1
                            && Math.Abs(gameObject.Position.y - position.y) < 1)
                        {
                            this.DebugWithoutID("false");
                            return false;
                        }
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
        protected XYPosition CorrectPosition(XYPosition position, int objectWidth = 1, int objectHeight = 1, int objectLayer = 0)
        {
            Debug("Correcting Position : " + position.ToString() + " width : " + objectWidth + " height : " + objectHeight + " layer : " + objectLayer);

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


        protected override void OnChildrenMove(GameObject childrenGameObject, MoveEventArgs e, XYPosition previousPosition)
        {
            this.Debug("Attempting to move Children : " + childrenGameObject.ID);
            //base.OnChildrenMove(childrenGameObject, e, out eOut);
            XYPosition aim;
            double resultDistance = e.distance;
            double deltaX = e.distance * Math.Cos(e.angle);
            double deltaY = e.distance * Math.Sin(e.angle);
            Direction collisionDirection = 0;
            Debug("initialize resultDistance : " + resultDistance);

            void RefreshResultDistanceDeltaXDeltaY(double newResultDistance)
            {
                deltaX = deltaX * newResultDistance / resultDistance;
                deltaY = deltaY * newResultDistance / resultDistance;
                resultDistance = newResultDistance;
            }

            //这一段代码是用来调整MoveEventArgs，使其移动后的位置在地图内，避免后面的判断发生异常
            double XDistance = resultDistance;
            Direction XDirection = 0;
            double YDistance = resultDistance;
            Direction YDirection = 0;
            if (deltaX + previousPosition.x < (double)childrenGameObject.Width / 2)
            {
                XDistance = XDistance * ((double)childrenGameObject.Width / 2 - previousPosition.x) / deltaX;
                //deltaX = (double)childrenGameObject.Width / 2 - previousPosition.x;
                XDirection = Direction.Left;
            }
            else if (deltaX + previousPosition.x > (double)this._width - (double)childrenGameObject.Width / 2)
            {
                XDistance = XDistance * ((double)this._width - (double)childrenGameObject.Width / 2 - previousPosition.x) / deltaX;
                //deltaX = (double)this._width - (double)childrenGameObject.Width / 2 - previousPosition.x;
                XDirection = Direction.Right;
            }
            if (deltaY + previousPosition.y < (double)childrenGameObject.Height / 2)
            {
                YDistance = YDistance * ((double)childrenGameObject.Height / 2 - previousPosition.y) / deltaY;
                //deltaY = (double)childrenGameObject.Height / 2 - previousPosition.y;
                YDirection = Direction.Down;
            }
            else if (deltaY + previousPosition.y > (double)this._height - (double)childrenGameObject.Height / 2)
            {
                YDistance = YDistance * ((double)this._height - (double)childrenGameObject.Height / 2 - previousPosition.y) / deltaY;
                //deltaY = (double)this._height - (double)childrenGameObject.Height / 2 - previousPosition.y;
                YDirection = Direction.Up;
            }
            RefreshCollisionDirection();
            void RefreshCollisionDirection()
            {
                if (XDistance < YDistance)
                {
                    collisionDirection = XDirection;
                    RefreshResultDistanceDeltaXDeltaY(XDistance);
                }
                else if (YDistance < XDistance)
                {
                    collisionDirection = YDirection;
                    RefreshResultDistanceDeltaXDeltaY(YDistance);
                }
                else
                {
                    if (XDirection == Direction.Left)
                    {
                        if (YDirection == Direction.Down)
                            collisionDirection = Direction.LeftDown;
                        else
                            collisionDirection = Direction.LeftUp;
                    }
                    else
                    {
                        if (YDirection == Direction.Down)
                            collisionDirection = Direction.RightDown;
                        else
                            collisionDirection = Direction.RightUp;
                    }
                    RefreshResultDistanceDeltaXDeltaY(XDistance);
                }
            }
            aim = new XYPosition(previousPosition.x + deltaX, previousPosition.y + deltaY);




            //XYPosition deltaVector = new XYPosition(deltaX, deltaY);
            this.Debug("Move Children : " + childrenGameObject.ID + " from : " + previousPosition.ToString() + " aim : " + aim.ToString());
            XDirection = deltaX < 0 ? Direction.Left : Direction.Right;
            YDirection = deltaY >= 0 ? Direction.Up : Direction.Down;
            this.Debug("Move children : " + childrenGameObject.ID + " direction : " + XDirection + " , " + YDirection);

            List<double> LeftRightExtends = new List<double>();
            List<double> UpDownExtends = new List<double>();

            if (XDirection == Direction.Left)
            {
                LeftRightExtends.Add(aim.x - 0.5);
                LeftRightExtends.Add(previousPosition.x - 0.5);
            }
            else
            {
                LeftRightExtends.Add(previousPosition.x + 0.5);
                LeftRightExtends.Add(aim.x + 0.5);
            }
            if (YDirection == Direction.Down)
            {
                UpDownExtends.Add(aim.y - 0.5);
                UpDownExtends.Add(previousPosition.y - 0.5);
            }
            else
            {
                UpDownExtends.Add(previousPosition.y + 0.5);
                UpDownExtends.Add(aim.y + 0.5);
            }

            HashSet<GameObject> toCheckGameObjects = new HashSet<GameObject>();//这个列表里的GameObject都是有可能与childrenGameObject发生碰撞的
            void TryToAddToCheckGameObjects(int x, int y)//搜索(x,y)方格里是否有可以添加到toCheckGameObjects里的方块，若有，添加之。
            {
                bool isAdded = false;
                foreach (var layer in _layerCollisionMatrix[childrenGameObject.Layer][true])//对于每一个可碰撞的层
                {
                    if (!_grid[x, y].Layers.ContainsKey(layer))//如果这个方格里没有这个层，则跳过
                        continue;
                    foreach (var gameObject in _grid[x, y].Layers[layer])
                    {
                        if (gameObject != childrenGameObject)//避免添加自己
                            toCheckGameObjects.Add(gameObject);
                        this.Debug("toCheckGameObject : " + gameObject.ID + " (" + gameObject.Position.x + "," + gameObject.Position.y + ")  Added");
                        isAdded = true;
                    }
                }
                if (!isAdded)
                    this.Debug("toCheckPosition : (" + x + "," + y + ")  Ignored");
            }

            for (int row = (int)previousPosition.y - 1; row <= (int)previousPosition.y + 1; row += 2)
            {
                if (!YIsLegal(row))
                    continue;
                for (int column = (int)previousPosition.x - 1; column <= (int)previousPosition.x + 1; column++)
                {
                    if (!XIsLegal(column))
                        continue;
                    TryToAddToCheckGameObjects(column, row);
                }
            }
            for (int column = (int)previousPosition.x - 1; column <= (int)previousPosition.x + 1; column += 2)
            {
                if (!XIsLegal(column))
                    continue;
                for (int row = (int)previousPosition.y - 1 + 1; row <= (int)previousPosition.y + 1 - 1; row++)
                {
                    if (!YIsLegal(row))
                        continue;
                    TryToAddToCheckGameObjects(column, row);
                }
            }

            for (double xToCheck = (int)(LeftRightExtends[0] - 0.5) + 1 + 0.5; xToCheck < LeftRightExtends[1]; xToCheck++)
            {
                double yDown = previousPosition.y - 0.5 + (xToCheck - (previousPosition.x + ((XDirection == Direction.Left) ? -0.5 : 0.5))) * Math.Tan(e.angle);
                double yToCheck = (int)(yDown - 0.5) + 1 + 0.5;
                if (YIsLegal((int)yToCheck) && XIsLegal((int)xToCheck))
                {
                    TryToAddToCheckGameObjects((int)xToCheck, (int)yToCheck);
                }
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (YIsLegal(yToAdd) && XIsLegal(xToAdd))
                    {
                        TryToAddToCheckGameObjects(xToAdd, yToAdd);
                    }
                }
            }
            for (double yToCheck = (int)(UpDownExtends[0] - 0.5) + 1 + 0.5; yToCheck < UpDownExtends[1]; yToCheck++)
            {
                double xLeft = previousPosition.x - 0.5 + (yToCheck - (previousPosition.y + ((YDirection == Direction.Down) ? -0.5 : 0.5))) / Math.Tan(e.angle);
                double xToCheck = (int)(xLeft - 0.5) + 1 + 0.5;
                if (YIsLegal((int)yToCheck) && XIsLegal((int)xToCheck))
                {
                    TryToAddToCheckGameObjects((int)xToCheck, (int)yToCheck);
                }
                foreach (var item in EightCornerVector)
                {
                    int xToAdd = (int)(xToCheck + 2 * item.Value.x);
                    int yToAdd = (int)(yToCheck + 2 * item.Value.y);
                    if (YIsLegal(yToAdd) && XIsLegal(xToAdd))
                    {
                        TryToAddToCheckGameObjects(xToAdd, yToAdd);
                    }
                }
            }

            foreach (var toCheckGameObject in toCheckGameObjects)
            {
                //double tempResultDistance = resultDistance;
                XDistance = resultDistance;
                //double LeftRightDistance = resultDistance;
                double intervalX = (XDirection == Direction.Left) ?
                    Math.Abs((toCheckGameObject.Position.x + 0.5) - (previousPosition.x - 0.5)) :
                    Math.Abs((toCheckGameObject.Position.x - 0.5) - (previousPosition.x + 0.5));
                if (intervalX < Math.Abs(deltaX))
                {
                    this.Debug("deltaX : " + deltaX);
                    double yDown = previousPosition.y - 0.5 + intervalX * Math.Sign(deltaX) * deltaY / deltaX;
                    if (yDown > toCheckGameObject.Position.y - 0.5 - 1
                        && yDown < toCheckGameObject.Position.y + 0.5)
                    {
                        this.Debug("intervalX : " + intervalX);
                        XDistance = intervalX * resultDistance / Math.Abs(deltaX);
                    }
                }
                this.Debug("XDistance : " + XDistance);

                //double UpDownDistance = resultDistance;
                YDistance = resultDistance;
                double intervalY = (YDirection == Direction.Down) ?
                    Math.Abs((toCheckGameObject.Position.y + 0.5) - (previousPosition.y - 0.5)) :
                    Math.Abs((toCheckGameObject.Position.y - 0.5) - (previousPosition.y + 0.5));
                if (intervalY < Math.Abs(deltaY))
                {
                    this.Debug("deltaY : " + deltaY);
                    double xLeft = previousPosition.x - 0.5 + intervalY * Math.Sign(deltaY) * deltaX / deltaY;
                    if (xLeft > toCheckGameObject.Position.x - 0.5 - 1
                        && xLeft < toCheckGameObject.Position.x + 0.5)
                    {
                        this.Debug("intervalY : " + intervalY);
                        YDistance = intervalY * resultDistance / Math.Abs(deltaY);
                    }
                }
                this.Debug("YDistance : " + YDistance);

                RefreshCollisionDirection();
            }
            Debug("resultDistance : " + resultDistance);
            childrenGameObject._position = previousPosition + new XYPosition(resultDistance * Math.Cos(e.angle), resultDistance * Math.Sin(e.angle));
            this._grid[(int)previousPosition.x, (int)previousPosition.y].DeleteGameObject(childrenGameObject);
            this._grid[(int)childrenGameObject.Position.x, (int)childrenGameObject.Position.y].AddGameObject(childrenGameObject);

            if (childrenGameObject.Bouncable && resultDistance < e.distance)
            {
                double bounceAngle;
                if (collisionDirection == XDirection)
                {
                    bounceAngle = Math.PI - e.angle;
                }
                else if (collisionDirection == YDirection)
                {
                    bounceAngle = -e.angle;
                }
                else
                {
                    bounceAngle = Math.PI + e.angle;
                }
                childrenGameObject.Move(new MoveEventArgs(bounceAngle, e.distance - resultDistance));
                childrenGameObject.Velocity = new Vector(bounceAngle, childrenGameObject.Velocity.length);
            }

            return;
        }

        //Layer
        protected int _layerCount = 1;
        public int LayerCount
        {
            get { return this._layerCount; }
            set
            {
                Debug("set LayCount to " + LayerCount);
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
            Debug("Add Layer");
            this._layerCollisionMatrix.Add(this._layerCount, new Dictionary<bool, HashSet<int>>
            {
                { true, new HashSet<int>() },
                { false, new HashSet<int>() }
            });
            for (int i = 0; i < this._layerCount; i++)
                this._layerCollisionMatrix[this._layerCount][false].Add(i);
            for (int i = 0; i <= this._layerCount; i++)
                this._layerCollisionMatrix[i][false].Add(this._layerCount);
            this._layerCount++;
        }
        public void DeleteLayer()
        {
            foreach (var gameObject in new HashSet<GameObject>(_gameObjectListByLayer[this._layerCount - 1]))
            {
                gameObject.Parent = null;
            }
            _layerCollisionMatrix.Remove(this._layerCount - 1);
            for (int i = 0; i < this._layerCount - 1; i++)
            {
                _layerCollisionMatrix[i][true].Remove(this._layerCount - 1);
                _layerCollisionMatrix[i][false].Remove(this._layerCount - 1);
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
        public bool SetLayerCollisionTrue(int layer1, int layer2)
        {
            if (layer1 >= this._layerCount || layer2 >= this._layerCount)
                return false;
            Debug("Enable " + layer1 + " and " + layer2 + " collision");
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
            Debug("Disable " + layer1 + " and " + layer2 + " collision");
            this._layerCollisionMatrix[layer1][true].Remove(layer2);
            this._layerCollisionMatrix[layer1][false].Add(layer2);
            this._layerCollisionMatrix[layer2][true].Remove(layer1);
            this._layerCollisionMatrix[layer2][false].Add(layer1);
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
            if (!_layerCollisionMatrix.ContainsKey(e.layer))
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
            return;
        }
        //Layer

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
                            foreach (var gameObject in layer.Value)
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
    public class MapCell
    {
        public readonly object publicLock = new object();
        protected readonly object privateLock = new object();
        protected Dictionary<int, HashSet<GameObject>> _layers = new Dictionary<int, HashSet<GameObject>>();
        public Dictionary<int, HashSet<GameObject>> Layers { get { return _layers; } }
        protected void AddGameObjectByLayer(GameObject gameObject)
        {
            if (!_layers.ContainsKey(gameObject.Layer))
            {
                _layers.Add(gameObject.Layer, new HashSet<GameObject>());
            }
            _layers[gameObject.Layer].Add(gameObject);
        }
        protected void DeleteGameObjectByLayer(GameObject gameObject)
        {
            if (!_layers.ContainsKey(gameObject.Layer))
                return;
            _layers[gameObject.Layer].Remove(gameObject);
            if (_layers[gameObject.Layer].Count <= 0)
                _layers.Remove(gameObject.Layer);
        }
        protected Dictionary<Type, HashSet<GameObject>> _types = new Dictionary<Type, HashSet<GameObject>>();
        public Dictionary<Type, HashSet<GameObject>> Types { get { return _types; } }
        protected void AddGameObjectByType(GameObject gameObject)
        {
            if (!_types.ContainsKey(gameObject.GetType()))
            {
                _types.Add(gameObject.GetType(), new HashSet<GameObject>());
            }
            _types[gameObject.GetType()].Add(gameObject);
        }
        protected void DeleteGameObjectByType(GameObject gameObject)
        {
            _types[gameObject.GetType()].Remove(gameObject);
            if (_types[gameObject.GetType()].Count <= 0)
                _types.Remove(gameObject.GetType());
        }
        public void AddGameObject(GameObject gameObject)
        {
            AddGameObjectByLayer(gameObject);
            AddGameObjectByType(gameObject);
        }
        public void DeleteGameObject(GameObject gameObject)
        {
            DeleteGameObjectByLayer(gameObject);
            DeleteGameObjectByType(gameObject);
        }
        public bool ContainsGameObject(GameObject gameObject)
        {
            if (!_layers.ContainsKey(gameObject.Layer))
                return false;
            return _layers[gameObject.Layer].Contains(gameObject);
        }
        public bool IsEmpty()
        {
            return _layers.Count == 0;
        }
    }
}
