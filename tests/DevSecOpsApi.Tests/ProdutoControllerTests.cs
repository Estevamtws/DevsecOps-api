using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Grupo.DevSecOps.Tests;

/// <summary>
/// Testes de integracao do CRUD de produtos, dos cabecalhos de seguranca
/// e da disponibilidade da documentacao OpenAPI.
///
/// Observacao: as asercoes sobre conteudo JSON usam apenas texto ASCII,
/// por isso palavras como "mecanico" aparecem sem acento.
/// </summary>
public class ProdutoControllerTests(WebApplicationFactory<Program> fabrica) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    [Fact]
    public async Task DeveListarProdutosCadastrados()
    {
        var resposta = await _cliente.GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, corpo.ValueKind);
        Assert.NotEmpty(corpo.EnumerateArray());
    }

    [Fact]
    public async Task DeveCadastrarProdutoValido()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", new { nome = "Monitor 24 polegadas", preco = 899.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("id", out _));
        Assert.Equal("Monitor 24 polegadas", corpo.GetProperty("nome").GetString());
        Assert.Equal(899.00m, corpo.GetProperty("preco").GetDecimal());
    }

    [Fact]
    public async Task DeveRejeitarProdutoInvalido()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", new { nome = "", preco = 10.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, corpo.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task DeveRetornar404AoBuscarProdutoInexistente()
    {
        var resposta = await _cliente.GetAsync("/api/produtos/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, corpo.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task DeveAtualizarProdutoExistente()
    {
        long id = await CadastrarAsync("Cadeira gamer", 1200.00m);

        var resposta = await _cliente.PutAsJsonAsync($"/api/produtos/{id}", new { nome = "Cadeira gamer XL", preco = 1500.00m });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Cadeira gamer XL", corpo.GetProperty("nome").GetString());
        Assert.Equal(1500.00m, corpo.GetProperty("preco").GetDecimal());
    }

    [Fact]
    public async Task DeveRemoverProdutoExistente()
    {
        long id = await CadastrarAsync("Fone de ouvido", 150.00m);

        var remocao = await _cliente.DeleteAsync($"/api/produtos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        var consulta = await _cliente.GetAsync($"/api/produtos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task DeveIncluirCabecalhosDeSeguranca()
    {
        var resposta = await _cliente.GetAsync("/api/produtos");

        Assert.Equal("nosniff", resposta.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", resposta.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", resposta.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("no-store", resposta.Headers.CacheControl?.ToString());
        Assert.True(resposta.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task DeveExporDocumentacaoOpenApi()
    {
        var resposta = await _cliente.GetAsync("/v3/api-docs");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveAbrirSwaggerUi()
    {
        var resposta = await _cliente.GetAsync("/swagger-ui/index.html");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    private async Task<long> CadastrarAsync(string nome, decimal preco)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", new { nome, preco });
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return corpo.GetProperty("id").GetInt64();
    }
}
