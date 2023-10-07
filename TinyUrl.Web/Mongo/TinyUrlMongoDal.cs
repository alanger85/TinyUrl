using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TinyUrl.Dal.Mongo.Entities;
using TinyUrl.Web.Mongo;

namespace TinyUrl.Dal.Mongo
{
    public class TinyUrlMongoDal
    {
        private readonly IMongoCollection<MongoUrlRedirect> _urlRedirectCollection;
        private readonly IMongoCollection<MongoLongUrl> _longUrlCollection;
        public TinyUrlMongoDal(IOptions<TinyUrlDatabaseSettings> tinyUrlDatabaseSettings)
        {
            var mongoClient = new MongoClient(
           tinyUrlDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                tinyUrlDatabaseSettings.Value.DatabaseName);

            _urlRedirectCollection = mongoDatabase.GetCollection<MongoUrlRedirect>(
                "RedirectPairs");

            _longUrlCollection = mongoDatabase.GetCollection<MongoLongUrl>(
              "LongUrls");
        }

        public async Task<MongoUrlRedirect> GetRedirectPairAsync(string id)
        {
            return await _urlRedirectCollection.Find(x => x.ShortUrl == id).FirstOrDefaultAsync();
        }
        public MongoUrlRedirect GetRedirectPair(string id)
        {
            return _urlRedirectCollection.Find(x => x.ShortUrl == id).FirstOrDefault();
        }

        public async Task<MongoUrlRedirect> GetRedirectPairByLongUrlAsync(string longUrl)
        {
            return await _urlRedirectCollection.Find(x => x.LongUrl == longUrl).FirstOrDefaultAsync();
        }

        public async Task CreateRedirectPairAsync(MongoUrlRedirect urlRedirect)
        {
            await _urlRedirectCollection.InsertOneAsync(urlRedirect);
        }

        public async Task CreateLongUrlAsync(string longUrl)
        {
            await _longUrlCollection.InsertOneAsync(new MongoLongUrl() { LongUrl = longUrl });
        }

    }
}