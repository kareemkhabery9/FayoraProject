using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.GetDashboardSummary;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetDashboardSummaryQuery, Result<GetDashboardSummaryResponse>>
{
    public async Task<Result<GetDashboardSummaryResponse>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userStats = await adminRepository.GetUserStatsAsync(cancellationToken);
        var bookingStats = await adminRepository.GetBookingStatsAsync(cancellationToken);
        var contentStats = await adminRepository.GetContentStatsAsync(cancellationToken);
        var recentActivities = await adminRepository.GetRecentActivitiesAsync(cancellationToken);
        var chatbotSnapshot = await adminRepository.GetChatbotSnapshotAsync(cancellationToken);
        var financialSnapshot = await adminRepository.GetFinancialSnapshotAsync(cancellationToken);
        var growthIndicators = await adminRepository.GetGrowthIndicatorsAsync(cancellationToken);

        var result = new GetDashboardSummaryResponse(
            userStats,
            bookingStats,
            contentStats,
            recentActivities,
            chatbotSnapshot,
            financialSnapshot,
            growthIndicators
        );

        return result;
    }
}
