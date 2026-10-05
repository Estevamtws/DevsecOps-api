using Grupo.DevSecOps.Dtos;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Grupo.DevSecOps.Excecoes;

/// <summary>
/// Tratador global de excecoes da API.
/// Converte erros em respostas HTTP padronizadas, sem jamais expor stack trace
/// ou detalhes internos ao cliente.
/// </summary>
public class TratadorGlobalDeErros(ILogger<TratadorGlobalDeErros> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var caminho = httpContext.Request.Path.Value ?? string.Empty;

        ErroRespostaDto resposta = exception switch
        {
            RecursoNaoEncontradoException ex => Montar(StatusCodes.Status404NotFound, ex.Message, caminho),
            _ => TratarErroGenerico(exception, caminho)
        };

        httpContext.Response.StatusCode = resposta.Status;
        await httpContext.Response.WriteAsJsonAsync(resposta, cancellationToken);
        return true;
    }

    private ErroRespostaDto TratarErroGenerico(Exception exception, string caminho)
    {
        logger.LogError(exception, "Erro inesperado em {Caminho}", caminho);
        return Montar(StatusCodes.Status500InternalServerError, "Erro interno no servidor", caminho);
    }

    private static ErroRespostaDto Montar(int status, string erro, string caminho) =>
        new() { Status = status, Erro = erro, Caminho = caminho };
}
