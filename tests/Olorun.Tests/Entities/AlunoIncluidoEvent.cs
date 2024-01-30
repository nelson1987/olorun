namespace Olorun.Tests.Entities
{
    public interface IEvent { Guid Id { get; set; } }
    public record AlunoIncluidoEvent : IEvent
    {
        public Guid Id { get; set; }
    }
}