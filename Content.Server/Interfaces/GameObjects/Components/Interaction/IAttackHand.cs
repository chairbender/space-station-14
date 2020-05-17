using System;
using Content.Server.Interaction;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    /// This interface gives components behavior when being clicked on or "attacked" by a user with an empty hand
    /// </summary>
    public interface IAttackHand
    {
        /// <summary>
        /// Called when a player directly interacts with an empty hand
        /// </summary>
        bool AttackHand(AttackHandEventArgs eventArgs);
    }

    public class AttackHandEventArgs : EventArgs
    {
        public IEntity User { get; set; }
    }
}
