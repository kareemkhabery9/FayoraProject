using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.GuideModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Features.TourGuideModule.Common;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.TourGuideModule.Commands.CreatePackageOccurrences;

public class CreatePackageOccurrencesCommandHandler(
    IPackageRepository packageRepository,
    IPackageOccurrenceRepository packageOccurrenceRepository,
    IClientContextProvider clientContextProvider,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreatePackageOccurrencesCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        CreatePackageOccurrencesCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = clientContextProvider.GetContext().UserId;

        var package = await packageRepository.GetPackageByIdAsync(
            request.PackageId, new IPackageRepository.PackageQueryOptions { ReadOnly = false, IncludeOccurrences = true }, cancellationToken);

        if (package is null || package.UserId != currentUserId)
            return TourGuideErrors.PackageNotFound;

        var occurrencesToAdd = request.Occurrences
            .Select(x => (x.Date, x.AvailableSeats))
            .ToList();

        var result = package.AddOccurrences(occurrencesToAdd);

        if (result.IsError)
            return result.Errors;

        foreach (var occurrence in result.Value)
        {
            packageOccurrenceRepository.Add(occurrence);
        }

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
