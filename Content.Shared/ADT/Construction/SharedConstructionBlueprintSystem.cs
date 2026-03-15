using Robust.Shared.Serialization;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Linq;
using System.Text;
using Robust.Shared.Map;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Construction;

public sealed class SharedConstructionBlueprintSystem : EntitySystem
{
    private Dictionary<NetUserId, ConstructionBlueprint> _memorizedBlueprints = new();

    public override void Initialize()
    {
        Log.Debug("Init");
        base.Initialize();
        SubscribeNetworkEvent<ConstructionBlueprintEvent>(OnConstructionBlueprintEvent);
    }

    public void OnConstructionBlueprintEvent(ConstructionBlueprintEvent ev, EntitySessionEventArgs args)
    {
        Log.Debug($"We got blueprint message from client. Saving from ${args.SenderSession.UserId}.");
        _memorizedBlueprints[args.SenderSession.UserId] = ev.Blueprint;
    }

    #region Memory

    public ConstructionBlueprint? GetMemorizedBlueprint(NetUserId netUserId)
    {
        return _memorizedBlueprints[netUserId] ?? null;
    }

    public ConstructionBlueprint? GetMemorizedBlueprint(EntityUid uid)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return null;

        return GetMemorizedBlueprint(actor.PlayerSession.UserId);
    }

    #endregion

    #region Serialization

    public string SerializeBlueprint(ConstructionBlueprint blueprint)
    {
        List<Vector2i> positions = blueprint.Positions.Select(x => x.Coordinates.Position.Floored()).ToList();

        int minX = positions.Min(p => p.X);
        int maxX = positions.Max(p => p.X);
        int minY = positions.Min(p => p.Y);
        int maxY = positions.Max(p => p.Y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        var grid = new char[height, width];

        // Fill with dots
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid[y, x] = '.';

        // Mark entity positions
        foreach (var pos in positions)
        {
            var x = pos.X - minX;
            var y = pos.Y - minY;

            grid[y, x] = '#';
        }

        var sb = new StringBuilder();

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[y, x]);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    #endregion
}

[Serializable, NetSerializable]
public sealed class ConstructionBlueprintColor
{
    public int Position;

    public Color Color;
}

[Serializable, NetSerializable]
public sealed class ConstructionBlueprintGhost
{
    public NetCoordinates Coordinates;
    //public MapId MapId;
    public Angle Rotation;
    public ProtoId<ConstructionPrototype>? Prototype;
}

[Serializable, NetSerializable]
public sealed class ConstructionBlueprint
{
    public readonly List<ConstructionBlueprintColor> ColorList;

    public readonly List<ConstructionBlueprintGhost> Positions;

    public ConstructionBlueprint(List<ConstructionBlueprintColor> colorList, List<ConstructionBlueprintGhost> positions)
    {
        ColorList = colorList;
        Positions = positions;
    }
}

[Serializable, NetSerializable]
public sealed class ConstructionBlueprintEvent : EntityEventArgs
{
    public readonly ConstructionBlueprint Blueprint;

    public ConstructionBlueprintEvent(ConstructionBlueprint blueprint)
    {
        Blueprint = blueprint;
    }
}
