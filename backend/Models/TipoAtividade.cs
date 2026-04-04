namespace Backend.Models;

/// <summary>
/// Tipos de atividade que podem compor a programação de um evento acadêmico.
/// </summary>
public enum TipoAtividade
{
    /// <summary>Palestra com exposição de tema por especialista.</summary>
    Palestra,

    /// <summary>Minicurso com carga horária reduzida.</summary>
    Minicurso,

    /// <summary>Mesa redonda com debate entre convidados.</summary>
    MesaRedonda,

    /// <summary>Apresentação de trabalhos científicos.</summary>
    ApresentacaoTrabalho,

    /// <summary>Outro formato não listado explicitamente.</summary>
    Outro
}
