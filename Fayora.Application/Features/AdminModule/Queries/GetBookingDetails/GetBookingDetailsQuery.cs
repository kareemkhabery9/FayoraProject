using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.GetBookings;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Queries.GetBookingDetails;

public record GetBookingDetailsQuery(Guid BookingId) : IQuery<Result<GetBookingDetailsResponse>>;
