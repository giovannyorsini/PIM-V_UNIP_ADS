using Backend.Excecoes;
using Backend.Persistencia;

namespace Backend.Services;

/// <summary>
/// Fachada que agrupa contexto, repositório e todos os gerenciadores.
/// Facilita o uso no Program e demonstra composição de serviços.
/// </summary>
public class SistemaPimV
{
    public ContextoDados Contexto { get; }
    public RepositorioJson Repositorio { get; }

    public GerenciadorEventos Eventos { get; }
    public GerenciadorParticipantes Participantes { get; }
    public GerenciadorAdministradores Administradores { get; }
    public GerenciadorAtividades Atividades { get; }
    public GerenciadorInscricoes Inscricoes { get; }
    public GerenciadorCertificados Certificados { get; }

    public SistemaPimV(string? caminhoJson = null)
    {
        Contexto = new ContextoDados();
        Repositorio = new RepositorioJson(caminhoJson);

        Eventos = new GerenciadorEventos(Contexto);
        Participantes = new GerenciadorParticipantes(Contexto);
        Administradores = new GerenciadorAdministradores(Contexto);
        Atividades = new GerenciadorAtividades(Contexto);
        Inscricoes = new GerenciadorInscricoes(Contexto);
        Certificados = new GerenciadorCertificados(Contexto);
    }

    /// <summary>Carrega estado do JSON para memória.</summary>
    public void CarregarDados()
    {
        var snap = Repositorio.Carregar();
        Contexto.Hidratar(snap);
    }

    /// <summary>Persiste o estado atual.</summary>
    public void SalvarDados()
    {
        Repositorio.Salvar(Contexto.ParaSnapshot());
    }
}
