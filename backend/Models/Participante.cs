namespace Backend.Models;

/// <summary>
/// Representa um participante de eventos acadêmicos.
/// Herda de <see cref="Pessoa"/> e adiciona dados institucionais.
/// </summary>
public class Participante : Pessoa
{
    private string _instituicao = string.Empty;
    private string _cpf = string.Empty;

    /// <summary>Instituição de ensino ou empresa de vínculo.</summary>
    public string Instituicao
    {
        get => _instituicao;
        set => _instituicao = value ?? string.Empty;
    }

    /// <summary>CPF (apenas armazenamento textual; validação simplificada para fins acadêmicos).</summary>
    public string Cpf
    {
        get => _cpf;
        set => _cpf = value ?? string.Empty;
    }

    /// <summary>
    /// Regras adicionais: instituição obrigatória; CPF com pelo menos 11 caracteres numéricos se preenchido.
    /// </summary>
    public override void ValidarDados()
    {
        base.ValidarDados();

        if (string.IsNullOrWhiteSpace(Instituicao))
            throw new ArgumentException("A instituição do participante é obrigatória.");

        var digitosCpf = new string(Cpf.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(Cpf) && digitosCpf.Length < 11)
            throw new ArgumentException("CPF informado parece inválido (esperado ao menos 11 dígitos).");
    }
}
