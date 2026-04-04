namespace Backend.Models;

/// <summary>
/// Estado da inscrição no fluxo do evento.
/// </summary>
public enum StatusInscricao
{
    Pendente,
    Confirmada,
    Cancelada
}

/// <summary>
/// Liga um <see cref="Participante"/> a um <see cref="Evento"/>.
/// Regra de negócio: par (ParticipanteId, EventoId) deve ser único no sistema.
/// </summary>
public class Inscricao : IValidavel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ParticipanteId { get; set; }

    public Guid EventoId { get; set; }

    public DateTime DataInscricao { get; set; } = DateTime.UtcNow;

    public StatusInscricao Status { get; set; } = StatusInscricao.Confirmada;

    public void Validar()
    {
        if (ParticipanteId == Guid.Empty || EventoId == Guid.Empty)
            throw new ArgumentException("Participante e evento são obrigatórios na inscrição.");
    }
}
