using CustomerOrders.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Helpers
{
    public static class ValidationHelper
    {
        // General validation method
        public static async Task<IActionResult> ValidateRequest<T>(IValidator<T> validator, T request)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
            return null;
        }

        // ID Validation method
        public static async Task<IActionResult> ValidateId(int id, IValidator<int> idValidator)
        {
            var validationResult = idValidator.Validate(id);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
            return null;
        }
    }
}
