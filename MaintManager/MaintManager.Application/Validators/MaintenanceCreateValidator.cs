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
