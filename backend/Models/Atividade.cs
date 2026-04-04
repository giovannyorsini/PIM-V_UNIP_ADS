namespace Backend.Models;

/// <summary>
/// Atividade pontual vinculada a um evento (palestra, minicurso, etc.).
/// Regra de negócio: toda atividade pertence a exatamente um evento (<see cref="EventoId"/>).
/// </summary>
public class Atividade : IValidavel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Evento ao qual esta atividade está associada.</summary>
    public Guid EventoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public TipoAtividade Tipo { get; set; }

    /// <summary>Quando a atividade ocorre dentro da programação.</summary>
    public DateTime DataHoraInicio { get; set; }

    public DateTime? DataHoraFim { get; set; }

    /// <summary>Nome do palestrante ou facilitador, se houver.</summary>
    public string? Responsavel { get; set; }

    public void Validar()
    {
        if (EventoId == Guid.Empty)
            throw new ArgumentException("A atividade deve estar associada a um evento válido.");

        if (string.IsNullOrWhiteSpace(Titulo))
            throw new ArgumentException("O título da atividade é obrigatório.");

        if (DataHoraFim.HasValue && DataHoraFim < DataHoraInicio)
            throw new ArgumentException("O horário de fim não pode ser anterior ao início.");
    }
}
