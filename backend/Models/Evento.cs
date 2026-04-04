namespace Backend.Models;

/// <summary>
/// Representa um evento acadêmico (congresso, semana científica, etc.).
/// As atividades são referenciadas por Id e gerenciadas pelo serviço correspondente.
/// </summary>
public class Evento : IValidavel
{
    /// <summary>Identificador único do evento.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Título exibido em inscrições e certificados.</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Descrição resumida ou completa do evento.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Data/hora de início do evento.</summary>
    public DateTime DataInicio { get; set; }

    /// <summary>Data/hora de término do evento.</summary>
    public DateTime DataFim { get; set; }

    /// <summary>Local físico ou link virtual.</summary>
    public string Local { get; set; } = string.Empty;

    /// <summary>Carga horária total declarada (útil para certificado).</summary>
    public int CargaHorariaHoras { get; set; }

    /// <summary>Indica se o evento ainda aceita novas inscrições.</summary>
    public bool InscricoesAbertas { get; set; } = true;

    /// <summary>
    /// Valida consistência temporal e campos obrigatórios.
    /// </summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
            throw new ArgumentException("O título do evento é obrigatório.");

        if (DataFim < DataInicio)
            throw new ArgumentException("A data de término não pode ser anterior à data de início.");

        if (CargaHorariaHoras < 0)
            throw new ArgumentException("A carga horária não pode ser negativa.");
    }
}
