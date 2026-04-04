using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Operações de criação, edição e listagem de eventos.
/// Utiliza LINQ para ordenação e filtros quando aplicável.
/// </summary>
public class GerenciadorEventos
{
    private readonly ContextoDados _ctx;

    public GerenciadorEventos(ContextoDados ctx) => _ctx = ctx;

    /// <summary>Cadastra novo evento após validação.</summary>
    public Evento Criar(Evento evento)
    {
        try
        {
            evento.Validar();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }

        _ctx.Eventos.Add(evento);
        return evento;
    }

    /// <summary>Atualiza campos de um evento existente pelo Id.</summary>
    public void Editar(Guid id, Action<Evento> mutador)
    {
        var ev = _ctx.Eventos.FirstOrDefault(e => e.Id == id);
        if (ev == null)
            throw new DadosInvalidosException("Evento não encontrado para edição.");

        mutador(ev);

        try
        {
            ev.Validar();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }
    }

    /// <summary>Lista todos os eventos (sem ordenação específica).</summary>
    public IReadOnlyList<Evento> Listar() => _ctx.Eventos.AsReadOnly();

    /// <summary>
    /// LINQ: eventos ordenados por data de início (mais próximos primeiro).
    /// </summary>
    public IEnumerable<Evento> ListarOrdenadosPorDataInicio() =>
        _ctx.Eventos.OrderBy(e => e.DataInicio);

    /// <summary>LINQ: busca por título (contém, ignorando maiúsculas).</summary>
    public IEnumerable<Evento> BuscarPorTitulo(string trecho) =>
        _ctx.Eventos.Where(e => e.Titulo.Contains(trecho, StringComparison.OrdinalIgnoreCase));

    public Evento? ObterPorId(Guid id) => _ctx.Eventos.FirstOrDefault(e => e.Id == id);
}
