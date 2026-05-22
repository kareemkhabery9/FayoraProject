using System;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.CreatePushCampaign;

public record CreatePushCampaignCommand(
    string Title,
    string Body,
    string? ImageUrl,
    string TargetAudience,
    DateTimeOffset? ScheduledAt) : ICommand<Result<Guid>>;
