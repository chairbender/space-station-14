using System;
using Content.Server.Interaction;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when they're held on the selected hand.
    /// </summary>
    public interface IHandSelected
    {
        void HandSelected(HandSelectedEventArgs eventArgs);
    }

    public class HandSelectedEventArgs : EventArgs
    {
        public HandSelectedEventArgs(IEntity user)
        {
            User = user;
        }

        public IEntity User { get; }
    }
}
