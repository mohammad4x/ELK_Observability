using System.Collections.Concurrent;
using ELKStack.Contracts;
using ELKStack.PaymentService.Models;

namespace ELKStack.PaymentService.State;

public sealed class PaymentState
{
    private readonly ConcurrentDictionary<Guid, PaymentRecord> _payments = new();

    public IReadOnlyCollection<PaymentRecord> All =>
        _payments.Values.OrderByDescending(payment => payment.RequestedAt).ToArray();

    public PaymentRecord? Get(Guid paymentId) =>
        _payments.GetValueOrDefault(paymentId);

    public PaymentRecord? GetByBookingId(Guid bookingId) =>
        _payments.Values.FirstOrDefault(payment => payment.BookingId == bookingId);

    public PaymentRecord MarkRequested(PaymentRequested message)
    {
        return _payments.AddOrUpdate(
            message.PaymentId,
            _ => new PaymentRecord(
                message.PaymentId,
                message.BookingId,
                message.Amount,
                message.Currency,
                "Requested",
                message.RequestedAt,
                null,
                null),
            (_, existing) => existing with
            {
                Status = "Requested",
                RequestedAt = message.RequestedAt
            });
    }

    public PaymentRecord MarkCompleted(PaymentRequested message, string transactionId)
    {
        return _payments.AddOrUpdate(
            message.PaymentId,
            _ => new PaymentRecord(
                message.PaymentId,
                message.BookingId,
                message.Amount,
                message.Currency,
                "Completed",
                message.RequestedAt,
                DateTimeOffset.UtcNow,
                transactionId),
            (_, existing) => existing with
            {
                Status = "Completed",
                CompletedAt = DateTimeOffset.UtcNow,
                TransactionId = transactionId
            });
    }
}
