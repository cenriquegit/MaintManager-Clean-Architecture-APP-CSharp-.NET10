using FluentValidation;
using MaintManager.Application.DTOs.Maintenance;

namespace MaintManager.Application.Validators;

public sealed class DiagnosisRequestValidator : AbstractValidator<DiagnosisRequest>
{
    public DiagnosisRequestValidator()
    {
        RuleFor(x => x.GeneralStatus)
            .NotEmpty().WithMessage("El estado general del vehículo es obligatorio.")
            .MaximumLength(100).WithMessage("El estado general no puede superar 100 caracteres.");
    }
}