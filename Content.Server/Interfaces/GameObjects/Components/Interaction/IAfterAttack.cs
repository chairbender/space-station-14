using System;
using Content.Server.GameObjects.EntitySystems;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    /// This interface gives components a behavior when clicking on another object and no interaction occurs
    /// Doesn't pass what you clicked on as an argument, but if it becomes necessary we can add it later
    /// </summary>
    public interface IAfterAttack
    {
        /// <summary>
        /// Called when we interact with nothing, or when we interact with an entity out of range that has no behavior
        /// </summary>
        void AfterAttack(AfterAttackEventArgs eventArgs);
    }

    public class AfterAttackEventArgs : EventArgs
    {
        public IEntity User { get; set; }
        public GridCoordinates ClickLocation { get; set; }
        public IEntity Attacked { get; set; }
    }
}
