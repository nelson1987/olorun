using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace SharedDomain.Features.Pagamentos;
public interface IPagamentoReadRepository
{
    Task<List<Pagamento>> GetAsync(CancellationToken cancellationToken = default);

    Task<Pagamento?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Pagamento newBook, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, Pagamento updatedBook, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
public class PagamentoReadRepository : IPagamentoReadRepository
{
    private readonly IMongoCollection<Pagamento> _booksCollection;

    public PagamentoReadRepository(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<Pagamento>(
            bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<Pagamento>> GetAsync(CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<Pagamento?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    [Obsolete]
    public async Task CreateAsync(Pagamento newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

    public async Task UpdateAsync(Guid id, Pagamento updatedBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);

    public void CreateAsync(Pagamento pagamento)
    {
        throw new NotImplementedException();
    }
}
