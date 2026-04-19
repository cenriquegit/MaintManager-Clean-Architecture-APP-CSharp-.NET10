// MaintManager.Domain/Entities/Existing/Job.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
/// Puesto de trabajo. Mapea la tabla existente public.job.
/// Solo lectura, usado para determinar rol (Admin/Tecnico).
/// </summary>
public sealed class Job
{
    public short Jobid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
}