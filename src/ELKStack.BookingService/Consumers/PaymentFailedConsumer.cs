using ELKStack.BookingService.State;
using ELKStack.Contracts;
using ELKStack.Observability;
using MassTransit;

namespace ELKStack.BookingService.Consumers;

public sealed class PaymentFailedConsumer(
    BookingState state,
    IConfiguration configuration,
    ILogger<PaymentFailedConsumer> logger) : IConsumer<PaymentFailed>
{
    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        state.MarkPaymentFailed(context.Message);
        logger.LogForStage(
            configuration,
            LogLevel.Warning,
            "Booking failed",
            "Booking {BookingId} failed because payment {PaymentId} failed: {Reason}",
            context.Message.BookingId,
            context.Message.PaymentId,
            context.Message.Reason);

        return Task.CompletedTask;
    }
}
