using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.ToggleMasterInterest;

public record ToggleMasterInterestCommand(int Id) : ICommand<Result<Success>>;
