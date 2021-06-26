using System;
using System.Configuration;
using System.Runtime.Caching;
using System.Threading;
using ThirdParty;

namespace ImprovedAdvertisementService
{
    /// <inheritdoc />
    public class AdvertisementService : IAdvertisementService
    {
        #region Class Constants

        /// <summary>
        /// Threshold for number of HTTP errors wihin last hour, below which to try HTTP Provider
        /// </summary>
        private const int AcceptableHttpErrorsWithin1Hour = 10;

        /// <summary>
        /// Time in milliseconds to wait before retrying HTTP provider 
        /// </summary>
        private const int RetryWaitTime = 1000;

        #endregion

        #region Private Member Variables

        /// <summary>
        /// Advertisement cache, shared across all instances of this class
        /// </summary>
        private readonly static MemoryCache _advertCache = new MemoryCache("AdvertCache");

        /// <summary>
        /// HTTP error queue
        /// </summary>
        private readonly HttpErrorQueue _httpErrorQueue = new HttpErrorQueue();

        /// <summary>
        /// Locking object used to protect multi-threaded access to _advertCache, shared across all instances of this class
        /// </summary>
        private readonly static object _lockObj = new object();

        /// <summary>
        /// Retry count for HTTP requests, from App Config, shared across all instances of this class 
        /// </summary>
        private readonly static int _retryCount = int.Parse(ConfigurationManager.AppSettings["RetryCount"]);

        #endregion

        #region Public Methods

        /// <inheritdoc />
        /// <remarks>
        /// Detailed implementation-specific logic:
        /// 
        /// 1. Search for advertId in cache, return data if found, otherwise go to Step 2
        ///
        /// 2. If acceptable number of HTTP errors within last hour, try HTTP provider (Main Provider). 
        ///    In case of error, retry AppSettings["RetryCount"] times.
        ///    If success, return data, otherwise go to Step 3
        ///
        /// 3. Try Backup Provider
        /// </remarks>
        public Advertisement GetAdvertisement(string advertId)
        {
            lock (_lockObj)
            {
                var _advert = (Advertisement)_advertCache.Get($"AdvKey_{advertId}");
                if (_advert != null) return _advert;

                // Clear excess error queue items older than 1 hour
                _httpErrorQueue.ClearExcessItems(DateTime.Now.AddHours(-1));

                // If HTTP errors timestamped within last hour at acceptable level, try HTTP provider
                if (_httpErrorQueue.CountWithinThreshold(DateTime.Now.AddHours(-1)) < AcceptableHttpErrorsWithin1Hour)
                {
                    int attempts = 0;
                    var dataProvider = new NoSqlAdvProvider();
                    do
                    {
                        try { _advert = dataProvider.GetAdv(advertId); }
                        catch
                        {
                            Thread.Sleep(RetryWaitTime);
                            _httpErrorQueue.Enqueue(DateTime.Now);
                        }
                    } while ((_advert == null) && (++attempts <= _retryCount));  // Note: x retries means maximum x+1 attempts

                    if (_advert != null)
                    {
                        // Success - place advert in cache and return it
                        _advertCache.Set($"AdvKey_{advertId}", _advert, DateTimeOffset.Now.AddMinutes(5));
                        return _advert;
                    }
                }

                // Try Backup provider
                _advert = SQLAdvProvider.GetAdv(advertId);
                if (_advert != null)
                {
                    // Success - place advert in cache and return it
                    _advertCache.Set($"AdvKey_{advertId}", _advert, DateTimeOffset.Now.AddMinutes(5));
                    return _advert;
                }
            }

            // All attempts failed
            return null;
        }

        #endregion
    }
}
