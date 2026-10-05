using System.Collections.Concurrent;
using Grupo.DevSecOps.Dtos;
using Grupo.DevSecOps.Excecoes;
using Grupo.DevSecOps.Models;

namespace Grupo.DevSecOps.Services;

/// <summary>
/// Camada de servico do CRUD de produtos.
/// Os dados ficam apenas em memoria (ConcurrentDictionary), sem persistencia em banco de dados,
/// pois o foco deste projeto academico e demonstrar o pipeline DevSecOps, nao a API em si.
/// </summary>
public class ProdutoService
{
    private readonly ConcurrentDictionary<long, Produto> _produtos = new();
    private long _ultimoId;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(ILogger<ProdutoService> logger)
    {
        _logger = logger;
        SalvarNovo("Teclado mecanico", 249.90m);
        SalvarNovo("Mouse sem fio", 89.90m);
    }

    private void SalvarNovo(string nome, decimal preco)
    {
        long id = Interlocked.Increment(ref _ultimoId);
        _produtos[id] = new Produto { Id = id, Nome = nome, Preco = preco };
    }

    public IEnumerable<Produto> ListarTodos() => _produtos.Values.OrderBy(p => p.Id).ToList();

    public Produto BuscarPorId(long id)
    {
        if (!_produtos.TryGetValue(id, out Produto? produto))
        {
            throw new RecursoNaoEncontradoException($"Produto nao encontrado com id: {id}");
        }

        return produto;
    }

    public Produto Criar(ProdutoRequest request)
    {
        long id = Interlocked.Increment(ref _ultimoId);
        var produto = new Produto
        {
            Id = id,
            Nome = request.Nome!,
            Preco = request.Preco!.Value
        };
        _produtos[id] = produto;
        _logger.LogInformation("Produto criado: id={Id}", id);
        return produto;
    }

    public Produto Atualizar(long id, ProdutoRequest request)
    {
        Produto existente = BuscarPorId(id);
        existente.Nome = request.Nome!;
        existente.Preco = request.Preco!.Value;
        _logger.LogInformation("Produto atualizado: id={Id}", id);
        return existente;
    }

    public void Remover(long id)
    {
        if (!_produtos.TryRemove(id, out _))
        {
            throw new RecursoNaoEncontradoException($"Produto nao encontrado com id: {id}");
        }

        _logger.LogInformation("Produto removido: id={Id}", id);
    }
}
