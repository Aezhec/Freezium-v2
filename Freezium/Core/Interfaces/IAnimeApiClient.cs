using Freezium.Core.Models;

namespace Freezium.Core.Interfaces
{
    /// <summary>
    /// Abstracts HTTP requests made to the Anizium API.
    /// </summary>
    public interface IAnimeApiClient
    {
        Anime GetAnime(string id);
    }
}
