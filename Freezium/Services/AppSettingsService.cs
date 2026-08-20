using Freezium.Core.Interfaces;
using Freezium.Core.Models;

namespace Freezium.Services
{
    /// <summary>
    /// Management of application settings. Accessible globally via static accessor.
    /// Changes are automatically persisted via ObservableProxy.
    /// </summary>
    public static class AppSettingsService
    {
        private static IAppSettings _current;
        private static ISettingsRepository _repository;

        /// <summary>
        /// Access to current application settings (via ObservableProxy).
        /// </summary>
        public static IAppSettings Current
        {
            get => _current;
            private set => _current = value;
        }

        /// <summary>
        /// Initializes the repository and loads the settings.
        /// </summary>
        public static void Initialize(ISettingsRepository repository)
        {
            _repository = repository;

            var settings = _repository.Load();
            if (settings != null)
            {
                Current = settings.GetProxy();
            }
            else
            {
                Current = new AppSettings().GetProxy();
            }
        }

        /// <summary>
        /// Persistently saves the current settings.
        /// Automatically called by ObservableProxy.
        /// </summary>
        public static void Save()
        {
            if (_repository != null && _current != null)
            {
                _repository.Save(_current.Get());
            }
        }
    }
}
