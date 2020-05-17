using Content.Server.GameObjects.EntitySystems;
using Content.Shared.Construction;
using Content.Shared.Interfaces;
using Content.Shared.Physics;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects.Components;
using Robust.Shared.Interfaces.Map;
using Robust.Shared.Interfaces.Physics;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.GameObjects.Components.Items
{
    /// <summary>
    /// Constructs a Construction at user's position when used, consuming the item in hand.
    /// </summary>
    [RegisterComponent]
    public class UseToConstructComponent : Component, IUse
    {
#pragma warning disable 649
        [Dependency]
        private readonly IMapManager _mapManager;
        [Dependency]
        private readonly IPrototypeManager _prototypeManager;
        [Dependency]
        private readonly IPhysicsManager _physicsManager;
#pragma warning restore 649

        private ConstructionPrototype _construct;

        public override string Name => "UseToConstruct";

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);
            if (serializer.TryReadDataField("construct", out string construct))
            {
                if (_prototypeManager.TryIndex(construct, out _construct) == false)
                {
                    Logger.Error("prototype {0} component {1}: field 'construct' value '{2}' should be a ConstructionPrototype, but" +
                                 " wasn't.", Owner.Prototype.Name, Name, construct);
                }
            }
            else
            {
                Logger.Error("prototype {0} component {1}: field 'construct' not found but is required.",
                    Owner.Prototype.Name, Name);
            }
        }

        public bool UseEntity(UseEntityEventArgs eventArgs)
        {
            if (_construct == null) return false;

            var user = eventArgs.User;
            var userGrid = _mapManager.GetGrid(user.Transform.GridID);
            var destinationGridCoords = userGrid.SnapGridCenter(user.Transform.GridPosition);
            var destinationWorldPos = destinationGridCoords.ToMapPos(_mapManager);

            if (_construct.CanBuildInImpassable == false)
            {
                // check if construction is blocked by something,
                // for now we just check a unit square centered on the snapped position where the
                // object will go
                //TODO: Check the actual collider of the thing we will spawn - not currently possible

                // slightly less than unit size so we don't hit the edges of adjacent tiles
                var physBox = new Box2(-0.45f, -0.45f, 0.45f, 0.45f).Translated(destinationWorldPos);
                if (_physicsManager.TryGetCollision(physBox, userGrid.ParentMapId,
                    (int) CollisionGroup.MobImpassable, out var collidedWith, user))
                {
                    user.PopupMessage(user, $"Blocked by {collidedWith.Name}");
                    return false;
                }
            }

            var ent = Owner.EntityManager.SpawnEntity(_construct.ID, destinationGridCoords);
            // this is needed because south is the default rotation when placing things,
            // otherwise IconSmooths like tables will have the wrong rotation
            ent.GetComponent<ITransformComponent>().LocalRotation = Direction.South.ToAngle();
            Owner.Delete();

            return true;
        }
    }
}
