using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static FracturedTruth.Translator;
using static FracturedTruth.Helpers.PathHelper;
using FracturedTruth.Modules;
using FracturedTruth.Common.Panels.Audio;

namespace FracturedTruth.Common.ClientFeatures.ResourcesManager;

public static class ResourcesDownloader
{
    public static async Task<bool> StartDownload(string resourcepath, string localpath, string filename)
    {
        if (File.Exists(audioDownloader_savepath + filename) || File.Exists(imageDownloader_savepath + filename))
        {
            return true;
        }

        resourcepath = resourcepath + filename;
        var sp = filename.Split(".");
        var name = sp[0];
        var ext = sp[1];
        var DownloadFilePath = localpath + filename;

        if (!IsValidUrl(resourcepath))
        {
            Logger.Error($"Invalid URL: {resourcepath}", "Download Resources", false);
            return false;
        }

        if (ext is "wav" && localpath != audioDownloader_savepath + filename)
        {
            if (Enum.IsDefined(typeof(SupportMusics), name))
            {
                SupportMusics musicType = (SupportMusics)Enum.Parse(typeof(SupportMusics), name, true);
                new FracturedSounds(musicType);
            }
            else if (Enum.IsDefined(typeof(XtremeSounds), name))
            {
                XtremeSounds soundType = (XtremeSounds)Enum.Parse(typeof(XtremeSounds), name, true);
                new FracturedSounds(soundType);
            }

            DownloadFilePath =  audioDownloader_savepath + filename;
        }
        else if (ext is "png" or "jpg" && localpath != imageDownloader_savepath + filename)
        {
            DownloadFilePath = imageDownloader_savepath + filename;
        }


        string DownloadFileTempPath = DownloadFilePath + ".xwr";
        File.Create(DownloadFileTempPath).Close();
        Logger.Msg("Start Downloading from: " + resourcepath, "Download Resources");
        Logger.Msg("Saving file to: " + DownloadFilePath, "Download Resources");

        try
        {
            using var client = new HttpClientDownloadWithProgress(resourcepath, DownloadFileTempPath);
            await client.StartDownload();
            Thread.Sleep(100);
            Logger.Info($"Succeed in {resourcepath}", "Download Resources");
            File.Move(DownloadFileTempPath, DownloadFilePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "Download Resources", false);
            File.Delete(DownloadFileTempPath);
            return false;
        }

    }
    private static bool IsValidUrl(string url)
    {
        string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
    }
}