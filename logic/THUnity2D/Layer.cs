using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace THUnity2D
{
    public class Layer
    {
        protected internal readonly ConcurrentDictionary<Layer, byte> CollisionLayers = new ConcurrentDictionary<Layer, byte>();
        protected internal readonly ConcurrentDictionary<Layer, byte> TriggerLayers = new ConcurrentDictionary<Layer, byte>();
        protected internal readonly ConcurrentDictionary<GameObject, byte> GameObjectList = new ConcurrentDictionary<GameObject, byte>();
        public Layer()
        { }
    }
}
