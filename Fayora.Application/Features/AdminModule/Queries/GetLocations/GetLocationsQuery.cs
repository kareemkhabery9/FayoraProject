using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.UpdateLocation;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetLocations;

public record GetLocationsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery) : IQuery<Result<List<GetLocationsResponse>>>;
