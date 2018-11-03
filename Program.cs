using System;
using System.Threading;
using System.Net;
using System.IO;

namespace UnstableServerDownloader
{
    class Program
    {
        public class Options
        {
            public static Uri Url { get; set; }

            public static string Path { get; set; }
        }
        // Value in bytes / this = value in mebibytes (MiB)
        const double _bytesToMiB = 1.049e+6;
        static int _bytesRead;
        static int _fileSize;

        // Read buffer
        static byte[] _buffer;
        static void Main(string[] args)
        {
            // TODO: Replace this with an actual commandline argument parser
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Please specify the URL from which you want to download a file.");
                    return;
                case 1:
                    Options.Url = new Uri(args[0]);
                    Options.Path = Path.GetFullPath(Path.GetFileName(Options.Url.LocalPath));
                    break;
                case 2:
                    Options.Url = new Uri(args[0]);
                    Options.Path = Path.GetFullPath(args[1]);
                    break;
            }

            // Periodic progress report
            Timer timer = new Timer(Progress, "", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.75));

            // New web request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Options.Url);

            // Variable setup, fetching response
            using (WebResponse response = request.GetResponse())
            {
                _fileSize = (int)response.ContentLength;
                _buffer = new byte[_fileSize];

                using (BinaryReader ns = new BinaryReader(response.GetResponseStream()))
                {
                RestartDL:
                    try
                    {
                        // Read until the whole file is read
                        while (_bytesRead < _fileSize)
                            _bytesRead += ns.BaseStream.Read(_buffer, _bytesRead, _fileSize - _bytesRead);
                    }
                    catch
                    {
                        // Try again if an IO error occurs
                        goto RestartDL;
                    }
                }
            }

            // Write contents to a file
            File.WriteAllBytes(Options.Path, _buffer);
            Console.WriteLine("Download complete.");
        }

        private static void Progress(object state)
        {
            // Using string interpolation to report the progress
            // .ToString("F") rounds it to two decimal places
            Console.WriteLine($"Downloaded {(_bytesRead / _bytesToMiB).ToString("F")}MiB/{(_fileSize / _bytesToMiB).ToString("F")}MiB");
        }
    }
}
