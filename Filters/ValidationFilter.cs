using FluentValidation;

namespace dotnet.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Get the validator from DI (registered earlier)
        var validator = context.HttpContext.RequestServices
                                     .GetService<IValidator<T>>();
        if (validator is not null)
        {
            // Find the argument of type T (e.g. the bound model)
            T? model = context.Arguments.OfType<T>().FirstOrDefault();
            if (model is not null)
            {
                var result = await validator.ValidateAsync(model);
                if (!result.IsValid)
                {
                    var ProblemDetails = new HttpValidationProblemDetails(result.ToDictionary())
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occured.",
                    };

                    // Return a ValidationProblem result if invalid
                    return Results.Problem(ProblemDetails);
                }
            }
        }
        // Continue if valid or no validator found
        return await next(context);
    }
}