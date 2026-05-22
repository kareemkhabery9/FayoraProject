using System;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.CancelPushCampaign;

public record CancelPushCampaignCommand(Guid Id) : ICommand<Result<Success>>;
