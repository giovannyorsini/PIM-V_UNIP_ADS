using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Gera certificados apenas para inscrições confirmadas e simula gravação em arquivo .txt.
/// </summary>
public class GerenciadorCertificados
{
    private readonly ContextoDados _ctx;
    private readonly string _pastaCertificados;

    public GerenciadorCertificados(ContextoDados ctx, string? pastaCertificados = null)
    {
        _ctx = ctx;
        _pastaCertificados = pastaCertificados ?? Path.Combine(AppContext.BaseDirectory, "CertificadosEmitidos");
    }

    /// <summary>
    /// Monta o texto do certificado e opcionalmente salva em disco.
    /// Regra: não emite duas vezes para a mesma inscrição (idempotência simples).
    /// </summary>
    public Certificado GerarParaInscricao(Guid inscricaoId, bool salvarArquivo = true)
    {
        var inscricao = _ctx.Inscricoes.FirstOrDefault(i => i.Id == inscricaoId)
            ?? throw new DadosInvalidosException("Inscrição não encontrada.");

        if (inscricao.Status != StatusInscricao.Confirmada)
            throw new DadosInvalidosException("Só é possível emitir certificado para inscrição confirmada.");

        var existente = _ctx.Certificados.FirstOrDefault(c => c.InscricaoId == inscricaoId);
        if (existente != null)
            return existente;

        var participante = _ctx.Participantes.FirstOrDefault(p => p.Id == inscricao.ParticipanteId)
            ?? throw new DadosInvalidosException("Participante da inscrição não encontrado.");

        var evento = _ctx.Eventos.FirstOrDefault(e => e.Id == inscricao.EventoId)
            ?? throw new DadosInvalidosException("Evento da inscrição não encontrado.");

        var atividades = _ctx.Atividades
            .Where(a => a.EventoId == evento.Id)
            .OrderBy(a => a.DataHoraInicio)
            .Select(a => $"  - {a.Titulo} ({a.Tipo})")
            .ToList();

        var programacao = atividades.Count > 0
            ? string.Join(Environment.NewLine, atividades)
            : "  (Programação detalhada não cadastrada no sistema.)";

        var texto = $"""
            ============================================
            CERTIFICADO DE PARTICIPAÇÃO
            ============================================
            Certificamos que {participante.Nome}
            instituição: {participante.Instituicao}
            e-mail: {participante.Email}

            participou do evento "{evento.Titulo}",
            realizado em {evento.Local}, no período de {evento.DataInicio:dd/MM/yyyy} a {evento.DataFim:dd/MM/yyyy},
            com carga horária declarada de {evento.CargaHorariaHoras} hora(s).

            Programação registrada:
            {programacao}

            Emitido em {DateTime.Now:dd/MM/yyyy HH:mm} (simulação acadêmica PIM V)
            ============================================
            """;

        var cert = new Certificado
        {
            InscricaoId = inscricao.Id,
            ParticipanteId = participante.Id,
            EventoId = evento.Id,
            ConteudoTexto = texto,
            DataEmissao = DateTime.UtcNow
        };

        cert.Validar();

        if (salvarArquivo)
        {
            try
            {
                Directory.CreateDirectory(_pastaCertificados);
                var nomeArquivo = $"cert_{cert.Id:N}.txt";
                var caminho = Path.Combine(_pastaCertificados, nomeArquivo);
                File.WriteAllText(caminho, texto);
                cert.CaminhoArquivo = caminho;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Certificado permanece em memória; logamos e re-lançamos como persistência parcial
                throw new PersistenciaException("Certificado gerado em memória, mas falhou ao salvar arquivo.", ex);
            }
        }

        _ctx.Certificados.Add(cert);
        return cert;
    }

    public IReadOnlyList<Certificado> Listar() => _ctx.Certificados.AsReadOnly();
}
