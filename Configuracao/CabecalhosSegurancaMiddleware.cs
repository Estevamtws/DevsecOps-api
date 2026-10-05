namespace Grupo.DevSecOps.Configuracao;

/// <summary>
/// Middleware que adiciona cabecalhos de seguranca em todas as respostas.
/// Esses cabecalhos ajudam a mitigar clickjacking, sniffing de tipo de conteudo
/// e vazamento de informacao via Referrer.
///
/// A Content-Security-Policy e restritiva por padrao. Nas rotas do Swagger UI ela e
/// relaxada apenas no minimo necessario para a interface carregar (scripts e estilos
/// inline usados pela propria pagina do Swagger UI).
/// </summary>
public class CabecalhosSegurancaMiddleware(RequestDelegate proximo)
{
    private const string CspRestritiva = "default-src 'self'; frame-ancestors 'none'; object-src 'none'";

    private const string CspSwagger =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "frame-ancestors 'none'; " +
        "object-src 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        var cabecalhos = context.Response.Headers;
        cabecalhos["X-Content-Type-Options"] = "nosniff";
        cabecalhos["X-Frame-Options"] = "DENY";
        cabecalhos["Referrer-Policy"] = "no-referrer";
        cabecalhos["Cache-Control"] = "no-store";
        cabecalhos["Content-Security-Policy"] = EhRotaDeDocumentacao(context.Request.Path)
            ? CspSwagger
            : CspRestritiva;

        return proximo(context);
    }

    private static bool EhRotaDeDocumentacao(PathString caminho) =>
        caminho.StartsWithSegments("/swagger-ui") || caminho.StartsWithSegments("/v3/api-docs");
}
