using FluentValidation;

namespace TeknikServis.Application.Features.Service.Commands.CreateServiceRecord;

public class CreateServiceRecordValidator : AbstractValidator<CreateServiceRecordCommand>
{
    public CreateServiceRecordValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Müşteri seçilmelidir.");
        RuleFor(x => x.DeviceModelId).GreaterThan(0).WithMessage("Cihaz modeli seçilmelidir.");
        RuleFor(x => x.FaultDescription)
            .NotEmpty().WithMessage("Arıza açıklaması zorunludur.")
            .MaximumLength(1000);
    }
}
