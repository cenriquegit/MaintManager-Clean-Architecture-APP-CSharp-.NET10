using FluentValidation;
using MaintManager.Shared.Models;

namespace MaintManager.Application.Validators;

public sealed class LotCreateValidator : AbstractValidator<LotCreateRequest>
{
    public LotCreateValidator()
    {
        RuleFor(x => x.Mateid)
            .GreaterThan(0).WithMessage("El material es obligatorio.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("El costo unitario no puede ser negativo.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("La fecha de vencimiento debe ser futura.")
            .When(x => x.ExpirationDate.HasValue);
    }
}