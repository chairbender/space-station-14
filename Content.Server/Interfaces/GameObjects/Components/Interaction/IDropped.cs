using System;
using Content.Server.Interaction;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when they're dropped by a mob.
    /// </summary>
    public interface IDropped
    {
        void Dropped(DroppedEventArgs eventArgs);
    }

    public class DroppedEventArgs : EventArgs
    {
        public DroppedEventArgs(IEntity user)
        {
            User = user;
        }

        public IEntity User { get; }
    }
}
