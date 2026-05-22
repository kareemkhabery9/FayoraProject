using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.RefundTransaction;

public record RefundTransactionCommand(Guid BookingId) : ICommand<Result<Success>>;
