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
    /// Thread-safe LiteDB-based repository implementation.
    /// Anime cache, WatchList/Follow/Favorite CRUD and Settings persistence.
    /// </summary>
    public class LiteDbRepository : IAnimeRepository, ISettingsRepository, IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly IAnimeApiClient _apiClient;
        private readonly object _dbLock = new object();
        private bool _disposed;

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
            if (settings == null) return;

            lock (_dbLock)
            {
                if (_disposed) return;
                var collection = _db.GetCollection<AppSettings>("settings");
                collection.Upsert(settings);
            }
        }

        public AppSettings Load()
        {
            lock (_dbLock)
            {
                if (_disposed) return null;
                var collection = _db.GetCollection<AppSettings>("settings");
                return collection.FindById(1);
            }
        }

        #endregion

        #region Anime Cache

        public Anime GetCachedAnime(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            Anime cachedData = null;

            lock (_dbLock)
            {
                if (_disposed) return null;
                var cache = _db.GetCollection<Anime>("Anime_Cache");
                var found = cache.Find(x => x.ID == id).FirstOrDefault();

                if (found != null)
                {
                    if (DateTime.UtcNow.CompareTo(found.Expire) < 0)
                    {
                        return found;
                    }
                    cachedData = found; // Kept as fallback in case API fetch fails
                }
            }

            // Cache miss or expired - fetch from API
            try
            {
                var anime = _apiClient.GetAnime(id);
                if (anime != null)
                {
                    anime.Expire = DateTime.UtcNow.AddDays(7);
                    lock (_dbLock)
                    {
                        if (!_disposed)
                        {
                            var cache = _db.GetCollection<Anime>("Anime_Cache");
                            cache.Upsert(anime);
                        }
                    }
                    return anime;
                }
            }
            catch
            {
                // Fallback to expired cache if API call fails
            }

            return cachedData;
        }

        public void CacheAnime(Anime anime)
        {
            if (anime == null || string.IsNullOrEmpty(anime.ID)) return;

            lock (_dbLock)
            {
                if (_disposed) return;
                var cache = _db.GetCollection<Anime>("Anime_Cache");
                cache.Upsert(anime);
            }
        }

        #endregion

        #region Watch List

        public bool AddWatchList(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            var anime = GetCachedAnime(id);
            if (anime == null) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var existing = wl.FindOne(x => x.id == id);
                if (existing != null)
                {
                    if (existing.Data == null) existing.Data = new AnimeUser();
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
            }

            return true;
        }

        public void RemoveWatchList(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            lock (_dbLock)
            {
                if (_disposed) return;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);

                if (found != null)
                {
                    if (found.Data != null)
                    {
                        found.Data.watch_list = false;
                        wl.Update(found);
                    }
                }
            }
        }

        public bool IsInWatchList(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);
                return found?.Data?.watch_list ?? false;
            }
        }

        public List<Anime> GetWatchList()
        {
            List<WatchList> items;
            lock (_dbLock)
            {
                if (_disposed) return new List<Anime>();
                var wl = _db.GetCollection<WatchList>("Watch_List");
                items = wl.Find(x => x.Data != null && x.Data.watch_list == true).ToList();
            }

            var list = new List<Anime>();
            foreach (var item in items)
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
            if (string.IsNullOrEmpty(id)) return false;

            var anime = GetCachedAnime(id);
            if (anime == null) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var existing = wl.FindOne(x => x.id == id);
                if (existing != null)
                {
                    if (existing.Data == null) existing.Data = new AnimeUser();
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
            }

            return true;
        }

        public void RemoveFollow(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            lock (_dbLock)
            {
                if (_disposed) return;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);

                if (found != null)
                {
                    if (found.Data != null)
                    {
                        found.Data.follow = false;
                        wl.Update(found);
                    }
                }
            }
        }

        public bool IsInFollow(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);
                return found?.Data?.follow ?? false;
            }
        }

        #endregion

        #region Favorite

        public bool AddFavorite(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            var anime = GetCachedAnime(id);
            if (anime == null) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var existing = wl.FindOne(x => x.id == id);
                if (existing != null)
                {
                    if (existing.Data == null) existing.Data = new AnimeUser();
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
            }

            return true;
        }

        public void RemoveFavorite(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            lock (_dbLock)
            {
                if (_disposed) return;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);

                if (found != null)
                {
                    if (found.Data != null)
                    {
                        found.Data.favorite = false;
                        wl.Update(found);
                    }
                }
            }
        }

        public bool IsInFavorite(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            lock (_dbLock)
            {
                if (_disposed) return false;
                var wl = _db.GetCollection<WatchList>("Watch_List");
                var found = wl.FindOne(x => x.id == id);
                return found?.Data?.favorite ?? false;
            }
        }

        public List<Anime> GetFavoriteList()
        {
            List<WatchList> items;
            lock (_dbLock)
            {
                if (_disposed) return new List<Anime>();
                var wl = _db.GetCollection<WatchList>("Watch_List");
                items = wl.Find(x => x.Data != null && x.Data.favorite == true).ToList();
            }

            var list = new List<Anime>();
            foreach (var item in items)
            {
                var anime = GetCachedAnime(item.id);
                if (anime != null)
                    list.Add(anime);
            }

            return list;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            lock (_dbLock)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _db?.Dispose();
                }
            }
        }

        #endregion
    }
}

