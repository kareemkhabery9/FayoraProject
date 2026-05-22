using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.CancelBooking;

public record CancelBookingCommand(Guid BookingId, string Reason) : ICommand<Result<Success>>;
