// MaintManager.Application/Validators/AllValidators.cs
using FluentValidation;
using MaintManager.Application.DTOs.Auth;
using MaintManager.Application.DTOs.Inventory;
using MaintManager.Application.DTOs.Maintenance;

namespace MaintManager.Application.Validators;

/// <summary>Validador de credenciales de login.</summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(25).WithMessage("El usuario no puede superar 25 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

/// <summary>Validador para crear una orden de mantenimiento.</summary>
public sealed class MaintenanceCreateValidator : AbstractValidator<MaintenanceCreateRequest>
{
    public MaintenanceCreateValidator()
    {
        RuleFor(x => x.Prcoid)
            .GreaterThan(0).WithMessage("El vehículo es obligatorio.");

        RuleFor(x => x.Matyid)
            .InclusiveBetween((short)1, (short)2)
            .WithMessage("El tipo de mantenimiento debe ser 1 (Calendarizado) o 2 (Emergencia).");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

        RuleFor(x => x.AssignedTo)
            .GreaterThan(0).WithMessage("Debe asignar un mecánico a la orden.");

        // Calendarizado requiere tipo de servicio A o B
        RuleFor(x => x.Setyid)
            .NotNull().WithMessage("El tipo de servicio (A o B) es obligatorio para mantenimientos calendarizados.")
            .When(x => x.Matyid == 1);

        // Emergencia NO debe tener tipo de servicio
        RuleFor(x => x.Setyid)
            .Null().WithMessage("Los mantenimientos de emergencia no tienen tipo de servicio A/B.")
            .When(x => x.Matyid == 2);

        RuleFor(x => x.OriginService)
            .Must(v => v == "Taller propio" || v == "Taller externo")
            .WithMessage("El origen del servicio debe ser 'Taller propio' o 'Taller externo'.");
    }
}

/// <summary>Validador para crear un material.</summary>
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

/// <summary>Validador para calificación de materiales.</summary>
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
public sealed class DiagnosisRequestValidator : AbstractValidator<DiagnosisRequest>
{
    public DiagnosisRequestValidator()
    {
        RuleFor(x => x.GeneralStatus)
            .NotEmpty().WithMessage("El estado general del vehículo es obligatorio.")
            .MaximumLength(100).WithMessage("El estado general no puede superar 100 caracteres.");
    }
}
