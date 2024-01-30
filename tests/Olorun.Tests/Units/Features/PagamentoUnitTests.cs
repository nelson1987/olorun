using FluentAssertions;
using Olorun.Tests.Entities;

namespace Olorun.Tests.Units.Features
{
    public class AlunoUnitTests
    {
        [Fact]
        public void Test1()
        {

        }
    }
    public class AlunoFactoryUnitTests
    {
        [Fact]
        public void CriarComSucesso()
        {
            var aluno = AlunoFactory.Create("nome", "documento");
            aluno.Status.Should().Be(Status.Cadastrado);
        }

        [Fact]
        public void CriarSemNomeComErro()
        {
            var aluno = AlunoFactory.Create("", "documento");
            aluno.Status.Should().Be(Status.Cadastrado);
        }

        [Fact]
        public void CriarSemDocumentoComErro()
        {
            var aluno = AlunoFactory.Create("nome", "");
            aluno.Status.Should().Be(Status.Cadastrado);
        }
    }
    public class AlunoWriteRepositoryUnitTests { [Fact] public void Test1() { } }
    public class AlunoReadRepositoryUnitTests { [Fact] public void Test1() { } }
    public class InclusaoAlunoCommandUnitTests { [Fact] public void Test1() { } }
    public class InclusaoAlunoCommandvalidatorUnitTests { [Fact] public void Test1() { } }
    public class InclusaoAlunoHandlerUnitTests { [Fact] public void Test1() { } }
    public class AlunoIncluidoEventUnitTests { [Fact] public void Test1() { } }
    public class KafkaProducerUnitTests { [Fact] public void Test1() { } }
    public class KafkaConsumerUnitTests { [Fact] public void Test1() { } }
}