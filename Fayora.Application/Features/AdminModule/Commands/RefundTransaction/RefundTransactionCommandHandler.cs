using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.RefundTransaction;

public class RefundTransactionCommandHandler(
    IAdminRepository adminRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RefundTransactionCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await adminRepository.GetTransactionByBookingIdAsync(request.BookingId, cancellationToken);
        if (transaction is null)
            return Error.NotFound("Transaction.NotFound", "Payment transaction not found.");

        var refundResult = transaction.MarkAsRefunded();
        if (refundResult.IsError)
            return refundResult.Errors;

        await unitOfWork.CommitChangesAsync(cancellationToken);
        return Result.Success;
    }
}
