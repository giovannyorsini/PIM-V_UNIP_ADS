using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Cadastro de atividades e associação obrigatória a um evento existente.
/// </summary>
public class GerenciadorAtividades
{
    private readonly ContextoDados _ctx;

    public GerenciadorAtividades(ContextoDados ctx) => _ctx = ctx;

    /// <summary>
    /// Registra atividade vinculada ao evento. Verifica se o evento existe.
    /// </summary>
    public Atividade Cadastrar(Atividade atividade)
    {
        var eventoExiste = _ctx.Eventos.Any(e => e.Id == atividade.EventoId);
        if (!eventoExiste)
            throw new DadosInvalidosException("Não é possível cadastrar atividade: evento não encontrado.");

        try
        {
            atividade.Validar();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }

        _ctx.Atividades.Add(atividade);
        return atividade;
    }

    /// <summary>LINQ: atividades de um evento, ordenadas por data/hora de início.</summary>
    public IEnumerable<Atividade> ListarPorEventoOrdenadas(Guid eventoId) =>
        _ctx.Atividades
            .Where(a => a.EventoId == eventoId)
            .OrderBy(a => a.DataHoraInicio);

    public IReadOnlyList<Atividade> ListarTodas() => _ctx.Atividades.AsReadOnly();
}
