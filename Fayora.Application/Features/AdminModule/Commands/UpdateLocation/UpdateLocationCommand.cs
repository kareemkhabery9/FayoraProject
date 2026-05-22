using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateLocation;

public record UpdateLocationCommand(
    int Id,
    string Name,
    string? Description,
    decimal Rating,
    decimal Latitude,
    decimal Longitude,
    int Category,
    string MainImageUrl,
    List<string> ImageUrls) : ICommand<Result<Success>>;
