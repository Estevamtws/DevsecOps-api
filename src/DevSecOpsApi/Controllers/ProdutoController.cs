using Grupo.DevSecOps.Dtos;
using Grupo.DevSecOps.Models;
using Grupo.DevSecOps.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Grupo.DevSecOps.Controllers;

/// <summary>
/// Endpoints REST para o CRUD de produtos.
/// </summary>
[ApiController]
[Route("api/produtos")]
[SwaggerTag("Operacoes de cadastro, consulta, atualizacao e remocao de produtos")]
public class ProdutoController(ProdutoService produtoService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Listar produtos", Description = "Retorna todos os produtos cadastrados em memoria")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de produtos retornada com sucesso", typeof(IEnumerable<Produto>))]
    public ActionResult<IEnumerable<Produto>> Listar() => Ok(produtoService.ListarTodos());

    [HttpGet("{id:long}")]
    [SwaggerOperation(Summary = "Buscar produto por id", Description = "Retorna um produto especifico pelo seu identificador")]
    [SwaggerResponse(StatusCodes.Status200OK, "Produto encontrado", typeof(Produto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Produto nao encontrado", typeof(ErroRespostaDto))]
    public ActionResult<Produto> BuscarPorId([FromRoute] long id) => Ok(produtoService.BuscarPorId(id));

    [HttpPost]
    [SwaggerOperation(Summary = "Cadastrar produto", Description = "Cria um novo produto a partir dos dados informados")]
    [SwaggerResponse(StatusCodes.Status201Created, "Produto criado com sucesso", typeof(Produto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados invalidos", typeof(ErroRespostaDto))]
    public ActionResult<Produto> Criar([FromBody] ProdutoRequest request)
    {
        Produto produto = produtoService.Criar(request);
        return CreatedAtAction(nameof(BuscarPorId), new { id = produto.Id }, produto);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(Summary = "Atualizar produto", Description = "Atualiza nome e preco de um produto existente")]
    [SwaggerResponse(StatusCodes.Status200OK, "Produto atualizado com sucesso", typeof(Produto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados invalidos", typeof(ErroRespostaDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Produto nao encontrado", typeof(ErroRespostaDto))]
    public ActionResult<Produto> Atualizar([FromRoute] long id, [FromBody] ProdutoRequest request) =>
        Ok(produtoService.Atualizar(id, request));

    [HttpDelete("{id:long}")]
    [SwaggerOperation(Summary = "Remover produto", Description = "Remove um produto existente pelo seu identificador")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Produto removido com sucesso")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Produto nao encontrado", typeof(ErroRespostaDto))]
    public IActionResult Remover([FromRoute] long id)
    {
        produtoService.Remover(id);
        return NoContent();
    }
}
