namespace Olorun.Tests.Entities
{
    public interface IKafkaProducer
    {
        Task Send<T>(T message);
    }

    public class KafkaProducer : IKafkaProducer
    {
        public Task Send<T>(T message)
        {
            throw new NotImplementedException();
        }
    }
}