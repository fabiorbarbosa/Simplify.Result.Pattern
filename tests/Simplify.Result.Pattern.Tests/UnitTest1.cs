using Microsoft.AspNetCore.Mvc;
using Simplify.Result.Pattern.Enums;
using Simplify.Result.Pattern.Extensions;
using Simplify.Result.Pattern.Results;

namespace Simplify.Result.Pattern.Tests;

public class ResultExtensionTests
{
    [Fact]
    public void ToObjectResult_ReturnsOkObjectResult_ForSuccessResult()
    {
        var result = Result<string>.Success("payload");

        var response = result.ToObjectResult();

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Equal("payload", ok.Value);
    }

    [Fact]
    public void ToObjectResult_ReturnsNotFoundObjectResult_ForMissingResource()
    {
        var result = Result<string>.NotFound("missing resource");

        var response = result.ToObjectResult();

        var notFound = Assert.IsType<NotFoundObjectResult>(response);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(notFound.Value);
        Assert.Contains("missing resource", errors);
    }

    [Fact]
    public void OnSuccess_ExecutesCallback_WhenResultHasValue()
    {
        var result = Result<int>.Success(42);
        var executed = false;

        result.OnSuccess(value =>
        {
            executed = true;
            Assert.Equal(42, value);
        });

        Assert.True(executed);
    }

    [Fact]
    public void OnFailure_ExecutesCallback_WhenErrorsExist()
    {
        var result = Result<string>.Failure(ResultType.Failure, "error");
        var executed = false;

        result.OnFailure(errors =>
        {
            executed = true;
            Assert.Contains("error", errors);
        });

        Assert.True(executed);
    }

    [Fact]
    public void ToObjectResult_WrapsSuccessPayload_WhenFlagEnabled()
    {
        var result = Result<string>.Success("wrapped", wrapInData: true);

        var response = result.ToObjectResult();

        var ok = Assert.IsType<OkObjectResult>(response);
        var dataProperty = ok.Value?.GetType().GetProperty("data");
        Assert.NotNull(dataProperty);
        Assert.Equal("wrapped", dataProperty!.GetValue(ok.Value));
    }

    [Fact]
    public void ToObjectResult_ReturnsUnauthorizedErrors()
    {
        var result = Result<string>.Unauthorized("not allowed");

        var response = result.ToObjectResult();

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response);
        var errorsProp = unauthorized.Value?.GetType().GetProperty("errors");
        Assert.NotNull(errorsProp);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProp!.GetValue(unauthorized.Value));
        Assert.Contains("not allowed", errors);
    }

    [Fact]
    public void Failure_ThrowsWhenErrorsCollectionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Failure(ResultType.Failure, (IEnumerable<string>)null!));
    }

    [Fact]
    public void Failure_ThrowsWhenErrorMessageIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(ResultType.Failure, string.Empty));
    }
}
