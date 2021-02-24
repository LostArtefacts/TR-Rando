using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TR2RandomizerView.Updates
{
    public class UpdateChecker
    {
        private static UpdateChecker _instance;

        public static UpdateChecker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UpdateChecker();
                }
                return _instance;
            }
        }

        private const string _updateUrl = "https://api.github.com/repos/DanzaG/TR2-Rando/releases/latest";
        private const string _userAgent = "TR2-Rando"; //https://docs.github.com/en/rest/overview/resources-in-the-rest-api#user-agent-required

        private readonly TimeSpan _initialDelay, _periodicDelay;
        private readonly CancellationTokenSource _cancelSource;
        private readonly CancellationToken _cancelToken;

        public event EventHandler<UpdateEventArgs> UpdateAvailable;

        public Update LatestUpdate { get; private set; }

        private UpdateChecker()
        {
            _initialDelay = new TimeSpan(0, 0, 20);
            _periodicDelay = new TimeSpan(0, 30, 0);

            _cancelSource = new CancellationTokenSource();
            _cancelToken = _cancelSource.Token;

            Application.Current.Exit += Application_Exit;

            Run();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _cancelSource.Cancel();
        }

        private async void Run()
        {
            await Task.Delay(_initialDelay, _cancelToken);
            do
            {
                if (!_cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        CheckForUpdates();
                    }
                    catch { }

                    await Task.Delay(_periodicDelay, _cancelToken);
                }
            }
            while (!_cancelToken.IsCancellationRequested);
        }

        public bool CheckForUpdates()
        {
            HttpWebRequest req = WebRequest.CreateHttp(_updateUrl);
            req.UserAgent = _userAgent;
            req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            using (WebResponse response = req.GetResponse())
            using (Stream receiveStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(receiveStream))
            {
                Dictionary<string, object> releaseInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());
                string currentVersion = ((App)Application.Current).TaggedVersion;
                if (!releaseInfo.ContainsKey("tag_name"))
                {
                    throw new IOException("Invalid response from GitHub - missing tag_name field.");
                }

                string latestVersion = releaseInfo["tag_name"].ToString();
                if (latestVersion.Equals(currentVersion))
                {
                    return false;
                }

                LatestUpdate = new Update
                {
                    CurrentVersion = currentVersion,
                    NewVersion = latestVersion,
                    ReleaseDate = DateTime.Parse(releaseInfo["published_at"].ToString()),
                    UpdateBody = releaseInfo["body"].ToString(),
                    UpdateURL = releaseInfo["html_url"].ToString()
                };

                UpdateAvailable?.Invoke(this, new UpdateEventArgs(LatestUpdate));

                return true;
            }
        }
    }
}