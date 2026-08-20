using System;
using System.Reflection;

namespace Freezium.Core.Models
{
    public class AppSettings : IAppSettings
    {
        public IAppSettings GetProxy()
        {
            return ObservableProxy<IAppSettings>.Create(this, _ => 
                Services.AppSettingsService.Save());
        }

        public AppSettings Get() => this;

        public int Id { get; } = 1;
        public bool ManipulateWL { get; set; } = true;
        public string CfControl { get; set; }
    }

    public interface IAppSettings
    {
        bool ManipulateWL { get; set; }
        string CfControl { get; set; }
        AppSettings Get();
    }

    public class ObservableProxy<T> : DispatchProxy
    {
        private T _instance;
        public event Action<string> PropertyChanged;

        public static T Create(T instance, Action<string> onChanged)
        {
            var proxy = Create<T, ObservableProxy<T>>() as ObservableProxy<T>;
            proxy._instance = instance;
            proxy.PropertyChanged += onChanged;
            return (T)(object)proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var result = targetMethod.Invoke(_instance, args);
            if (targetMethod.Name.StartsWith("set_"))
            {
                string propName = targetMethod.Name.Substring(4);
                PropertyChanged?.Invoke(propName);
            }
            return result;
        }
    }
}
