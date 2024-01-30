using AutoMapper;

namespace Olorun.Tests.Entities
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<InclusaoAlunoCommand, AlunoIncluidoEvent>();
        }
    }
}