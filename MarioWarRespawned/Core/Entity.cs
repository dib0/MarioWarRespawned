using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarioWarRespawned.Core
{
    public abstract class Entity
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public float Layer { get; set; } = 0.5f;

        private readonly Dictionary<Type, IComponent> _components = new();

        public T? GetComponent<T>() where T : class, IComponent
        {
            return _components.GetValueOrDefault(typeof(T)) as T;
        }

        public void AddComponent<T>(T component) where T : class, IComponent
        {
            _components[typeof(T)] = component;
            component.Owner = this;
        }

        public void RemoveComponent<T>() where T : class, IComponent
        {
            _components.Remove(typeof(T));
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
    }

    public interface IComponent
    {
        Entity Owner { get; set; }
    }
}
