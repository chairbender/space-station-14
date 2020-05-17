using System;
using Content.Server.Interaction;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when being activated in the world.
    /// </summary>
    public interface IActivate
    {
        /// <summary>
        ///     Called when this component is activated by another entity.
        /// </summary>
        void Activate(ActivateEventArgs eventArgs);
    }

    public class ActivateEventArgs : EventArgs
    {
        public IEntity User { get; set; }
    }
}
