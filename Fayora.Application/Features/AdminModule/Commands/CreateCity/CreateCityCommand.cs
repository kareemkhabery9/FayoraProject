using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.CreateCity;

public record CreateCityCommand(
    string Name,
    string CountryCode,
    decimal Latitude,
    decimal Longitude) : ICommand<Result<int>>;
