using System;
using Content.Server.GameObjects.EntitySystems;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    /// This interface gives components behavior when using the entity in your hands
    /// </summary>
    public interface IUse
    {
        /// <summary>
        /// Called when we activate an object we are holding to use it
        /// </summary>
        /// <returns></returns>
        bool UseEntity(UseEntityEventArgs eventArgs);
    }

    public class UseEntityEventArgs : EventArgs
    {
        public IEntity User { get; set; }
    }
}
