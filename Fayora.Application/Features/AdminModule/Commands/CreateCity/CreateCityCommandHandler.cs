using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.SharedModule;
using Fayora.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.CreateCity;

public class CreateCityCommandHandler(
    IAdminRepository adminRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCityCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var coordinatesResult = GeoPoint.Create(request.Latitude, request.Longitude);
        if (coordinatesResult.IsError)
            return coordinatesResult.Errors;

        var city = new City(request.Name, request.CountryCode, coordinatesResult.Value);

        adminRepository.AddCity(city);
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return city.Id;
    }
}
