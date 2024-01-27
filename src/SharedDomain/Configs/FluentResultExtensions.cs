using FluentResults;
using FluentValidation.Results;

namespace SharedDomain.Configs;

public static class FluentResultExtensions
{
    public static Result ToFailResult(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(x => new Error(x.ErrorMessage)
            .WithMetadata(nameof(x.PropertyName), x.PropertyName)
            .WithMetadata(nameof(x.AttemptedValue), x.AttemptedValue));

        return Result.Fail(errors);
    }
}
