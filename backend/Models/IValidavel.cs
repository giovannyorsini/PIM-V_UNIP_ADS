namespace Backend.Models;

/// <summary>
/// Contrato para entidades que possuem regras de consistência próprias.
/// Complementa o modelo OO com polimorfismo sobre validação (além da herança em <see cref="Pessoa"/>).
/// </summary>
public interface IValidavel
{
    /// <summary>Executa validações de negócio; lança <see cref="ArgumentException"/> se inválido.</summary>
    void Validar();
}
