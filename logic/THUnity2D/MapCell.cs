using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace THUnity2D
{
    public class MapCell
    {
        public readonly object publicLock = new object();
        protected readonly object privateLock = new object();

        //Layers
        protected ConcurrentDictionary<Layer, ConcurrentDictionary<GameObject, byte>> _layers = new ConcurrentDictionary<Layer, ConcurrentDictionary<GameObject, byte>>();
        protected internal ConcurrentDictionary<Layer, ConcurrentDictionary<GameObject, byte>> Layers { get { return _layers; } }
        protected void AddGameObjectToLayers(GameObject gameObject)
        {
            lock (privateLock)
            {
                if (!_layers.ContainsKey(gameObject.Layer))
                {
                    _layers.TryAdd(gameObject.Layer, new ConcurrentDictionary<GameObject, byte>());
                }
                _layers[gameObject.Layer].TryAdd(gameObject, 0);
            }
        }
        protected void DeleteGameObjectFromLayers(GameObject gameObject)
        {
            lock (privateLock)
            {
                if (!_layers.ContainsKey(gameObject.Layer))
                    return;
                byte temp;
                _layers[gameObject.Layer].TryRemove(gameObject, out temp);
                if (_layers[gameObject.Layer].Count <= 0)
                {
                    ConcurrentDictionary<GameObject, byte> tmp;
                    _layers.TryRemove(gameObject.Layer, out tmp);
                }
            }
        }
        public HashSet<GameObject>? GetObjects(Layer layer)
        {
            lock (privateLock)
            {
                if (!_layers.ContainsKey(layer))
                    return null;
                return new HashSet<GameObject>(_layers[layer].Keys);
            }
        }
        public GameObject? GetFirstObject(Layer layer)
        {
            lock (privateLock)
            {
                if (!_layers.ContainsKey(layer))
                    return null;
                foreach (var gameObject in _layers[layer].Keys)
                {
                    return gameObject;
                }
                return null;
            }
        }
        //Layers end

        //Types
        protected ConcurrentDictionary<Type, ConcurrentDictionary<GameObject, byte>> _types = new ConcurrentDictionary<Type, ConcurrentDictionary<GameObject, byte>>();
        protected internal ConcurrentDictionary<Type, ConcurrentDictionary<GameObject, byte>> Types { get { return _types; } }
        protected void AddGameObjectToTypes(GameObject gameObject)
        {
            lock (privateLock)
            {
                if (!_types.ContainsKey(gameObject.GetType()))
                {
                    _types.TryAdd(gameObject.GetType(), new ConcurrentDictionary<GameObject, byte>());
                }
                _types[gameObject.GetType()].TryAdd(gameObject, 0);
            }
        }
        protected void DeleteGameObjectFromTypes(GameObject gameObject)
        {
            lock (privateLock)
            {
                if (!_types.ContainsKey(gameObject.GetType()))
                    return;
                byte temp;
                _types[gameObject.GetType()].TryRemove(gameObject, out temp);
                if (_types[gameObject.GetType()].Count <= 0)
                {
                    ConcurrentDictionary<GameObject, byte>? tmp;
                    _types.TryRemove(gameObject.GetType(), out tmp);
                }
            }
        }
        public HashSet<GameObject>? GetObjects(Type type)
        {
            lock (privateLock)
            {
                if (!_types.ContainsKey(type))
                    return null;
                return new HashSet<GameObject>(_types[type].Keys);
            }
        }
        public GameObject? GetFirstObject(Type type)
        {
            lock (privateLock)
            {
                if (!_types.ContainsKey(type))
                    return null;
                foreach (var gameObject in _types[type].Keys)
                {
                    return gameObject;
                }
                return null;
            }
        }
        //Types end

        internal void AddGameObject(GameObject gameObject)
        {
            lock (privateLock)
            {
                AddGameObjectToLayers(gameObject);
                AddGameObjectToTypes(gameObject);
            }
        }
        internal void DeleteGameObject(GameObject gameObject)
        {
            lock (privateLock)
            {
                DeleteGameObjectFromLayers(gameObject);
                DeleteGameObjectFromTypes(gameObject);
            }
        }
        public bool ContainsGameObject(GameObject gameObject)
        {
            lock (privateLock)
            {
                if (!_layers.ContainsKey(gameObject.Layer))
                    return false;
                return _layers[gameObject.Layer].ContainsKey(gameObject);
            }
        }
        public bool ContainsLayer(Layer layer)
        {
            lock (privateLock)
            {
                return _layers.ContainsKey(layer);
            }
        }
        public bool ContainsType(Type type)
        {
            lock (privateLock)
            {
                return _types.ContainsKey(type);
            }
        }
        public bool IsEmpty()
        {
            lock (privateLock)
            {
                return _layers.Count == 0;
            }
        }
    }
}
