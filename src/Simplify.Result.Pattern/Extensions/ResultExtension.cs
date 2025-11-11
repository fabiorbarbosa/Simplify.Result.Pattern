using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simplify.Result.Pattern.Enums;
using Simplify.Result.Pattern.Results;

namespace Simplify.Result.Pattern.Extensions;

/// <summary>
/// Extension methods for the Result class.
/// </summary>
public static class ResultExtension
{
    /// <summary>
    /// Converts a Result object to an IActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    /// <param name="result">The Result object to convert.</param>
    /// <returns>An IActionResult based on the result type.</returns>
    public static IActionResult ToObjectResult<T>(this Result<T> result)
        => result.Type switch
        {
            ResultType.Success when result.StatusCode is null
                => new OkObjectResult(FormatSuccessPayload(result)),

            ResultType.Success when result.StatusCode is not null
                => new ObjectResult(FormatSuccessPayload(result)) { StatusCode = (int)result.StatusCode },

            ResultType.Created when result.ActionName is not null && result.RouteValues is not null
                => new CreatedAtActionResult(result.ActionName, null, result.RouteValues, result.Value),

            ResultType.NoContent
                => new NoContentResult(),

            ResultType.Failure
                => new ObjectResult(new { errors = result.Errors }) { StatusCode = 500 },

            ResultType.NotFound
                => new NotFoundObjectResult(result.Errors),

            ResultType.Validation
                => new UnprocessableEntityObjectResult((object?)result.Errors ?? (object?)result.Value),

            ResultType.Conflict
                => new ConflictObjectResult(result.Errors),

            ResultType.Unauthorized
                => new UnauthorizedObjectResult(new { errors = result.Errors }),

            _ => new ObjectResult(result.Value) { StatusCode = (int?)(result.StatusCode ?? HttpStatusCode.OK) }
        };

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    /// <param name="result">The Result object to execute the action on.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <param name="logger">The logger to use for logging errors.</param>
    /// <returns>The Result object.</returns>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action, ILogger? logger = default)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            try
            {
                action(result.Value);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error executing success action.");
            }
        }

        return result;
    }

    /// <summary>
    /// Executes an action if the result is not successful.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    /// <param name="result">The Result object to execute the action on.</param>
    /// <param name="action">The action to execute with the error collection.</param>
    /// <param name="logger">The logger to use for logging errors.</param>
    /// <returns>The Result object.</returns>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IEnumerable<string>> action, ILogger? logger = default)
    {
        if (!result.IsSuccess && result.Errors is not null)
        {
            try
            {
                action(result.Errors);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error executing error action.");
            }
        }

        return result;
    }

    private static object? FormatSuccessPayload<T>(Result<T> result)
        => result.WrapInData ? new { data = result.Value } : result.Value;
}
