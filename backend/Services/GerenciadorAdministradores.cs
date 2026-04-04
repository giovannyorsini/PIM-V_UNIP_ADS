using Backend.Excecoes;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Gerencia usuários administradores (entidade obrigatória do domínio).
/// </summary>
public class GerenciadorAdministradores
{
    private readonly ContextoDados _ctx;

    public GerenciadorAdministradores(ContextoDados ctx) => _ctx = ctx;

    public UsuarioAdministrador Cadastrar(UsuarioAdministrador admin)
    {
        try
        {
            admin.ValidarDados();
        }
        catch (ArgumentException ex)
        {
            throw new DadosInvalidosException(ex.Message, ex);
        }

        if (_ctx.Administradores.Any(a => a.Login.Equals(admin.Login, StringComparison.OrdinalIgnoreCase)))
            throw new DadosInvalidosException("Login de administrador já está em uso.");

        _ctx.Administradores.Add(admin);
        return admin;
    }

    public IReadOnlyList<UsuarioAdministrador> Listar() => _ctx.Administradores.AsReadOnly();
}
