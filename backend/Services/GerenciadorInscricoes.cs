using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Controla inscrições: evita duplicidade (mesmo participante no mesmo evento),
/// valida existência de entidades e estado do evento.
/// </summary>
public class GerenciadorInscricoes
{
    private readonly ContextoDados _ctx;

    public GerenciadorInscricoes(ContextoDados ctx) => _ctx = ctx;

    /// <summary>
    /// Realiza inscrição. Lança <see cref="InscricaoDuplicadaException"/> se já inscrito.
    /// </summary>
    public Inscricao RealizarInscricao(Guid participanteId, Guid eventoId)
    {
        var participante = _ctx.Participantes.FirstOrDefault(p => p.Id == participanteId)
            ?? throw new DadosInvalidosException("Participante não encontrado.");

        var evento = _ctx.Eventos.FirstOrDefault(e => e.Id == eventoId)
            ?? throw new DadosInvalidosException("Evento não encontrado.");

        if (!evento.InscricoesAbertas)
            throw new DadosInvalidosException("Este evento não está aceitando novas inscrições.");

        var duplicada = _ctx.Inscricoes.Any(i =>
            i.ParticipanteId == participanteId &&
            i.EventoId == eventoId &&
            i.Status != StatusInscricao.Cancelada);

        if (duplicada)
            throw new InscricaoDuplicadaException(
                $"O participante '{participante.Nome}' já possui inscrição ativa neste evento.");

        var inscricao = new Inscricao
        {
            ParticipanteId = participanteId,
            EventoId = eventoId,
            Status = StatusInscricao.Confirmada
        };

        try
        {
            inscricao.Validar();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }

        _ctx.Inscricoes.Add(inscricao);
        return inscricao;
    }

    public IReadOnlyList<Inscricao> Listar() => _ctx.Inscricoes.AsReadOnly();

    /// <summary>LINQ: inscrições de um participante específico.</summary>
    public IEnumerable<Inscricao> BuscarPorParticipante(Guid participanteId) =>
        _ctx.Inscricoes.Where(i => i.ParticipanteId == participanteId);

    /// <summary>LINQ: inscrições confirmadas de um evento.</summary>
    public IEnumerable<Inscricao> ListarConfirmadasPorEvento(Guid eventoId) =>
        _ctx.Inscricoes.Where(i => i.EventoId == eventoId && i.Status == StatusInscricao.Confirmada);

    public Inscricao? ObterPorId(Guid id) => _ctx.Inscricoes.FirstOrDefault(i => i.Id == id);

    /// <summary>
    /// Dicionário eventoId → quantidade de inscrições confirmadas (uso de Dictionary conforme requisito).
    /// </summary>
    public Dictionary<Guid, int> ContagemConfirmadasPorEvento() =>
        _ctx.Inscricoes
            .Where(i => i.Status == StatusInscricao.Confirmada)
            .GroupBy(i => i.EventoId)
            .ToDictionary(g => g.Key, g => g.Count());
}
