using System.Collections.Generic;
using Freezium.Core.Models;

namespace Freezium.Core.Interfaces
{
    /// <summary>
    /// Repository interface for anime data.
    /// Includes WatchList, Follow, Favorite CRUD operations and anime cache management.
    /// </summary>
    public interface IAnimeRepository
    {
        // Anime cache
        Anime GetCachedAnime(string id);
        void CacheAnime(Anime anime);

        // Watch List
        bool AddWatchList(string id);
        void RemoveWatchList(string id);
        bool IsInWatchList(string id);
        List<Anime> GetWatchList();

        // Follow
        bool AddFollow(string id);
        void RemoveFollow(string id);
        bool IsInFollow(string id);

        // Favorite
        bool AddFavorite(string id);
        void RemoveFavorite(string id);
        bool IsInFavorite(string id);
        List<Anime> GetFavoriteList();
    }
}
