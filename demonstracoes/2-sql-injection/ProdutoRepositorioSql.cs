// DEMONSTRACAO DE VULNERABILIDADE - NAO USAR EM CODIGO REAL.
//
// Monta a consulta SQL concatenando o parametro recebido diretamente na
// string e executa usando SqlCommand. Um usuario mal-intencionado poderia
// informar um "nome" como: ' OR '1'='1
// e teria acesso a todos os produtos da tabela, ou pior, poderia manipular
// a consulta para apagar dados.
//
// Esta e a versao INSEGURA, usada para a regra propria do Semgrep
// (.semgrep/regras.yml) detectar o problema quando o arquivo e copiado
// para dentro da pasta src/ do projeto real.

using Microsoft.Data.SqlClient;

namespace Grupo.DevSecOps.Repositorios;

public class ProdutoRepositorioSql(SqlConnection conexao)
{
    public SqlDataReader BuscarPorNome(string nome)
    {
        string sql = "SELECT * FROM produtos WHERE nome = '" + nome + "'";
        var comando = new SqlCommand(sql, conexao);
        return comando.ExecuteReader();
    }
}
