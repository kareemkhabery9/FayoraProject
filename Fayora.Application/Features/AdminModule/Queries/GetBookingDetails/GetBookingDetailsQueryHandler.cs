using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.GetBookings;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetBookingDetails;

public class GetBookingDetailsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetBookingDetailsQuery, Result<GetBookingDetailsResponse>>
{
    public async Task<Result<GetBookingDetailsResponse>> Handle(GetBookingDetailsQuery request, CancellationToken cancellationToken)
    {
        var details = await adminRepository.GetBookingDetailsAsync(request.BookingId, cancellationToken);
        if (details is null)
            return Error.NotFound($"Booking with ID {request.BookingId} not found.");

        return details;
    }
}
