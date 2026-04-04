namespace Backend.Excecoes;

/// <summary>
/// Falha ao ler ou gravar dados no armazenamento (arquivo JSON, permissões, formato corrompido).
/// </summary>
public class PersistenciaException : Exception
{
    public PersistenciaException(string mensagem) : base(mensagem) { }

    public PersistenciaException(string mensagem, Exception interna) : base(mensagem, interna) { }
}
