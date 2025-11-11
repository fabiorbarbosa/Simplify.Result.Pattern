using System.Net;
using Simplify.Result.Pattern.Enums;

namespace Simplify.Result.Pattern.Results;

/// <summary>
/// Represents a result of an operation that can be either successful or failed.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <remarks>
/// This class is used to return a result of an operation that can be either successful or failed.
/// </remarks>
public class Result<T>
{
    public bool IsSuccess { get; }
    public ResultType Type { get; }
    public T? Value { get; }
    public IEnumerable<string>? Errors { get; }
    public string? ActionName { get; }
    public object? RouteValues { get; }
    public HttpStatusCode? StatusCode { get; }
    public bool WrapInData { get; }

    protected Result(
        bool success,
        ResultType type,
        T? value,
        IEnumerable<string>? errors = default,
        string? actionName = default,
        object? routeValues = default,
        HttpStatusCode? statusCode = default,
        bool wrapInData = false)
    {
        if (success && errors is not null && errors.Any())
            throw new ArgumentException("Result of success cannot have errors");

        if (!success && value is not null)
            throw new ArgumentException("Result of error cannot have value");

        IsSuccess = success;
        Type = type;
        Value = value;
        Errors = errors;
        ActionName = actionName;
        RouteValues = routeValues;
        StatusCode = statusCode;
        WrapInData = wrapInData;
    }

    /// <summary>
    /// Creates a successful result with the specified value and status code.
    /// </summary>
    /// <param name="value">The value returned on success.</param>
    /// <param name="statusCode">The status code to return.</param>
    /// <returns>A successful result with the specified value and status code.</returns>
    public static Result<T> Success(T value, HttpStatusCode? statusCode = null, bool wrapInData = false)
        => new(true, ResultType.Success, value, statusCode: statusCode, wrapInData: wrapInData);

    /// <summary>
    /// Creates a successful result with no content.
    /// </summary>
    /// <returns>A successful result with no content.</returns>
    public static Result<T> NoContent()
        => new(true, ResultType.NoContent, default);

    /// <summary>
    /// Creates a successful result with the specified value, action name, and route values.
    /// </summary>
    /// <param name="value">The value returned on success.</param>
    /// <param name="actionName">The name of the action to return.</param>
    /// <param name="routeValues">The route values to return.</param>
    /// <returns>A successful result with the specified value, action name, and route values.</returns>
    public static Result<T> Created(T value, string actionName, object routeValues)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentNullException(nameof(actionName), "The 'actionName' parameter is required.");

        if (routeValues is null)
            throw new ArgumentNullException(nameof(routeValues), "The 'routeValues' parameter is required.");

        return new(true, ResultType.Created, value, actionName: actionName, routeValues: routeValues);
    }

    /// <summary>
    /// Creates a failed result with the specified type and errors.
    /// </summary>
    /// <param name="type">The type of the result.</param>
    /// <param name="errors">The errors to return.</param>
    /// <returns>A failed result with the specified type and errors.</returns>
    public static Result<T> Failure(ResultType type, IEnumerable<string> errors)
        => new(false, type, default, errors: errors ?? throw new ArgumentNullException(nameof(errors)));

    /// <summary>
    /// Creates a failed result with the specified type and error.
    /// </summary>
    /// <param name="type">The type of the result.</param>
    /// <param name="error">The error to return.</param>
    /// <returns>A failed result with the specified type and error.</returns>
    public static Result<T> Failure(ResultType type, string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));

        return new(false, type, default, errors: [error]);
    }

    /// <summary>
    /// Creates a failed result with the type NotFound.
    /// </summary>
    /// <param name="message">The message to return.</param>
    /// <returns>A failed result with the type NotFound.</returns>
    public static Result<T> NotFound(string message = "Resource not found")
        => Failure(ResultType.NotFound, message);

    /// <summary>
    /// Creates a failed result with the type Validation.
    /// </summary>
    /// <param name="errors">The errors to return.</param>
    /// <returns>A failed result with the type Validation.</returns>
    public static Result<T> ValidationError(IEnumerable<string> errors)
        => Failure(ResultType.Validation, errors);

    /// <summary>
    /// Creates a failed result with the type Unauthorized.
    /// </summary>
    /// <param name="message">The message to return.</param>
    /// <returns>A failed result with the type Unauthorized.</returns>
    public static Result<T> Unauthorized(string message = "Unauthorized")
        => Failure(ResultType.Unauthorized, message);

    /// <summary>
    /// Creates a failed result with the type Conflict.
    /// </summary>
    /// <param name="message">The message to return.</param>
    /// <returns>A failed result with the type Conflict.</returns>
    public static Result<T> Conflict(string message = "Conflict detected")
        => Failure(ResultType.Conflict, message);
}
