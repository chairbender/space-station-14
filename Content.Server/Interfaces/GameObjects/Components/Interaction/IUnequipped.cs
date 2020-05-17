using System;
using Content.Server.Interaction;
using Content.Shared.GameObjects.Components.Inventory;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when their owner is removed from an inventory slot.
    /// </summary>
    public interface IUnequipped
    {
        void Unequipped(UnequippedEventArgs eventArgs);
    }


    public class UnequippedEventArgs : EventArgs
    {
        public UnequippedEventArgs(IEntity user, EquipmentSlotDefines.Slots slot)
        {
            User = user;
            Slot = slot;
        }

        public IEntity User { get; }
        public EquipmentSlotDefines.Slots Slot { get; }
    }
}
