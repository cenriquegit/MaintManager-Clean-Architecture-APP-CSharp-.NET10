// MaintManager.Domain/Entities/TechnicianAssignment.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Asignación de técnicos a órdenes de mantenimiento.
/// Hoy: 1 registro por orden. Escalable para múltiples técnicos por orden.
/// </summary>
public sealed class TechnicianAssignment
{
    public int Teasid { get; private set; }
    public int Mainid { get; private set; }
    public int Workid { get; private set; }
    public string RoleInJob { get; private set; } = "Principal";
    public DateTime AssignedAt { get; private set; }
    public int AssignedBy { get; private set; }

    private TechnicianAssignment() { }

    public static TechnicianAssignment Create(int mainid, int workid, int assignedBy,
        string roleInJob = "Principal")
    {
        if (string.IsNullOrWhiteSpace(roleInJob))
            throw new ArgumentException("El rol en el trabajo es obligatorio.", nameof(roleInJob));

        return new TechnicianAssignment
        {
            Mainid = mainid,
            Workid = workid,
            RoleInJob = roleInJob,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };
    }
}
