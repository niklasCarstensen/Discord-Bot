﻿using Discord.Audio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MEE7.Commands.Edit.Edit;
using Image = System.Drawing.Image;

namespace MEE7.Backend.HelperFunctions
{
    public static class MultiMediaHelper
    {
        static readonly string youtubeDownloadLock = "";

        private static Process CreateFFMPEGProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -filter:a \"volume = 0.05\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
        public static string DownloadVideoFromYouTube(string YoutubeURL)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return "";

            lock (youtubeDownloadLock)
            {
                string videofile = $"Downloads{Path.DirectorySeparatorChar}YoutubeVideo.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(videofile));
                if (File.Exists(videofile))
                {
                    int i = 0;
                    while (true)
                    {
                        if (File.Exists(videofile) && new FileInfo(videofile).IsFileLocked())
                            videofile = $"Downloads{Path.DirectorySeparatorChar}YoutubeVideo{++i}.mp4";
                        else
                        {
                            File.Delete(videofile);
                            break;
                        }
                    }
                }

                bool worked = false;
                $"youtube-dl.exe -f mp4 -o \"{videofile}\" {YoutubeURL}".RunAsConsoleCommand(25, () => { },
                    (s, e) => { if (s != null) worked = true; }, (StreamWriter w) => w.Write("e"));

                if (worked)
                    return videofile;
                else
                    return "";
            }
        }
        public static Process GetStreamFromYouTubeVideo(string YoutubeURL, string arguments = "")
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            return Process.Start(new ProcessStartInfo()
            {
                FileName = "youtube-dl",
                Arguments = $"{arguments} -o - {YoutubeURL}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static Process GetAudioStreamFromYouTubeVideo(string YoutubeURL, string audioFormat)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            string filename;
            if (Program.RunningOnLinux) filename = "./youtube-dl";
            else filename = "youtube-dl";

            return Process.Start(new ProcessStartInfo()
            {
                FileName = filename,
                Arguments = $"--audio-format {audioFormat} -o - {YoutubeURL}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static Process CreateFfmpegOut(string filePath)
        {
            string filename;
            if (Program.RunningOnLinux) filename = "./ffmpeg";
            else filename = "ffmpeg";

            return Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = $"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 -i {filePath} -acodec pcm_u8 -ar 22050 -f wav -",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, Stream stream)
        {
            Exception ex = null;
            using (AudioOutStream audioStream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await stream.CopyToAsync(audioStream); }
                catch (Exception e) { ex = e; }
                finally { await audioStream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, string path)
        {
            Exception ex = null;
            using (Process ffmpeg = CreateFFMPEGProcess(path))
            using (AudioOutStream stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                catch (Exception e) { ex = e; }
                finally { await stream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
        public static Gif ImageToGif(Image i)
        {
            FrameDimension dimension = new FrameDimension(i.FrameDimensionsList[0]);
            int[] timings = new int[i.GetFrameCount(dimension)];
            return new Gif(Enumerable.Range(0, i.GetFrameCount(dimension)).
                Select(x =>
                {
                    i.SelectActiveFrame(dimension, x);
                    try
                    {
                        timings[x] = BitConverter.ToInt32(i.GetPropertyItem(20736).Value, x * 4) * 10; // this works on windows sometimes
                    }
                    catch
                    {
                        try
                        {
                            var prop = i.GetPropertyItem(20736);
                            timings[x] = (prop.Value[0] + prop.Value[1] * 256) * 10; // this works according to https://stackoverflow.com/questions/3785031/getting-the-frame-duration-of-an-animated-gif
                        }
                        catch
                        {
                            timings[x] = 33; // just set it to 30fps lul, works for consoles
                        }
                    }
                    return new Bitmap(i);
                }).
                ToArray(), timings);
        }
    }
}
