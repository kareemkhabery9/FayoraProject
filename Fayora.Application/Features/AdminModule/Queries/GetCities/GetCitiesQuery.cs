using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.Cities;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetCities;

public record GetCitiesQuery() : IQuery<Result<List<GetCitiesResponse>>>;
