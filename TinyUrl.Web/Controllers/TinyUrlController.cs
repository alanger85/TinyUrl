using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TinyUrl.Dal.Mongo;
using TinyUrl.Dal.Mongo.Entities;
using TinyUrl.Web.Logic;

namespace TinyUrl.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TinyUrlController : ControllerBase
    {
        private readonly string _host;
        private TinyUrlMongoDal _tinyUrlMongoDal;
        public TinyUrlController(TinyUrlMongoDal tinyUrlMongoDal, IServer server)
        {
            _host = server.Features.Get<IServerAddressesFeature>().Addresses.First();
            _tinyUrlMongoDal = tinyUrlMongoDal;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateTinyUrl(string url)
        {
            url = url.Trim();
            Regex regex = new Regex("^((https:\\/\\/)|(http:\\/\\/)|)?([a-zA-Z0-9]+\\.[a-zA-Z0-9]+)+\\??.*$");
            if (!regex.IsMatch(url))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "invalid url");
            }

            var redirectPair = new MongoUrlRedirect()
            {
                LongUrl = url,
            };

            var existingRedirectPairByLongUrl = await _tinyUrlMongoDal.GetRedirectPairByLongUrlAsync(url);
            if (existingRedirectPairByLongUrl != null)
            {
                redirectPair.ShortUrl = existingRedirectPairByLongUrl.ShortUrl;
            }
            else
            {
                redirectPair.ShortUrl = Util.GenerateShortUrl();

                // check for existing short url
                var existingRedirectPairByShortUrl = await _tinyUrlMongoDal.GetRedirectPairAsync(redirectPair.ShortUrl);
                if (existingRedirectPairByShortUrl != null && existingRedirectPairByShortUrl.LongUrl != redirectPair.LongUrl)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Collision Detected, try again");
                }

                // prevent duplicate long url with different short url
                try
                {
                    await _tinyUrlMongoDal.CreateLongUrlAsync(url);
                }
                catch (MongoWriteException ex)
                {
                    await Task.Delay(1_000);

                    existingRedirectPairByLongUrl = await _tinyUrlMongoDal.GetRedirectPairByLongUrlAsync(url);
                    if (existingRedirectPairByLongUrl != null)
                    {
                        redirectPair.ShortUrl = existingRedirectPairByLongUrl.ShortUrl;
                        return Ok(string.Concat(_host, "/", redirectPair.ShortUrl));
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                }

                // save pair
                await _tinyUrlMongoDal.CreateRedirectPairAsync(redirectPair);
            }

            return Ok(string.Concat(_host, "/", redirectPair.ShortUrl));
        }
    }
}