using Freezium.Core.Models;

namespace Freezium.Core.Interfaces
{
    /// <summary>
    /// Interface for persistent storage of application settings.
    /// </summary>
    public interface ISettingsRepository
    {
        void Save(AppSettings settings);
        AppSettings Load();
    }
}
