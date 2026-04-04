using Backend.Models;
using Backend.Persistencia;

namespace Backend.Services;

/// <summary>
/// Contêiner em memória de todas as coleções do sistema.
/// Os serviços operam sobre estas listas; a persistência serializa o mesmo estado.
/// </summary>
public class ContextoDados
{
    public List<Evento> Eventos { get; } = new();
    public List<Participante> Participantes { get; } = new();
    public List<UsuarioAdministrador> Administradores { get; } = new();
    public List<Atividade> Atividades { get; } = new();
    public List<Inscricao> Inscricoes { get; } = new();
    public List<Certificado> Certificados { get; } = new();

    /// <summary>Preenche as listas a partir de um snapshot (após carregar JSON).</summary>
    public void Hidratar(SnapshotDados snapshot)
    {
        Eventos.Clear();
        Eventos.AddRange(snapshot.Eventos);

        Participantes.Clear();
        Participantes.AddRange(snapshot.Participantes);

        Administradores.Clear();
        Administradores.AddRange(snapshot.Administradores);

        Atividades.Clear();
        Atividades.AddRange(snapshot.Atividades);

        Inscricoes.Clear();
        Inscricoes.AddRange(snapshot.Inscricoes);

        Certificados.Clear();
        Certificados.AddRange(snapshot.Certificados);
    }

    /// <summary>Monta snapshot atual para gravar em disco.</summary>
    public SnapshotDados ParaSnapshot() => new()
    {
        Eventos = Eventos.ToList(),
        Participantes = Participantes.ToList(),
        Administradores = Administradores.ToList(),
        Atividades = Atividades.ToList(),
        Inscricoes = Inscricoes.ToList(),
        Certificados = Certificados.ToList()
    };
}
