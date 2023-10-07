using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace TinyUrl.Web.Cache
{
    public class CacheManager
    {
        private ConcurrentDictionary<string, CacheRecord> _cacheObjectslMap = new ConcurrentDictionary<string, CacheRecord>();

        private System.Timers.Timer _evictionTimer;
        private const int MAX_CACHE_SIZE = 1_000; 

        public CacheManager()
        {
            _evictionTimer = new System.Timers.Timer();
            _evictionTimer.Interval = 5_000;
            _evictionTimer.Elapsed += _evictionTimerElapsed;
            _evictionTimer.Start();
        }

        private void _evictionTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var cacheObjectsPair = _cacheObjectslMap.ToList();

            var cacheKeysToRemove = new List<string>();
            Debug.WriteLine($"_evictionTimerElapsed Cache Record Count {cacheObjectsPair.Count} {DateTime.UtcNow}");
            foreach (var cacheObjectPair in cacheObjectsPair) 
            {
                if(cacheObjectPair.Value.Expiration < DateTime.UtcNow)
                    cacheKeysToRemove.Add(cacheObjectPair.Key);
            }

            foreach (var key in cacheKeysToRemove)
            {
                _cacheObjectslMap.Remove(key, out var removedCacheRecord);
                Debug.WriteLine($"Remove Cache Record {key}");

            }
        }
        private class CacheRecord
        {
            public object Value { get; set; }

            public TimeSpan SlidingExpiration;

            private DateTime _expiration;
            public DateTime Expiration => _expiration;


            public CacheRecord(object value, TimeSpan slidingExpiration)
            {
                Value = value;
                SlidingExpiration = slidingExpiration;
                ExtendExpiration();
            }

            public void ExtendExpiration()
            {
                _expiration = DateTime.UtcNow.Add(SlidingExpiration);
                Debug.WriteLine($"new sliding expiration, now = {DateTime.UtcNow}; expiration = {_expiration} ");
            }
        }

        public void Set(string name, object value, TimeSpan slidingExpiration)
        {
            if (_cacheObjectslMap.Count < MAX_CACHE_SIZE) 
            {
                var cacheRecord = new CacheRecord(value, slidingExpiration);

                _cacheObjectslMap.AddOrUpdate(name, cacheRecord, (key, exiatingRecord) => { return cacheRecord; });
            }
        }

        public object Get(string key) 
        {
            if (_cacheObjectslMap.TryGetValue(key, out var cacheRecord)) 
            {
                cacheRecord.ExtendExpiration();
            }

            return cacheRecord?.Value;
        }
    }
}
