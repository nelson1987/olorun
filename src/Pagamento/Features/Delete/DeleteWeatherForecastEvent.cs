using SharedDomain.Shared;

namespace Pagamento.Features.Delete;
public record DeleteWeatherForecastEvent : IEvent
{
    public Guid Id { get; init; }
}