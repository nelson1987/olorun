using FluentValidation.Results;

namespace SharedDomain.Configs;

public static class FluentValidationExtensions
{
    public static bool IsInvalid(this ValidationResult result) => !result.IsValid;
}
