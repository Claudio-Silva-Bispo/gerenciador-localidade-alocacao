using Project.Models;

namespace Project.Infrastructure.Interfaces
{
    public interface IEnderecoRepository
    {
        Task<Endereco> Criar(Endereco endereco);
        Task<Endereco> ConsultarId(string id);
        Task<List<Endereco>> ConsultarTodos();
        Task<Endereco?> Atualizar(Endereco endereco);
        Task Excluir(string id);

    }
}
