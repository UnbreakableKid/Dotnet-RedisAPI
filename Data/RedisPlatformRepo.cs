using System.Text.Json;
using RedisAPI.Models;
using StackExchange.Redis;

namespace RedisAPI.Data
{
    public class RedisPlatformRepo : IPlatformRepo
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisPlatformRepo(IConnectionMultiplexer redis)
        {
            _redis = redis;

        }
        public void CreatePlatform(Platform platform)
        {
            if (platform == null) throw new ArgumentNullException(nameof(platform));

            var db = _redis.GetDatabase();

            var serialPlat = JsonSerializer.Serialize(platform);

            //should add transaction here to make sure therte is no out of sync

            db.StringSet(platform.Id, serialPlat);
            db.SetAdd("PlatformSet", serialPlat);
        }

        public IEnumerable<Platform?>? GetAllPlatforms()
        {
            var db = _redis.GetDatabase();

            var completeSet = db.SetMembers("PlatformSet");

            if (completeSet.Length > 0)
            {
                var obj = Array.ConvertAll(completeSet, x => JsonSerializer.Deserialize<Platform>(x)).ToList();

                return obj;
            }
            return null;
        }

        public Platform? GetPlatformById(string id)
        {
            var db = _redis.GetDatabase();

            var platform = db.StringGet(id);

            if (!string.IsNullOrEmpty(platform))
            {
                return JsonSerializer.Deserialize<Platform>(platform);
            }

            return null;
        }
    }
}