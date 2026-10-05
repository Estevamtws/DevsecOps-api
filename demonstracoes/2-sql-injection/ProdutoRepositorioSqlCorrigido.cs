// Versao CORRIGIDA da demonstracao de SQL Injection.
//
// Aqui o parametro "nome" nunca e concatenado na string SQL: ele e enviado
// separadamente ao banco atraves de um SqlParameter, usando o marcador @nome.
// Isso impede que o conteudo informado pelo usuario seja interpretado como
// parte do comando SQL.

using Microsoft.Data.SqlClient;

namespace Grupo.DevSecOps.Repositorios;

public class ProdutoRepositorioSqlCorrigido(SqlConnection conexao)
{
    public SqlDataReader BuscarPorNome(string nome)
    {
        var comando = new SqlCommand("SELECT * FROM produtos WHERE nome = @nome", conexao);
        comando.Parameters.Add(new SqlParameter("@nome", nome));
        return comando.ExecuteReader();
    }
}
