using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.GetDashboardSummary;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery : IQuery<Result<GetDashboardSummaryResponse>>;
