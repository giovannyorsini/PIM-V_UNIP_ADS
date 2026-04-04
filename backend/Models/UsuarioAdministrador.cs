namespace Backend.Models;

/// <summary>
/// Usuário com perfil administrativo para operar o sistema (cadastros, relatórios).
/// Também herda de <see cref="Pessoa"/> — mesma base humana, papéis diferentes (polimorfismo conceitual).
/// </summary>
public class UsuarioAdministrador : Pessoa
{
    private string _login = string.Empty;

    /// <summary>Identificador de login único no sistema.</summary>
    public string Login
    {
        get => _login;
        set => _login = value ?? string.Empty;
    }

    /// <summary>
    /// Em produção usaria hash; aqui mantemos texto para simulação acadêmica (não usar em ambiente real).
    /// </summary>
    public string Senha { get; set; } = string.Empty;

    /// <summary>Nível ou perfil administrativo (ex.: coordenador, secretaria).</summary>
    public string Perfil { get; set; } = "Administrador";

    public override void ValidarDados()
    {
        base.ValidarDados();

        if (string.IsNullOrWhiteSpace(Login))
            throw new ArgumentException("O login do administrador é obrigatório.");

        if (string.IsNullOrWhiteSpace(Senha) || Senha.Length < 4)
            throw new ArgumentException("A senha deve ter pelo menos 4 caracteres (simulação acadêmica).");
    }
}
