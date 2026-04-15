
/// <summary>
/// Calificación (1-5 estrellas) de un material durante un mantenimiento.
/// Si rating ≤ 3, la observación es obligatoria.
/// </summary>
namespace MaintManager.Domain.Entities;

public sealed class MaterialRating
{
    public int Matraid { get; private set; }
    public int Mateid { get; private set; }
    public int Mainid { get; private set; }
    public short Rating { get; private set; }
    public string? Observation { get; private set; }
    public int RatedBy { get; private set; }
    public DateTime RatedAt { get; private set; }

    private MaterialRating() { }

    public static MaterialRating Create(int mateid, int mainid, short rating, int ratedBy, string? observation = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("La calificación debe estar entre 1 y 5.", nameof(rating));
        if (rating <= 3 && string.IsNullOrWhiteSpace(observation))
            throw new InvalidOperationException("La observación es obligatoria cuando la calificación es 3 o menor.");

        return new MaterialRating
        {
            Mateid = mateid,
            Mainid = mainid,
            Rating = rating,
            Observation = observation,
            RatedBy = ratedBy,
            RatedAt = DateTime.UtcNow
        };
    }
}


