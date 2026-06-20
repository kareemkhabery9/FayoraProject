using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.TouristModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Features.AuthModule.Common;
using Fayora.Application.Features.TouristModule.Common;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.TouristModule;
using static Fayora.Application.Common.Interfaces.Persistences.IdentityModule.IUserRepository;

namespace Fayora.Application.Features.TouristModule.Commands.CreateTouristProfile;

public class CreateTouristProfileCommandHandler(
    IUserRepository userRepository,
    ITouristRepository touristRepository,
    IMasterInterestRepository masterInterestRepository,
    IClientContextProvider clientContextProvider,
    IAuthTokenGenerator authTokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTouristProfileCommand, Result<CreateTouristProfileResult>>
{
    public async Task<Result<CreateTouristProfileResult>> Handle(CreateTouristProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = clientContextProvider.GetContext().UserId;

        var options = new UserQueryOptions { IsReadOnly = true };
        var user = await userRepository.GetUserByIdAsync(userId, options, cancellationToken);
        if (user is null) return AuthErrors.UserNotFound;

        var touristProfile = new TouristProfile(
            userId,
            request.BudgetTier,
            request.TravelStyle);

        touristRepository.AddTourist(touristProfile);

        if (request.InterestIds is not null && request.InterestIds.Count > 0)
        {
            if ((await masterInterestRepository.InterestsExistAsync(request.InterestIds, cancellationToken)) is false)
            {
                return TouristErrors.MasterInterestsNotFound;
            }

            var interests = request.InterestIds
                .Select(id => new TouristInterest(touristProfile.Id, id))
                .ToList();

            touristRepository.AddTouristInterests(interests);
        }

        var tokens = await authTokenGenerator.GenerateTokensAsync(
            user, request.DeviceId,
            cancellationToken);

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return new CreateTouristProfileResult(
            touristProfile.Id,
            user.FirstName,
            user.LastName,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresIn);
    }
}