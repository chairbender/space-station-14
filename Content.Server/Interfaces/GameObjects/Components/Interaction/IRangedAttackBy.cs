using System;
using Content.Server.GameObjects.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    /// This interface gives components behavior when being clicked by objects outside the range of direct use
    /// </summary>
    public interface IRangedAttackBy
    {
        /// <summary>
        /// Called when we try to interact with an entity out of range
        /// </summary>
        /// <returns></returns>
        bool RangedAttackBy(RangedAttackByEventArgs eventArgs);
    }

    [PublicAPI]
    public class RangedAttackByEventArgs : EventArgs
    {
        public IEntity User { get; set; }
        public IEntity Weapon { get; set; }
        public GridCoordinates ClickLocation { get; set; }
    }
}
