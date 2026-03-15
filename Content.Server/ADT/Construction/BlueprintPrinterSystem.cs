using Content.Shared.ADT.Construction;
using Content.Shared.Construction;
using Content.Shared.DocumentPrinter;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;

namespace Content.Server.ADT.Construction;

public sealed class BlueprintPrinterSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedConstructionBlueprintSystem _blueprint = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlueprintPrinterComponent, PrintingDocumentEvent>(OnPrinting);
    }

    public void OnPrinting(EntityUid uid, BlueprintPrinterComponent component, PrintingDocumentEvent args)
    {
        Log.Debug("On printing 1");
        if (!TryComp<PaperComponent>(args.Paper, out var paperComponent))
            return;

        if (!TryComp<ConstructionBlueprintPaperComponent>(args.Paper, out var constructionPaperComponent))
            return;

        ConstructionBlueprint? blueprint = _blueprint.GetMemorizedBlueprint(args.Actor);
        if (blueprint == null)
            return;

        paperComponent.Content = _blueprint.SerializeBlueprint(blueprint);
        constructionPaperComponent.Blueprint = blueprint;

        Dirty(args.Paper, paperComponent);
        Dirty(args.Paper, constructionPaperComponent);
    }
}
