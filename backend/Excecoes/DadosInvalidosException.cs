namespace Backend.Excecoes;

/// <summary>
/// Erro de validação de domínio (dados inconsistentes ou incompletos).
/// </summary>
public class DadosInvalidosException : Exception
{
    public DadosInvalidosException(string mensagem) : base(mensagem) { }

    public DadosInvalidosException(string mensagem, Exception interna) : base(mensagem, interna) { }
}
