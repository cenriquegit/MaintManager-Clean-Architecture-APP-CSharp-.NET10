namespace MaintManager.Application.DTOs.Vehicle;

public sealed record VehicleConfigResponse(
    int Prcoid,
    IReadOnlyList<ActionConfigItem> AllowedActions,
    IReadOnlyList<MaterialConfigItem> AllowedMaterials,
    IReadOnlyList<ActionConfigItem> AllowedComponents
);

public sealed record ActionConfigItem(int Acatid, string Name, string? Category);

public sealed record MaterialConfigItem(int Mateid, string Name, string? UnitOfMeasure);
