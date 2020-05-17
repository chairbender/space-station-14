using System;
using Content.Server.Interaction;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when they're held on a deselected hand.
    /// </summary>
    public interface IHandDeselected
    {
        void HandDeselected(HandDeselectedEventArgs eventArgs);
    }

    public class HandDeselectedEventArgs : EventArgs
    {
        public HandDeselectedEventArgs(IEntity user)
        {
            User = user;
        }

        public IEntity User { get; }
    }
}
