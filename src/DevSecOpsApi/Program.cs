using Grupo.DevSecOps.Configuracao;
using Grupo.DevSecOps.Dtos;
using Grupo.DevSecOps.Excecoes;
using Grupo.DevSecOps.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ProdutoService>();
builder.Services.AddExceptionHandler<TratadorGlobalDeErros>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Respostas de validacao (400) seguem o mesmo formato de ErroRespostaDto
builder.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var detalhes = contexto.ModelState.Values
            .SelectMany(campo => campo.Errors)
            .Select(erro => erro.ErrorMessage)
            .ToList();

        var resposta = new ErroRespostaDto
        {
            Status = StatusCodes.Status400BadRequest,
            Erro = "Dados invalidos",
            Caminho = contexto.HttpContext.Request.Path.Value ?? string.Empty,
            Detalhes = detalhes
        };

        return new BadRequestObjectResult(resposta);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.EnableAnnotations();

    // O nome do documento "api-docs" faz a especificacao ficar exatamente em /v3/api-docs
    opcoes.SwaggerDoc("api-docs", new()
    {
        Title = "API de Produtos - Pipeline DevSecOps",
        Version = "1.0.0",
        Description = "API REST de demonstracao usada no trabalho academico "
            + "\"Pipeline DevSecOps Completo: do Commit a Producao\". "
            + "Expoe um CRUD simples de produtos para servir de alvo das etapas de seguranca do pipeline.",
        Contact = new() { Name = "Pedro Henrique Almeida, Juan Arruda e Jonathan Cardoso" }
    });
});

// Kestrel escuta na porta 8080 tanto localmente quanto dentro do container
builder.WebHost.ConfigureKestrel(opcoes => opcoes.ListenAnyIP(8080));

var app = builder.Build();

app.UseMiddleware<CabecalhosSegurancaMiddleware>();

app.MapGet("/swagger-ui.html", () => Results.Redirect("/swagger-ui/index.html"))
    .ExcludeFromDescription();

app.UseSwagger(opcoes => opcoes.RouteTemplate = "v3/{documentName}");
app.UseSwaggerUI(opcoes =>
{
    opcoes.RoutePrefix = "swagger-ui";
    opcoes.SwaggerEndpoint("/v3/api-docs", "API de Produtos v1.0.0");
});

app.UseExceptionHandler();
app.MapControllers();

// Expoe apenas o status de saude, sem detalhes internos (resposta padrao do ASP.NET Core)
app.MapHealthChecks("/actuator/health");

app.Run();

// Torna a classe Program visivel para os testes de integracao
public partial class Program { }
