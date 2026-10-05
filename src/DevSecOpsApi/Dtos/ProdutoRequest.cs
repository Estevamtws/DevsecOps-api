using System.ComponentModel.DataAnnotations;

namespace Grupo.DevSecOps.Dtos;

/// <summary>
/// DTO de entrada para criacao e atualizacao de produtos.
/// As validacoes aqui evitam que dados invalidos cheguem na camada de servico.
/// </summary>
public class ProdutoRequest : IValidatableObject
{
    [Required(ErrorMessage = "O nome do produto e obrigatorio")]
    [StringLength(100, ErrorMessage = "O nome do produto deve ter no maximo 100 caracteres")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "O preco do produto e obrigatorio")]
    public decimal? Preco { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Preco is not null && Preco <= 0)
        {
            yield return new ValidationResult(
                "O preco deve ser maior que zero",
                [nameof(Preco)]);
        }
    }
}
