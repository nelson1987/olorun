namespace Olorun.Tests.Entities
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }

    public enum Status
    {
        Cadastrado = 1,
        Matriculado = 2,
        Cancelado = 3
    }

    public class Aluno : IEntity
    {
        public Guid Id { get; set; }
        public Status Status { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
        public void Matricular() => Status = Status.Matriculado;
        public void Cancelar() => Status = Status.Cancelado;
    }
}