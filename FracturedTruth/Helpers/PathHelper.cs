using FracturedTruth.Attributes;
using FracturedTruth.Common.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracturedTruth.Helpers;

public static class PathHelper
{
    public const string GithubUrl = @"https://github.com/XtremeWave/FracturedTruth/";
    public const string GiteeUrl = @"https://gitee.com/XtremeWave/FracturedTruth/";
    public const string ObjectStorageUrl = @"https://cn-sy1.rains3.com/xtremewavecloud/cloudreve/5/FracturedTruth/";
    public static IReadOnlyList<string> URLs_info => new List<string>
    {
#if DEBUG
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ft_info.json")}",
#else
        "https://raw.githubusercontent.com/XtremeWave/FracturedTruth/FracturedTruth/ft_info.json",
        "https://gitee.com/XtremeWave/FracturedTruth/raw/FracturedTruth/ft_info.json",
#endif
    }; 
    public static IReadOnlyList<string> URLs_resources => new List<string>
    {
#if DEBUG
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ft_resources.json")}",
#else
        "https://raw.githubusercontent.com/XtremeWave/FracturedTruth/FracturedTruth/ft_resources.json",
        "https://gitee.com/XtremeWave/FracturedTruth/raw/FracturedTruth/ft_resources.json",
#endif
    };

    public static readonly string modUpdater_github = GithubUrl + "releases/latest/download/FracturedTruth.dll";
    public static readonly string modUpdater_gitee = GiteeUrl + "releases/download/v{{showVer}}/FracturedTruth.dll";
    public static readonly string modUpdater_objectstorage = ObjectStorageUrl + "FracturedTruth.dll";
    public static readonly string modUpdater_savepath = "BepInEx/plugins/FracturedTruth.dll.temp";
    public static readonly string denynamelistPath = BanSavePath + "DenyName.txt";
    public static readonly string banlistPath = BanSavePath + "BanList.txt";
    public static readonly string banedwordsPath = BanSavePath + "BanWords.txt";

    public const string ResourcesUrl = "raw/FracturedTruth/Assets/";
    public const string ResourcesSavePath = "FracturedTruth_Data/Resources/";
    public const string DependsSavePath = "BepInEx/core/";
    public const string BanSavePath = "FracturedTruth_Data/Ban/";

    public const string audioPath = "Audios/";
    public const string imagePath = "Images/";
    public const string dependPath = "Depends/";
    public const string initfilePath = "InitFiles/";
    public const string filestatePath = ResourcesSavePath + "FileStates/";
    public const string packagePath = "{{packagename}}/";

    public static readonly string imageDownloader_github = GithubUrl + ResourcesUrl + imagePath;
    public static readonly string imageDownloader_gitee = GiteeUrl + ResourcesUrl + imagePath;
    public static readonly string imageDownloader_objectstorage = ObjectStorageUrl + imagePath;
    public static readonly string imageDownloader_savepath = ResourcesSavePath + imagePath;

    public static readonly string initfileDownloader_github = GithubUrl + ResourcesUrl + initfilePath;
    public static readonly string initfileDownloader_gitee = GiteeUrl + ResourcesUrl + initfilePath;
    public static readonly string initfileDownloader_objectstorage = ObjectStorageUrl + initfilePath;
    public static readonly string initfileDownloader_savepath = ResourcesSavePath + initfilePath;

    public static readonly string dependDownloader_github = GithubUrl + ResourcesUrl + dependPath;
    public static readonly string dependDownloader_gitee = GiteeUrl + ResourcesUrl + dependPath;
    public static readonly string dependDownloader_objectstorage = ObjectStorageUrl + dependPath;
    public static readonly string dependDownloader_savepath = DependsSavePath;

    public static readonly string audioDownloader_github = GithubUrl + ResourcesUrl + audioPath;
    public static readonly string audioDownloader_gitee = GiteeUrl + ResourcesUrl + audioPath;
    public static readonly string audioDownloader_objectstorage = ObjectStorageUrl + audioPath;
    public static readonly string audioDownloader_savepath = ResourcesSavePath + audioPath;

    public static readonly string packageDownloader_github = GithubUrl + ResourcesUrl + packagePath;
    public static readonly string packageDownloader_gitee = GiteeUrl + ResourcesUrl + packagePath;
    public static readonly string packageDownloader_objectstorage = ObjectStorageUrl + packagePath;

    [PluginModuleInitializer(InitializePriority.VeryHigh)]
    static void InitForAllPaths()
    {
        DirectoryInfo folder;
        if (!Directory.Exists(DependsSavePath))
            Directory.CreateDirectory(DependsSavePath);

        if (!Directory.Exists(ResourcesSavePath))
            Directory.CreateDirectory(ResourcesSavePath);
        folder = new(ResourcesSavePath);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;

        if (!Directory.Exists(audioDownloader_savepath))
            Directory.CreateDirectory(audioDownloader_savepath);
        folder = new(audioDownloader_savepath);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;

        if (!Directory.Exists(imageDownloader_savepath))
            Directory.CreateDirectory(imageDownloader_savepath);
        folder = new(imageDownloader_savepath);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;

        if (!Directory.Exists(filestatePath))
            Directory.CreateDirectory(filestatePath);
        folder = new(filestatePath);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;

        if (!Directory.Exists(BanSavePath))
            Directory.CreateDirectory(BanSavePath);

        if (!File.Exists(banlistPath))
            File.Create(banlistPath).Close();

        if (!File.Exists(denynamelistPath))
            File.Create(denynamelistPath).Close();


        SpamManager.Init();
    }
}
