using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.Extensions;

public static class ControllerValidationExtensions
{
    extension(ControllerBase controller)
    {
        public async Task<IActionResult?> ValidateModelAsync<T>(
            IValidator<T> validator,
            T instance)
        {
            var validationResult = await validator.ValidateAsync(instance);

            if (validationResult.IsValid) return null;

            foreach (var error in validationResult.Errors)
                controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return controller.ValidationProblem(controller.ModelState);
        }
    }
}