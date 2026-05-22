using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetUserDetails;

public class GetUserDetailsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsResult>>
{
    public async Task<Result<UserDetailsResult>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var user = await adminRepository.GetUserByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound($"User with ID {request.UserId} not found.");

        var touristProfileDto = await adminRepository.GetTouristProfileAsync(request.UserId, cancellationToken);
        var tourGuideProfileDto = await adminRepository.GetTourGuideProfileAsync(request.UserId, cancellationToken);
        var tourCompanyProfileDto = await adminRepository.GetTourCompanyProfileAsync(request.UserId, cancellationToken);

        var result = new UserDetailsResult(
            user.Id,
            user.FirstName,
            user.LastName,
            user.PrimaryEmail?.Value ?? "",
            user.PhoneNumber?.Value ?? "",
            user.Roles?.ToString() ?? "",
            user.Status.ToString(),
            user.CurrentBalance,
            user.PreferredLanguage?.ToString() ?? "",
            user.ProfileImageUrl?.Value ?? "",
            user.CreatedAt,
            touristProfileDto,
            tourGuideProfileDto,
            tourCompanyProfileDto
        );

        return result;
    }
}
