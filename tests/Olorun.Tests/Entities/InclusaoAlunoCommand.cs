namespace Olorun.Tests.Entities
{
    public interface ICommand { }
    public record InclusaoAlunoCommand : ICommand { }
    public class InclusaoAlunoCommandValidator : AbstractValidator<InclusaoAlunoCommand> { }
}