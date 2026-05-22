using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.CancelBooking;

public class CancelBookingCommandHandler(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : ICommandHandler<CancelBookingCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await adminRepository.GetBookingByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            return Error.NotFound($"Booking with ID {request.BookingId} not found.");

        var cancelResult = booking.Cancel(request.Reason);
        if (cancelResult.IsError)
            return cancelResult.Errors;

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
