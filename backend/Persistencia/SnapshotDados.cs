using Backend.Models;

namespace Backend.Persistencia;

/// <summary>
/// Agregação de todas as coleções persistidas em um único documento JSON.
/// Facilita serialização/deserialização atômica do estado da aplicação.
/// </summary>
public class SnapshotDados
{
    public List<Evento> Eventos { get; set; } = new();

    public List<Participante> Participantes { get; set; } = new();

    public List<UsuarioAdministrador> Administradores { get; set; } = new();

    public List<Atividade> Atividades { get; set; } = new();

    public List<Inscricao> Inscricoes { get; set; } = new();

    public List<Certificado> Certificados { get; set; } = new();
}
