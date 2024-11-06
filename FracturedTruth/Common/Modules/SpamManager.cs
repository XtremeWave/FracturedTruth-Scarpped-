using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FracturedTruth.Attributes;
using static FracturedTruth.Translator;
using static FracturedTruth.Helpers.PathHelper;


namespace FracturedTruth.Common.Modules;

public static class SpamManager
{
    public static List<string> BanWords = new();

    public static void Init()
    {
        CreateIfNotExists();
        BanWords = ReturnAllNewLinesInFile(banedwordsPath);
    }
    public static void CreateIfNotExists()
    {
        if (!File.Exists(banedwordsPath))
        {
            try
            {
                string fileName = GetUserLangByRegion().ToString();
                Logger.Warn($"Create New BanWords: {fileName}", "SpamManager");
                File.WriteAllText(banedwordsPath, GetResourcesTxt($"FracturedTruth_Data.Resources.Configs.BanWords.{fileName}.txt"));
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "SpamManager");
            }
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static List<string> ReturnAllNewLinesInFile(string filename)
    {
        if (!File.Exists(filename)) return new List<string>();
        using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
        string text;
        List<string> sendList = new();
        while ((text = sr.ReadLine()) != null)
            if (text.Length > 1 && text != "") sendList.Add(text.Replace("\\n", "\n").ToLower());
        return sendList;
    }
    public static void CheckSpam(PlayerControl player, ref string text)
    {
        if (player.AmOwner || !AmongUsClient.Instance.AmHost) return;

        var mt = text;
        bool banned = BanWords.Any(mt.Contains);

        if (banned)
        {
            foreach (string word in BanWords)
            {
                if (text.Contains(word))
                {
                    text = text.Replace(word, new string('*', word.Length));
                }
            }

        }

    }

}
