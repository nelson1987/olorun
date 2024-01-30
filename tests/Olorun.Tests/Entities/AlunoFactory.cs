namespace Olorun.Tests.Entities
{
    public static class AlunoFactory
    {
        public static Aluno Create(string nome, string documento)
        {
            if (string.IsNullOrEmpty(nome)) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(documento)) throw new ArgumentNullException();
            return new Aluno()
            {
                Id = Guid.NewGuid(),
                Status = Status.Cadastrado,
                Nome = nome,
                Documento = documento
            };
        }
    }
}