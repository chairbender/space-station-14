using System;
using Content.Server.Interaction;
using Content.Shared.GameObjects.Components.Inventory;
using Robust.Shared.Interfaces.GameObjects;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when their owner is put in an inventory slot.
    /// </summary>
    public interface IEquipped
    {
        void Equipped(EquippedEventArgs eventArgs);
    }

    public class EquippedEventArgs : EventArgs
    {
        public EquippedEventArgs(IEntity user, EquipmentSlotDefines.Slots slot)
        {
            User = user;
            Slot = slot;
        }

        public IEntity User { get; }
        public EquipmentSlotDefines.Slots Slot { get; }
    }
}
