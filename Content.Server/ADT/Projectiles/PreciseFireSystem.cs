using System.Numerics;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._TornadoTech.FriendlyFire;

public sealed partial class PreciseFireSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private float _requiredDistanceSquared = 4f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobCollisionComponent, ProjectileHitAttemptEvent>(OnHitProjectileAttempt);
    }

    private void OnHitProjectileAttempt(Entity<MobCollisionComponent> mob, ref ProjectileHitAttemptEvent args)
    {
        if (args.ToCoordinates == null)
            return;

        if (!TryComp<MobStateComponent>(mob, out var mobState))
            return;

        if (mobState.CurrentState != Shared.Mobs.MobState.Dead)
            return;

        EntityCoordinates mobCoords = new EntityCoordinates(mob, 0, 0);
        Vector2 mapCoords = _transform.ToMapCoordinates(mobCoords, true).Position;

        float len = Vector2Helpers.CompareLength(args.ToCoordinates.Value, mapCoords);

        //Log.Debug("ToCoordinates: {ToPrettyString(args.ToCoordinates.Value)}\nMapCoords: {ToPrettyString(mapCoords)}\nDiff: {ToPrettyString(len)}");

        //if (len >= _requiredDistanceSquared)
        //    args.Cancel();
    }
}
