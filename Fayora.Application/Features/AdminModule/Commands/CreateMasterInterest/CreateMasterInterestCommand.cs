using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.CreateMasterInterest;

public record CreateMasterInterestCommand(
    string Code,
    string Name,
    string IconUrl,
    int SortOrder) : ICommand<Result<int>>;
