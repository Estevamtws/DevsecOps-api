namespace Grupo.DevSecOps.Excecoes;

/// <summary>
/// Lancada quando um recurso solicitado (ex.: produto) nao existe.
/// Tratada pelo TratadorGlobalDeErros e convertida em HTTP 404.
/// </summary>
public class RecursoNaoEncontradoException(string mensagem) : Exception(mensagem);
