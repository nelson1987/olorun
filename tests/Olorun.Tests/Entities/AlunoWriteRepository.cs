namespace Olorun.Tests.Entities
{
    public interface IAlunoWriteRepository
    {
        Task Incluir(InclusaoAlunoCommand command, CancellationToken cancellationToken);
    }

    public class AlunoWriteRepository : IAlunoWriteRepository
    {
        public Task Incluir(InclusaoAlunoCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}