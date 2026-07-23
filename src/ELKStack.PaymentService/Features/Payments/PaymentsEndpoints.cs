namespace ELKStack.PaymentService.Features.Payments;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/payments").WithTags("Payments");

        GetPayments.Map(group);
        GetPaymentById.Map(group);
        CreatePayment.Map(group);

        return endpoints;
    }
}
