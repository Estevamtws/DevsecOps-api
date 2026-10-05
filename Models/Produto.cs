namespace Grupo.DevSecOps.Models;

/// <summary>
/// Representa um produto armazenado em memoria.
/// </summary>
public class Produto
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }
}
