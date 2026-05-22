using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.UpdateAccommodation;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetAccommodations;

public record GetAccommodationsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    string? StatusFilter) : IQuery<Result<List<GetAccommodationsResponse>>>;
