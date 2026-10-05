// DEMONSTRACAO DE VULNERABILIDADE - NAO USAR EM CODIGO REAL.
//
// Esta classe contem uma chave de API ficticia (gerada aleatoriamente, sem
// relacao com nenhum provedor real) escrita diretamente no codigo-fonte.
// O objetivo e mostrar, na apresentacao, como o Gitleaks detecta segredos
// commitados (regra generica "generic-api-key") quando este arquivo e
// copiado para dentro da pasta src/ do projeto real.

namespace Grupo.DevSecOps.Configuracao;

public static class ConfiguracaoPagamento
{
    // Chave de API falsa, apenas para demonstracao do Gitleaks.
    public const string ChaveApiPagamento = "pgto_k9f3m2x7q1w8e4r6t0y5u2i9o3a7s1d8f6g4h2j0";

    public static string ObterChaveConfigurada() => ChaveApiPagamento;
}
