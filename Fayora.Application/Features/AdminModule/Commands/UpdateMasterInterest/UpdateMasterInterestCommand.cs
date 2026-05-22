using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateMasterInterest;

public record UpdateMasterInterestCommand(
    int Id,
    string Name,
    string IconUrl,
    int SortOrder) : ICommand<Result<Success>>;
