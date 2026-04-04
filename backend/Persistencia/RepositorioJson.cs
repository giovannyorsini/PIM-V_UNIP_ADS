using System.Text.Json;
using Backend.Excecoes;

namespace Backend.Persistencia;

/// <summary>
/// Responsável por salvar e carregar o <see cref="SnapshotDados"/> em arquivo JSON.
/// Trata erros de I/O e JSON inválido com exceções de domínio.
/// </summary>
public class RepositorioJson
{
    private readonly string _caminhoArquivo;
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public RepositorioJson(string? caminhoArquivo = null)
    {
        // Caminho padrão ao lado do executável / diretório de trabalho
        _caminhoArquivo = caminhoArquivo ?? Path.Combine(AppContext.BaseDirectory, "dados_pimv.json");
    }

    /// <summary>Grava o snapshot completo no disco.</summary>
    public void Salvar(SnapshotDados dados)
    {
        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(_caminhoArquivo));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(dados, Opcoes);
            File.WriteAllText(_caminhoArquivo, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PersistenciaException($"Não foi possível salvar os dados em '{_caminhoArquivo}'.", ex);
        }
    }

    /// <summary>
    /// Carrega dados do arquivo; se não existir, retorna snapshot vazio.
    /// </summary>
    public SnapshotDados Carregar()
    {
        try
        {
            if (!File.Exists(_caminhoArquivo))
                return new SnapshotDados();

            var json = File.ReadAllText(_caminhoArquivo);
            if (string.IsNullOrWhiteSpace(json))
                return new SnapshotDados();

            var dados = JsonSerializer.Deserialize<SnapshotDados>(json, Opcoes);
            return dados ?? new SnapshotDados();
        }
        catch (JsonException ex)
        {
            throw new PersistenciaException("Arquivo de dados corrompido ou formato JSON inválido.", ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PersistenciaException($"Não foi possível ler '{_caminhoArquivo}'.", ex);
        }
    }

    public string CaminhoArquivo => _caminhoArquivo;
}
