using FluentValidation;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.Validation;

public class UserProfileValidator : AbstractValidator<UserProfile>
{
    public UserProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");


        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required");
    }
}
