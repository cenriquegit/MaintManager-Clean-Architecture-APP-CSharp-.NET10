using FluentValidation;
using MaintManager.Application.DTOs.Inventory; // o el namespace que corresponda
public sealed class MaterialCreateValidator : AbstractValidator<MaterialCreateRequest>
{
    public MaterialCreateValidator()
    {
        RuleFor(x => x.Macaid)
            .GreaterThan((short)0).WithMessage("La categoría del material es obligatoria.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del material es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
            .MaximumLength(30).WithMessage("La unidad de medida no puede superar 30 caracteres.");

        RuleFor(x => x.StockMinimum)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");
    }
}

/// <summary>Validador para ingresar un lote de material.</summary>
