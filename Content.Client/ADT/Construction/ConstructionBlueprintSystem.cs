using System.Linq;
using System.Text;
using Content.Client.Chat.Managers;
using Content.Client.Popups;
using Content.Shared.ADT.Construction;
using Content.Shared.Chat;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Construction;

public sealed class ConstructionBlueprintSystem : EntitySystem
{
    [Dependency] private readonly ConstructionSystem _constructionSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ConstructionBlueprintPaperComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(EntityUid uid, ConstructionBlueprintPaperComponent comp, UseInHandEvent args)
    {
        Log.Debug("Use 1");
        if (args.Handled)
            return;
        Log.Debug("Use 2");
        if (comp.Blueprint == null)
            return;
        Log.Debug("Use 3");

        List<ConstructionBlueprintGhost> ghosts = comp.Blueprint.Positions;

        Log.Debug(ghosts.Count.ToString());

        foreach (var ghost in ghosts)
        {
            MapCoordinates mapCoordinates = _transform.ToMapCoordinates(ghost.Coordinates);

            var mapEntity = _mapManager.GetMapEntityId(mapCoordinates.MapId);
            var entityCoords = _transform.ToCoordinates(mapEntity, mapCoordinates);

            if (ghost.Prototype.HasValue)
            {
                _popup.PopupClient("Спавн ", _playerManager.LocalEntity, PopupType.Medium);
                _constructionSystem.TrySpawnGhost(_proto.Index<ConstructionPrototype>(ghost.Prototype.Value), entityCoords, ghost.Rotation.GetDir(), out var spawned);
            }
        }
        /*ConstructionBlueprint? memorized = GetMemorizedBlueprint(uid);

        if (memorized == null)
        {
            _popup.PopupClient("Вы ничего не ", _playerManager.LocalEntity, PopupType.Medium);
        }*/
    }

    public void GenerateBlueprint(out ConstructionBlueprint blueprint)
    {
        List<ConstructionBlueprintColor> colorList = new();

        List<EntityUid> ghosts = _constructionSystem.GetGhosts().Values.ToList();
        List<ConstructionBlueprintGhost> blueprintGhosts = new();

        foreach (var uid in ghosts)
        {
            var transform = Comp<TransformComponent>(uid);
            var ghost = Comp<ConstructionGhostComponent>(uid);
            //MapCoordinates mapCoords = _transform.GetMapCoordinates(uid);
            NetCoordinates tileCoordinates = GetNetCoordinates(transform.Coordinates);
            //Vector2i tile = GetNetCoordinates(transform.Coordinates).Position.Floored();

            var blueprintGhost = new ConstructionBlueprintGhost
            {
                Coordinates = tileCoordinates,
                Rotation = transform.LocalRotation,
            };
            if (ghost.Prototype != null)
                blueprintGhost.Prototype = ghost.Prototype.ID;

            blueprintGhosts.Add(blueprintGhost);
        }

        blueprint = new ConstructionBlueprint(colorList, blueprintGhosts);
    }

    private void GetMostUsedColor(Texture texture)
    {
        Color[] colors = [];

        for (int y = 0; y < texture.Height; y++)
        {
            for (int x = 0; x < texture.Width; x++)
            {
                Color color = texture.GetPixel(x, y);
                colors.Append(color);
            }
        }

        Color mostUsedColor = colors.GroupBy(c => c).OrderByDescending(g => g.Count()).First().Key;
    }

    public void SaveBlueprint()
    {
        GenerateBlueprint(out var blueprint);
        RaiseNetworkEvent(new ConstructionBlueprintEvent(blueprint));

        //_chatManager.SendMessage($"Idk", ChatSelectChannel.Radio);
        //_popup.PopupClient(SerializeBlueprint(), _playerManager.LocalEntity, PopupType.Medium);
    }
}