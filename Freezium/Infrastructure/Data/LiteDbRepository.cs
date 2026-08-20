using System;
using System.Collections.Generic;
using System.Linq;
using Freezium.Core;
using Freezium.Core.Interfaces;
using Freezium.Core.Models;
using LiteDB;

namespace Freezium.Infrastructure.Data
{
    /// <summary>
    /// LiteDB-based repository implementation.
    /// Anime cache, WatchList/Follow/Favorite CRUD and Settings persistence.
    /// </summary>
    public class LiteDbRepository : IAnimeRepository, ISettingsRepository
    {
        private readonly LiteDatabase _db;
        private readonly IAnimeApiClient _apiClient;

        public LiteDbRepository(IAnimeApiClient apiClient)
        {
            string dir = System.IO.Path.GetDirectoryName(Constants.DatabasePath);
            if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

            _db = new LiteDatabase(Constants.DatabasePath);
            _apiClient = apiClient;
        }

        #region Settings

        public void Save(AppSettings settings)
        {
            var collection = _db.GetCollection<AppSettings>("settings");
            collection.Upsert(settings);
        }

        public AppSettings Load()
        {
            var collection = _db.GetCollection<AppSettings>("settings");
            return collection.FindById(1);
        }

        #endregion

        #region Anime Cache

        public Anime GetCachedAnime(string id)
        {
            var cache = _db.GetCollection<Anime>("Anime_Cache");
            var found = cache.Find(x => x.ID == id);

            if (found.Any())
            {
                var data = found.FirstOrDefault();
                if (DateTime.UtcNow.CompareTo(data.Expire) < 0)
                {
                    return data;
                }
            }

            // Cache miss or expired - fetch from API
            var anime = _apiClient.GetAnime(id);
            if (anime != null)
            {
                anime.Expire = DateTime.UtcNow.AddDays(7);
                cache.Upsert(anime);
            }

            return anime;
        }

        public void CacheAnime(Anime anime)
        {
            var cache = _db.GetCollection<Anime>("Anime_Cache");
            cache.Upsert(anime);
        }

        #endregion

        #region Watch List

        public bool AddWatchList(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var anime = GetCachedAnime(id);

            if (anime == null)
                return false;

            var existing = wl.FindOne(x => x.id == id);
            if (existing != null)
            {
                existing.Data.watch_list = true;
                wl.Update(existing);
            }
            else
            {
                wl.Insert(new WatchList
                {
                    id = anime.ID,
                    Data = new AnimeUser { watch_list = true }
                });
            }

            return true;
        }

        public void RemoveWatchList(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);

            if (found != null)
            {
                found.Data.watch_list = false;
                wl.Update(found);
            }
        }

        public bool IsInWatchList(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);
            return found?.Data.watch_list ?? false;
        }

        public List<Anime> GetWatchList()
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var list = new List<Anime>();

            foreach (var item in wl.Find(x => x.Data.watch_list == true))
            {
                var anime = GetCachedAnime(item.id);
                if (anime != null)
                    list.Add(anime);
            }

            return list;
        }

        #endregion

        #region Follow

        public bool AddFollow(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var anime = GetCachedAnime(id);

            if (anime == null)
                return false;

            var existing = wl.FindOne(x => x.id == id);
            if (existing != null)
            {
                existing.Data.follow = true;
                wl.Update(existing);
            }
            else
            {
                wl.Insert(new WatchList
                {
                    id = anime.ID,
                    Data = new AnimeUser { follow = true }
                });
            }

            return true;
        }

        public void RemoveFollow(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);

            if (found != null)
            {
                found.Data.follow = false;
                wl.Update(found);
            }
        }

        public bool IsInFollow(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);
            return found?.Data.follow ?? false;
        }

        #endregion

        #region Favorite

        public bool AddFavorite(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var anime = GetCachedAnime(id);

            if (anime == null)
                return false;

            var existing = wl.FindOne(x => x.id == id);
            if (existing != null)
            {
                existing.Data.favorite = true;
                wl.Update(existing);
            }
            else
            {
                wl.Insert(new WatchList
                {
                    id = anime.ID,
                    Data = new AnimeUser { favorite = true }
                });
            }

            return true;
        }

        public void RemoveFavorite(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);

            if (found != null)
            {
                found.Data.favorite = false;
                wl.Update(found);
            }
        }

        public bool IsInFavorite(string id)
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var found = wl.FindOne(x => x.id == id);
            return found?.Data.favorite ?? false;
        }

        public List<Anime> GetFavoriteList()
        {
            var wl = _db.GetCollection<WatchList>("Watch_List");
            var list = new List<Anime>();

            foreach (var item in wl.Find(x => x.Data.favorite == true))
            {
                var anime = GetCachedAnime(item.id);
                if (anime != null)
                    list.Add(anime);
            }

            return list;
        }

        #endregion
    }
}
