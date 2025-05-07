using dotnet.Models;
using FluentValidation;

namespace dotnet.Validators;

public class ContactInfoValidator : AbstractValidator<ContactInfo>
{
    public ContactInfoValidator()
    {
        RuleFor(x => x.MobileNumber)
        .NotEmpty().WithMessage("Mobile Number is required");

        RuleFor(x => x.Office)
        .NotEmpty().WithMessage("Office Contact Number is required");
    }
}