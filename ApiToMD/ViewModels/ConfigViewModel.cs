using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiToMD.Helpers;
using Windows.Storage;

namespace ApiToMD.ViewModels
{
    public class ConfigViewModel : Observable
    {
        /// <summary>
        ///  作者
        /// </summary>
        private string _author;
        public string Author
        {
            get { return _author; }
            set { Set(ref _author, value); }
        }

        /// <summary>
        /// 本地地址
        /// </summary>
        private string _localUrl;
        public string LocalUrl
        {
            get { return _localUrl; }
            set { Set(ref _localUrl, value); }
        }

        /// <summary>
        /// 实际地址
        /// </summary>
        private string _actualUrl;
        public string ActualUrl
        {
            get { return _actualUrl; }
            set { Set(ref _actualUrl, value); }
        }

        public ConfigViewModel()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            Author = localSettings.Values["Author"] as string;
            LocalUrl = localSettings.Values["LocalUrl"] as string;
            ActualUrl = localSettings.Values["ActualUrl"] as string;
        }


        /// <summary>
        /// 保存配置
        /// </summary>
        public async void OnSaveConfigClick()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Author"] = Author;
            localSettings.Values["LocalUrl"] = LocalUrl;
            localSettings.Values["ActualUrl"] = ActualUrl;

        }

    }
}
