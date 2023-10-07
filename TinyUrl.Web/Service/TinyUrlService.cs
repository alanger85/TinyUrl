using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using TinyUrl.Dal.Mongo;
using TinyUrl.Dal.Mongo.Entities;
using TinyUrl.Web.Cache;

namespace TinyUrl.Web.Service
{
    public class TinyUrlService
    {
        private TinyUrlMongoDal _tinyUrlMongoDal;
        private CacheManager _cacheManager;
        private ConcurrentDictionary<string, object> _lockByShortName = new ConcurrentDictionary<string, object>();
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        private System.Timers.Timer _timer;
        public TinyUrlService(TinyUrlMongoDal tinyUrlMongoDal, CacheManager cacheManager)
        {
            _tinyUrlMongoDal = tinyUrlMongoDal;
            _cacheManager = cacheManager;
            _timer = new System.Timers.Timer();
            _timer.Interval = 300_000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // clean locks
            _readerWriterLockSlim.EnterWriteLock();
            _lockByShortName = new ConcurrentDictionary<string, object>();
            _readerWriterLockSlim.ExitWriteLock();
        }

        public string GetRedirectUrl(string shortUrl)
        {
            var redirectUrl = (MongoUrlRedirect)_cacheManager.Get(shortUrl);
            if (redirectUrl != null)
            {
                Debug.WriteLine("from cache " + shortUrl);
                return redirectUrl.LongUrl;
            }

            _readerWriterLockSlim.EnterReadLock();
            _lockByShortName.TryAdd(shortUrl, new object());
            _lockByShortName.TryGetValue(shortUrl, out var shortUrlLock);
            _readerWriterLockSlim.ExitReadLock();

            lock (shortUrlLock)
            {
                redirectUrl = (MongoUrlRedirect)_cacheManager.Get(shortUrl);
                if (redirectUrl == null)
                {
                    redirectUrl = _tinyUrlMongoDal.GetRedirectPair(shortUrl);

                    if (redirectUrl == null)//not found 
                        return null;


                    Debug.WriteLine("from db " + shortUrl);
                    _cacheManager.Set(shortUrl, redirectUrl, new TimeSpan(0, 5, 0));
                    return redirectUrl.LongUrl;
                }
            }

            Debug.WriteLine("from cache " + shortUrl);
            return redirectUrl?.LongUrl;
        }
}
}
