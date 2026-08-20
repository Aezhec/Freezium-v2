using LiteDB;

namespace Freezium.Core.Models
{
    public class AnimeUser
    {
        [BsonIgnore]
        public bool success { get; set; }

        public bool watch_list { get; set; }
        public bool follow { get; set; }
        public bool favorite { get; set; }
    }
}
