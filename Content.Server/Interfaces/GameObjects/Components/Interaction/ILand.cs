using System;
using Content.Server.GameObjects.EntitySystems;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Interfaces.GameObjects.Components.Interaction
{
    /// <summary>
    ///     This interface gives components behavior when landing after being thrown.
    /// </summary>
    public interface ILand
    {
        void Land(LandEventArgs eventArgs);
    }

    public class LandEventArgs : EventArgs
    {
        public LandEventArgs(IEntity user, GridCoordinates landingLocation)
        {
            User = user;
            LandingLocation = landingLocation;
        }

        public IEntity User { get; }
        public GridCoordinates LandingLocation { get; }
    }
}
