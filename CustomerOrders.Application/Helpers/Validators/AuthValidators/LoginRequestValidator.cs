﻿using CustomerOrders.Application.DTOs.RequestDtos;
using FluentValidation;
namespace CustomerOrders.Application.Helpers.Validators.AuthValidators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
