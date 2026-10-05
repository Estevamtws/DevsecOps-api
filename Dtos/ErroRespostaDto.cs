namespace Grupo.DevSecOps.Dtos;

/// <summary>
/// Formato padronizado de resposta de erro da API.
/// Nao expoe stack trace nem detalhes internos, apenas informacoes seguras ao cliente.
/// </summary>
public class ErroRespostaDto
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public int Status { get; init; }

    public string Erro { get; init; } = string.Empty;

    public string Caminho { get; init; } = string.Empty;

    public IReadOnlyList<string>? Detalhes { get; init; }
}
