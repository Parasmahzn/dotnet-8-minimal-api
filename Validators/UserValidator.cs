using dotnet.Records;
using FluentValidation;

namespace dotnet.Validators;

internal sealed class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required")
        .MaximumLength(5).WithMessage("Name must be atleast 5 character long");

        RuleFor(x => x.Address)
        .NotEmpty().WithMessage("Address is required");

        RuleFor(x => x.ContactInfo)
        .NotNull().WithMessage("Contact Information is required")
        .SetValidator(new ContactInfoValidator());
    }
}