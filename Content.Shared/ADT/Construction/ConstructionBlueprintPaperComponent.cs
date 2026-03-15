using Robust.Shared.GameStates;

namespace Content.Shared.ADT.Construction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConstructionBlueprintPaperComponent : Component
{
    [DataField, AutoNetworkedField]
    public ConstructionBlueprint? Blueprint = null;
}
