# Pipeline DevSecOps Completo: do Commit a Producao

Trabalho academico que demonstra, na pratica, um pipeline de **DevSecOps**
completo em torno de uma API REST simples. A API em si e pequena de
proposito: o foco do trabalho e mostrar todas as etapas de seguranca que
um pipeline real deveria ter, do commit do codigo a publicacao da imagem
em producao.

**Integrantes:** Pedro Henrique Almeida, Juan Arruda e Jonathan Cardoso.

## Sobre a API

CRUD simples de produtos, guardado em memoria (sem banco de dados), feito
em C# com ASP.NET Core sobre o .NET 10 (LTS). Serve apenas como "alvo" para
as etapas de seguranca do pipeline (SAST, DAST, analise de dependencias, etc.).

### Endpoints

| Metodo | Caminho | Descricao |
|---|---|---|
| GET | `/api/produtos` | Lista todos os produtos cadastrados |
| GET | `/api/produtos/{id}` | Busca um produto pelo id |
| POST | `/api/produtos` | Cadastra um novo produto |
| PUT | `/api/produtos/{id}` | Atualiza um produto existente |
| DELETE | `/api/produtos/{id}` | Remove um produto existente |
| GET | `/actuator/health` | Verifica a saude da aplicacao |
| GET | `/v3/api-docs` | Especificacao OpenAPI (JSON) |
| GET | `/swagger-ui.html` | Interface do Swagger UI |

Exemplo de corpo para `POST`/`PUT`:

```json
{
  "nome": "Teclado mecanico",
  "preco": 249.90
}
```

## Estrutura do projeto

```
DevSecOps.sln                       Solucao (API + testes)
src/DevSecOpsApi/                   Codigo da API (ASP.NET Core)
  Controllers/ProdutoController.cs  Endpoints REST
  Services/ProdutoService.cs        Regras do CRUD (dados em memoria)
  Models/Produto.cs                 Modelo
  Dtos/ProdutoRequest.cs            Entrada com validacao
  Excecoes/                         Erros padronizados (404, 400, 500)
  Configuracao/                     Cabecalhos de seguranca
tests/DevSecOpsApi.Tests/           Testes de integracao (xUnit)
```

## Como rodar

### Com Docker

```bash
docker build -t devsecops-api .
docker run -p 8080:8080 devsecops-api
```

### Com o .NET SDK 10

```bash
dotnet run --project src/DevSecOpsApi
```

Depois de iniciar, a API fica disponivel em `http://localhost:8080`.

### Testes

```bash
dotnet test DevSecOps.sln
```

### Swagger UI

Com a aplicacao rodando, acesse:

```
http://localhost:8080/swagger-ui.html
```

## O pipeline DevSecOps

O pipeline esta definido em [.github/workflows/devsecops.yml](.github/workflows/devsecops.yml)
e roda em todo push/pull request para a `main`, alem de poder ser disparado
manualmente. Ele e dividido em 7 jobs, cada um responsavel por um tipo de
verificacao de seguranca:

| # | Job | Ferramenta | O que verifica | Quando bloqueia o pipeline |
|---|---|---|---|---|
| 1 | Segredos (Gitleaks) | [Gitleaks](https://github.com/gitleaks/gitleaks) | Senhas, chaves e tokens commitados no historico do Git | Qualquer segredo encontrado no repositorio |
| 2 | SAST (Semgrep) | [Semgrep](https://semgrep.dev/) | Padroes de codigo inseguro (ex.: SQL Injection) via regras publicas de C# + regra propria do projeto | Qualquer achado com severidade ERROR |
| 3 | Build e testes | .NET SDK 10 + xUnit | Se a solucao compila e se os testes automatizados passam | Falha de compilacao ou de qualquer teste |
| 4 | Dependencias e SBOM (Trivy) | [Trivy](https://aquasecurity.github.io/trivy/) | Gera o SBOM (CycloneDX) e procura vulnerabilidades nos pacotes NuGet | Vulnerabilidade CRITICAL e ja corrigida pelo fornecedor |
| 5 | IaC / Dockerfile (Checkov) | [Checkov](https://www.checkov.io/) | Boas praticas de seguranca no Dockerfile | Qualquer falha reportada pelo Checkov no Dockerfile |
| 6 | Imagem (Trivy) e DAST (OWASP ZAP) | Trivy + [OWASP ZAP](https://www.zaproxy.org/) | Vulnerabilidades na imagem Docker final e testes dinamicos contra a API em execucao | Vulnerabilidade CRITICAL na imagem, ou regra do ZAP marcada como FAIL em `.zap/rules.tsv` |
| 7 | Publicar e assinar imagem (Cosign) | [Cosign](https://github.com/sigstore/cosign) | Publica a imagem no GitHub Container Registry e assina digitalmente | So roda em push na `main`; falha se a assinatura ou verificacao falhar |

Cada job tambem esta comentado diretamente no arquivo do workflow,
explicando em linguagem simples o que ele faz.

### Arquivos de configuracao das ferramentas

- [.semgrep/regras.yml](.semgrep/regras.yml) - regra propria do Semgrep (SQL montada por string em `SqlCommand`)
- [.semgrepignore](.semgrepignore) - pastas ignoradas pelo Semgrep
- [.gitleaks.toml](.gitleaks.toml) - configuracao do Gitleaks
- [.zap/rules.tsv](.zap/rules.tsv) - regras do scan do OWASP ZAP
- [.github/dependabot.yml](.github/dependabot.yml) - atualizacoes automaticas de dependencias

### Demonstracoes de vulnerabilidades

A pasta [demonstracoes/](demonstracoes/README.md) contem exemplos de
codigo propositalmente inseguro, usados na apresentacao para mostrar o
pipeline bloqueando cada tipo de problema. Veja o README daquela pasta
para o passo a passo.

## Seguranca da aplicacao

- Cabecalhos de seguranca (`X-Content-Type-Options`, `X-Frame-Options`,
  `Referrer-Policy`, `Cache-Control`, `Content-Security-Policy`) adicionados
  em todas as respostas por [CabecalhosSegurancaMiddleware](src/DevSecOpsApi/Configuracao/CabecalhosSegurancaMiddleware.cs).
- O endpoint de saude (`/actuator/health`) mostra apenas o status, sem detalhes internos.
- Mensagens de erro nunca expoem stack trace (ver
  [TratadorGlobalDeErros](src/DevSecOpsApi/Excecoes/TratadorGlobalDeErros.cs)).
- Nenhuma senha, chave ou token no codigo-fonte ou nos arquivos de
  configuracao do projeto real.
