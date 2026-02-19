using System.ComponentModel.DataAnnotations;

namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Endpoint filter that validates any request parameter decorated with
/// <see cref="System.ComponentModel.DataAnnotations"/> attributes and returns
/// a 400 ValidationProblem response when validation fails.
/// Apply it to a route group or individual endpoint with
/// <c>.AddEndpointFilter&lt;ValidationFilter&lt;T&gt;&gt;()</c>.
/// </summary>
/// <typeparam name="T">The request type to validate.</typeparam>
public sealed class ValidationFilter<T> : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the first argument that is of type T and validate it.
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is not null)
        {
            var validationErrors = new List<ValidationResult>();
            var validationContext = new ValidationContext(argument);

            if (!Validator.TryValidateObject(argument, validationContext, validationErrors, validateAllProperties: true))
            {
                // Group errors by member name to produce a standard ValidationProblem dictionary.
                var errors = validationErrors
                    .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage ?? "Invalid value.").ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
