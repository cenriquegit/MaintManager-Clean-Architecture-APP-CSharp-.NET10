using FluentValidation;
using MaintManager.Application.DTOs.Inventory; // o el namespace que corresponda
public sealed class MaterialRatingValidator : AbstractValidator<MaterialRatingRequest>
{
    public MaterialRatingValidator()
    {
        RuleFor(x => x.Mateid).GreaterThan(0).WithMessage("El material es obligatorio.");
        RuleFor(x => x.Mainid).GreaterThan(0).WithMessage("El mantenimiento es obligatorio.");

        RuleFor(x => x.Rating)
            .InclusiveBetween((short)1, (short)5)
            .WithMessage("La calificación debe estar entre 1 y 5 estrellas.");

        RuleFor(x => x.Observation)
            .NotEmpty().WithMessage("La observación es obligatoria cuando la calificación es 3 o menor.")
            .When(x => x.Rating <= 3);
    }
}

/// <summary>Validador para diagnóstico final.</summary>
