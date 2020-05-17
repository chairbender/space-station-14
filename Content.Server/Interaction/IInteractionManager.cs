using System;
using Content.Shared.GameObjects.Components.Inventory;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Server.Interaction
{
    /// <summary>
    /// Provides common server-side functionality for dealing with interactions, such as triggering
    /// them.
    /// </summary>
    public interface IInteractionManager : ISharedInteractionManager
    {
        /// <summary>
        /// Activates the Activate behavior of an object
        /// Verifies that the user is capable of doing the use interaction first
        /// </summary>
        /// <param name="user"></param>
        /// <param name="used"></param>
        void TryInteractionActivate(IEntity user, IEntity used);

        /// <summary>
        /// Uses a weapon/object on an entity
        /// Finds components with the AttackBy interface and calls their function
        /// </summary>
        void Interaction(IEntity user, IEntity weapon, IEntity attacked, GridCoordinates clickLocation);

        /// <summary>
        /// Uses an empty hand on an entity
        /// Finds components with the AttackHand interface and calls their function
        /// </summary>
        void Interaction(IEntity user, IEntity attacked);

        /// <summary>
        /// Activates the Use behavior of an object
        /// Verifies that the user is capable of doing the use interaction first
        /// </summary>
        /// <param name="user"></param>
        /// <param name="used"></param>
        void TryUseInteraction(IEntity user, IEntity used);

        /// <summary>
        /// Activates/Uses an object in control/possession of a user
        /// If the item has the IUse interface on one of its components we use the object in our hand
        /// </summary>
        void UseInteraction(IEntity user, IEntity used);

        /// <summary>
        /// Activates the Throw behavior of an object
        /// Verifies that the user is capable of doing the throw interaction first
        /// </summary>
        bool TryThrowInteraction(IEntity user, IEntity item);

        /// <summary>
        ///     Calls Thrown on all components that implement the IThrown interface
        ///     on an entity that has been thrown.
        /// </summary>
        void ThrownInteraction(IEntity user, IEntity thrown);

        /// <summary>
        ///     Calls Land on all components that implement the ILand interface
        ///     on an entity that has landed after being thrown.
        /// </summary>
        void LandInteraction(IEntity user, IEntity landing, GridCoordinates landLocation);

        /// <summary>
        ///     Calls Equipped on all components that implement the IEquipped interface
        ///     on an entity that has been equipped.
        /// </summary>
        void EquippedInteraction(IEntity user, IEntity equipped, EquipmentSlotDefines.Slots slot);

        /// <summary>
        ///     Calls Unequipped on all components that implement the IUnequipped interface
        ///     on an entity that has been equipped.
        /// </summary>
        void UnequippedInteraction(IEntity user, IEntity equipped, EquipmentSlotDefines.Slots slot);

        /// <summary>
        /// Activates the Dropped behavior of an object
        /// Verifies that the user is capable of doing the drop interaction first
        /// </summary>
        bool TryDroppedInteraction(IEntity user, IEntity item);

        /// <summary>
        ///     Calls Dropped on all components that implement the IDropped interface
        ///     on an entity that has been dropped.
        /// </summary>
        void DroppedInteraction(IEntity user, IEntity item);

        /// <summary>
        ///     Calls HandSelected on all components that implement the IHandSelected interface
        ///     on an item entity on a hand that has just been selected.
        /// </summary>
        void HandSelectedInteraction(IEntity user, IEntity item);

        /// <summary>
        ///     Calls HandDeselected on all components that implement the IHandDeselected interface
        ///     on an item entity on a hand that has just been deselected.
        /// </summary>
        void HandDeselectedInteraction(IEntity user, IEntity item);

        /// <summary>
        /// Will have two behaviors, either "uses" the weapon at range on the entity if it is capable of accepting that action
        /// Or it will use the weapon itself on the position clicked, regardless of what was there
        /// </summary>
        void RangedInteraction(IEntity user, IEntity weapon, IEntity attacked, GridCoordinates clickLocation);
    }
}
