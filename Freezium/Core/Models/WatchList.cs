using LiteDB;

namespace Freezium.Core.Models
{
    public class WatchList
    {
        [BsonId]
        public string id { get; set; }

        public AnimeUser Data { get; set; } = new AnimeUser();
    }
}
