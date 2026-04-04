namespace Backend.Models;

/// <summary>
/// Certificado emitido com base em uma inscrição confirmada.
/// O texto (<see cref="ConteudoTexto"/>) simula o documento oficial; pode ser persistido em arquivo pelo serviço.
/// </summary>
public class Certificado : IValidavel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Inscrição que fundamenta a emissão.</summary>
    public Guid InscricaoId { get; set; }

    public Guid ParticipanteId { get; set; }

    public Guid EventoId { get; set; }

    /// <summary>Texto completo do certificado (simulação de documento).</summary>
    public string ConteudoTexto { get; set; } = string.Empty;

    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    /// <summary>Caminho do arquivo .txt gerado, se a emissão gravou em disco.</summary>
    public string? CaminhoArquivo { get; set; }

    public void Validar()
    {
        if (InscricaoId == Guid.Empty)
            throw new ArgumentException("Certificado deve referenciar uma inscrição.");

        if (string.IsNullOrWhiteSpace(ConteudoTexto))
            throw new ArgumentException("O conteúdo do certificado não pode ser vazio.");
    }
}
