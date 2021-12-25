using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace FileWire
{
    public class Downloader : IDisposable
    {
        private Uri link;
        private string savePath;
        private static int bufferSize = 819200;
        private DownloadCompleteEventHandler downloadCompleteEventHandler = null;
        private DownloadProgressChangedEventHandler downloadProgressChangedEventHandler = null;
        private DownloadFailedEventHandler downloadFailedEventHandler = null;


        public Downloader(Uri uri, string v)
        {
            this.link = uri;
            this.savePath = v;

            new Thread(new ThreadStart(startDownload)).Start();
        }

        private void startDownload()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                WebRequest webRequest = WebRequest.CreateHttp(link);
                WebResponse response = webRequest.GetResponse();
                long totalSize = long.Parse(response.Headers["Content-Length"]);
                Stream input = response.GetResponseStream();


                if ((!(File.Exists(savePath) && new FileInfo(savePath).Length == totalSize)) || true)
                {
                    FileStream fs = File.Create(savePath);
                    byte[] buffer = new byte[bufferSize];

                    long total = 0;
                    try
                    {


                        int count = 0;
                        while ((count = input.Read(buffer, 0, bufferSize)) != 0)
                        {
                            total += count;
                            if (downloadProgressChangedEventHandler != null)
                            {
                                downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs((total * 100) / totalSize));
                            }
                            fs.Write(buffer, 0, count);
                        }
                        fs.Close();

                        if (downloadCompleteEventHandler != null)
                        {
                            downloadCompleteEventHandler.DownloadComplete.Invoke(this);
                        }
                    }
                    catch (Exception)
                    {
                        fs.Close();
                        File.Delete(savePath);
                        if (downloadFailedEventHandler != null)
                            downloadFailedEventHandler.downloadFailed.Invoke(this);
                    }
                }
                else
                {

                    if (downloadCompleteEventHandler != null)
                    {
                        downloadCompleteEventHandler.DownloadComplete.Invoke(this);
                    }
                }

            }
            catch (Exception)
            {

                if (downloadFailedEventHandler != null)
                    downloadFailedEventHandler.downloadFailed.Invoke(this);
            }

        }



        public void Dispose()
        {
        }
        public class DownloadProgressChangedEventHandler
        {
            public Action<object, DownloadProgressChangedEventArgs> progressChange;

            public DownloadProgressChangedEventHandler(Action<object, DownloadProgressChangedEventArgs> progressChange)
            {
                this.progressChange = progressChange;
            }

            public class DownloadProgressChangedEventArgs
            {
                public DownloadProgressChangedEventArgs(long bytes)
                {
                    this.BytesReceived = bytes;
                }

                public long BytesReceived { get; set; }
            }
        }
        public class DownloadCompleteEventHandler
        {
            public Action<object> DownloadComplete;

            public DownloadCompleteEventHandler(Action<object> downloadComplete)
            {
                this.DownloadComplete = downloadComplete;
            }
        }

        public class DownloadFailedEventHandler
        {
            public Action<object> downloadFailed;

            public DownloadFailedEventHandler(Action<object> downloadFailed)
            {
                this.downloadFailed = downloadFailed;
            }
        }
        public void setOnDownloadProgressEventHandler(DownloadProgressChangedEventHandler downloadProgressChangedEventHandler)
        {
            this.downloadProgressChangedEventHandler = downloadProgressChangedEventHandler;
        }
        public void setOnDownloadCompleteEventHandler(DownloadCompleteEventHandler downloadCompleteEventHandler)
        {
            this.downloadCompleteEventHandler = downloadCompleteEventHandler;
        }

        public void setOnDownloadFailedEventHandler(DownloadFailedEventHandler downloadFailedEventHandler)
        {
            this.downloadCompleteEventHandler = downloadCompleteEventHandler;
        }

    }
}