namespace Olorun.Tests.Entities
{
    public interface IAlunoReadRepository
    {
        Task<Aluno> GetById(Guid Id, CancellationToken cancellationToken);
    }

    public class AlunoReadRepository : IAlunoReadRepository
    {
        public Task<Aluno> GetById(Guid Id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}