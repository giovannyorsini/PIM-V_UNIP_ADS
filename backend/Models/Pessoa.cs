namespace Backend.Models;

/// <summary>
/// Classe base abstrata para representar qualquer pessoa no sistema (participante ou administrador).
/// Demonstra herança e encapsulamento: atributos comuns centralizados e Id gerado de forma controlada.
/// </summary>
public abstract class Pessoa
{
    private string _nome = string.Empty;
    private string _email = string.Empty;

    /// <summary>Identificador único da pessoa no sistema.</summary>
    public Guid Id { get; protected set; }

    /// <summary>Nome completo utilizado em certificados e listagens.</summary>
    public string Nome
    {
        get => _nome;
        set => _nome = value ?? string.Empty;
    }

    /// <summary>E-mail para contato e, no caso do participante, pode servir como chave lógica.</summary>
    public string Email
    {
        get => _email;
        set => _email = value ?? string.Empty;
    }

    /// <summary>Data em que o cadastro foi criado (auditoria simples).</summary>
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    protected Pessoa()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Valida dados mínimos obrigatórios para qualquer pessoa.
    /// Regra de negócio: nome e e-mail não podem ser vazios; e-mail deve conter '@'.
    /// </summary>
    public virtual void ValidarDados()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            throw new ArgumentException("O nome da pessoa é obrigatório.");

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@', StringComparison.Ordinal))
            throw new ArgumentException("Informe um e-mail válido.");
    }

    public override string ToString() => $"{Nome} <{Email}>";
}
