using System;
using Content.Server.GameObjects.EntitySystems;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    /// This interface gives components behavior when being clicked on or "attacked" by a user with an object in their hand
    /// </summary>
    public interface IAttackBy
    {
        /// <summary>
        /// Called when using one object on another
        /// </summary>
        bool AttackBy(AttackByEventArgs eventArgs);
    }

    public class AttackByEventArgs : EventArgs
    {
        public IEntity User { get; set; }
        public GridCoordinates ClickLocation { get; set; }
        public IEntity AttackWith { get; set; }
    }
}
