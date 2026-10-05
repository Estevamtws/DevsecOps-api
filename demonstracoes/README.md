# Demonstracoes de vulnerabilidades

Esta pasta existe **apenas para a apresentacao do trabalho**. Ela contem
codigo propositalmente inseguro, usado para mostrar, ao vivo, que cada
etapa do pipeline realmente bloqueia o tipo de problema para o qual foi
criada.

Enquanto os arquivos estiverem dentro de `demonstracoes/`, eles **nao**
quebram o pipeline: as ferramentas (Gitleaks, Semgrep, Checkov, Trivy)
estao configuradas para ignorar esta pasta. O problema so aparece quando o
arquivo e **copiado para o lugar real** do projeto, como aconteceria em um
erro real de um desenvolvedor.

## Tabela: qual job barra cada demonstracao

| Demonstracao | Arquivo de origem | Job que barra | Ferramenta |
|---|---|---|---|
| Segredo no codigo | `1-segredo/ConfiguracaoPagamento.cs` | 1. Segredos | Gitleaks |
| SQL Injection | `2-sql-injection/ProdutoRepositorioSql.cs` | 2. SAST | Semgrep (regra propria) |
| Dependencia vulneravel (log4net, CVE-2018-1285) | `3-dependencia-vulneravel/trecho-para-o-csproj.txt` | 4. Dependencias e SBOM | Trivy |
| Dockerfile inseguro | `4-dockerfile-inseguro/Dockerfile-inseguro.txt` | 5. IaC / Dockerfile | Checkov |

A versao corrigida da SQL Injection, `2-sql-injection/ProdutoRepositorioSqlCorrigido.cs`,
usa `SqlParameter` e passa no Semgrep. Use-a para mostrar a correcao.

## Como demonstrar cada caso

Cada demonstracao e uma branch separada, sempre criada a partir da `main`,
com um pull request que o pipeline barra. **Nao faca merge de nenhuma.**
Rode os comandos na raiz do projeto, no Git Bash ou no terminal do VS Code.

Depois de cada `git push`, abra o repositorio no GitHub, clique no botao
**Compare & pull request**, confira que o destino e `main` e clique em
**Create pull request**. Em poucos minutos o job correspondente fica vermelho.

### 1. Segredo no codigo (Gitleaks)

```bash
git checkout main
git pull
git checkout -b demo/segredo-no-codigo
cp demonstracoes/1-segredo/ConfiguracaoPagamento.cs src/DevSecOpsApi/Configuracao/ConfiguracaoPagamento.cs
git add .
git commit -m "demo: adiciona chave de API no codigo (nao fazer isso!)"
git push -u origin demo/segredo-no-codigo
```

O job **"1. Segredos (Gitleaks)"** deve falhar, apontando a chave
encontrada em `ConfiguracaoPagamento.cs`.

### 2. SQL Injection (Semgrep)

```bash
git checkout main
git pull
git checkout -b demo/sql-injection
mkdir -p src/DevSecOpsApi/Repositorios
cp demonstracoes/2-sql-injection/ProdutoRepositorioSql.cs src/DevSecOpsApi/Repositorios/ProdutoRepositorioSql.cs
git add .
git commit -m "demo: adiciona consulta SQL concatenada (vulneravel)"
git push -u origin demo/sql-injection
```

O job **"2. SAST (Semgrep)"** deve falhar, apontando a regra
`sql-concatenada-em-sqlcommand` definida em `.semgrep/regras.yml`.

O job "3. Build e testes" tambem fica vermelho nesta demonstracao, porque o
arquivo usa o pacote `Microsoft.Data.SqlClient`, que o projeto nao tem. Na
apresentacao, mostre o job 2: e ele que aponta a falha de seguranca.

### 3. Dependencia vulneravel - log4net (Trivy)

```bash
git checkout main
git pull
git checkout -b demo/dependencia-vulneravel
```

Abra `src/DevSecOpsApi/DevSecOpsApi.csproj` e cole esta linha dentro do
`<ItemGroup>` que ja tem os outros `PackageReference`:

```xml
<PackageReference Include="log4net" Version="2.0.8" />
```

Depois:

```bash
git add .
git commit -m "demo: adiciona log4net 2.0.8 (CVE-2018-1285)"
git push -u origin demo/dependencia-vulneravel
```

O job **"4. Dependencias e SBOM (Trivy)"** deve falhar, apontando a
vulnerabilidade CRITICAL do log4net 2.0.8.

### 4. Dockerfile inseguro (Checkov)

```bash
git checkout main
git pull
git checkout -b demo/dockerfile-inseguro
cp demonstracoes/4-dockerfile-inseguro/Dockerfile-inseguro.txt Dockerfile
git add .
git commit -m "demo: usa Dockerfile sem USER e sem HEALTHCHECK"
git push -u origin demo/dockerfile-inseguro
```

O job **"5. IaC / Dockerfile (Checkov)"** deve falhar, apontando a
ausencia das instrucoes `USER` e `HEALTHCHECK`.

### Voltando para a main

Depois de criar as demonstracoes, volte para a branch principal:

```bash
git checkout main
```
