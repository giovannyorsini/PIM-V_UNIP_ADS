namespace Backend.Excecoes;

/// <summary>
/// Lançada quando o participante tenta se inscrever novamente no mesmo evento.
/// </summary>
public class InscricaoDuplicadaException : Exception
{
    public InscricaoDuplicadaException(string mensagem) : base(mensagem) { }

    public InscricaoDuplicadaException(string mensagem, Exception interna) : base(mensagem, interna) { }
}
