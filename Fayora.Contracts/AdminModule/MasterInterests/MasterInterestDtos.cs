namespace Fayora.Contracts.AdminModule.MasterInterests;

public record GetMasterInterestsResponse(
    int Id,
    string Code,
    string Name,
    string IconUrl,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record CreateMasterInterestRequest(
    string Code,
    string Name,
    string IconUrl,
    int SortOrder
);

public record UpdateMasterInterestRequest(
    string Name,
    string IconUrl,
    int SortOrder
);
