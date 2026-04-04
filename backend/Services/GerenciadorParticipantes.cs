using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Cadastro e consulta de participantes com filtros via LINQ.
/// </summary>
public class GerenciadorParticipantes
{
    private readonly ContextoDados _ctx;

    public GerenciadorParticipantes(ContextoDados ctx) => _ctx = ctx;

    public Participante Cadastrar(Participante p)
    {
        try
        {
            p.ValidarDados();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }

        // Regra: e-mail único entre participantes
        if (_ctx.Participantes.Any(x => x.Email.Equals(p.Email, StringComparison.OrdinalIgnoreCase)))
            throw new DadosInvalidosException("Já existe participante cadastrado com este e-mail.");

        _ctx.Participantes.Add(p);
        return p;
    }

    public IReadOnlyList<Participante> Listar() => _ctx.Participantes.AsReadOnly();

    /// <summary>LINQ: filtra por instituição.</summary>
    public IEnumerable<Participante> FiltrarPorInstituicao(string instituicao) =>
        _ctx.Participantes.Where(p =>
            p.Instituicao.Contains(instituicao, StringComparison.OrdinalIgnoreCase));

    /// <summary>LINQ: busca por nome parcial.</summary>
    public IEnumerable<Participante> BuscarPorNome(string trecho) =>
        _ctx.Participantes.Where(p => p.Nome.Contains(trecho, StringComparison.OrdinalIgnoreCase));

    public Participante? ObterPorId(Guid id) => _ctx.Participantes.FirstOrDefault(p => p.Id == id);

    /// <summary>Busca participante pelo e-mail (chave lógica para integração com API web).</summary>
    public Participante? ObterPorEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return _ctx.Participantes.FirstOrDefault(p =>
            p.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
