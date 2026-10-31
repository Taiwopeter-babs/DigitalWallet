using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DigitalWallet.Extensions;

public static class ValidationExtensions
{
    public static async Task<Dictionary<string, string[]>?> ValidateModelAsync<T>(
        this IValidator<T> validator,
        T instance, CancellationToken token)
    {
        var validationResult = await validator.ValidateAsync(instance, token);

        if (validationResult.IsValid) return null;

        // Group errors into the format required by Results.ValidationProblem
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );


        return errors;
    }
}