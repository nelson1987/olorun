using FluentValidation;

namespace SharedDomain.Features;
public interface IEvent
{
}
public interface IEntity
{
}
public interface IHandler { }
public class KafkaProducer<T> where T : IEvent
{
}
public class KafkaConsume<T> where T : IEvent
{
}
public record CreateTransferenciaEvent : IEvent
{
}
public class CreateTransferenciaEventValidator : AbstractValidator<CreateTransferenciaEvent>
{
}
public class Transferencia : IEntity
{
}
public record CreateTransferenciaCommand
{
}
public class CreateTransferenciaCommandValidator : AbstractValidator<CreateTransferenciaCommand>
{
}
public class CreateTransferenciaHandler : IHandler
{

}