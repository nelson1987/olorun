
public interface IWeatherForecastRepository
{
    Task<List<WeatherForecast>> GetAsync(CancellationToken cancellationToken = default);
    Task<WeatherForecast?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(WeatherForecast newBook, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, WeatherForecast updatedBook);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}

public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly IMongoCollection<WeatherForecast> _booksCollection;

    public WeatherForecastRepository(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<WeatherForecast>(
            bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<WeatherForecast>> GetAsync(CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<WeatherForecast?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task CreateAsync(WeatherForecast newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

    public async Task UpdateAsync(Guid id, WeatherForecast updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);

}
