namespace ELKStack.PaymentService.Models;

public sealed record PaymentRecord(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string? TransactionId,
    string? FailureReason = null);
