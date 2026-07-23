namespace ELKStack.PaymentService.Models;

using ELKStack.Contracts;

public sealed record CreatePaymentRequest(
    Guid BookingId,
    decimal Amount,
    string Currency = "USD");

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
