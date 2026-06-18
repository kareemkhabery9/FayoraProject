using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AccommodationModule.Commands.DeleteUnit;

public record DeleteUnitCommand(Guid UnitId) : ICommand<Result<Success>>;
