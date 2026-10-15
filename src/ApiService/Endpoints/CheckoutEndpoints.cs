using ApiService.Dtos;
using ApiService.Services;
using Stripe;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public static class CheckoutEndpoints
{
    public static RouteGroupBuilder MapCheckoutEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/checkout").WithTags("Checkout");

        group.MapPost("create-session", async (CheckoutRequest request, StripeCheckoutService service) =>
        {
            try
            {
                return Results.Ok(await service.CreateCheckoutSessionAsync(request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (StripeException ex)
            {
                return StripeError(ex);
            }
        });

        group.MapPost("confirm", async (CheckoutConfirmRequest? request, StripeCheckoutService service) =>
        {
            try
            {
                return Results.Ok(await service.ConfirmCheckoutSessionAsync(request?.SessionId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PaymentNotCompletedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status402PaymentRequired);
            }
            catch (StripeException ex)
            {
                return StripeError(ex);
            }
        });

        return group;
    }

    private static IResult StripeError(StripeException ex) =>
        Results.Json(new { error = $"Stripe error: {ex.Message}" }, statusCode: StatusCodes.Status502BadGateway);
}
