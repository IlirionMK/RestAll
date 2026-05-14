using FluentValidation;
using RestAll.Desktop.Core.Reservations;

namespace RestAll.Desktop.App.Validation;

public class ReservationValidator : AbstractValidator<Reservation>
{
    public ReservationValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\d{9,15}$").WithMessage("Phone number must be between 9 and 15 digits");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.TableId)
            .GreaterThan(0).WithMessage("Table ID must be greater than 0");

        RuleFor(x => x.NumberOfGuests)
            .GreaterThan(0).WithMessage("Number of guests must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("Number of guests cannot exceed 20");

        RuleFor(x => x.ReservationDate)
            .NotEmpty().WithMessage("Reservation date is required")
            .Must(date => date.Date >= DateTime.Today).WithMessage("Reservation date cannot be in the past");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(500).WithMessage("Special requests cannot exceed 500 characters");
    }
}
