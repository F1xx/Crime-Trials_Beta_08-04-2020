using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;


public static class BuildResources
{
    static string copyFrom = Application.dataPath + "/Resources";
    static string copyTo = "";

    static List<FileSystemInfo> filesToCopy;

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        copyTo = pathToBuiltProject.Replace(".exe","_Data/Resources");
        copyFrom = copyFrom.Replace("/", "\\");

        filesToCopy = new List<FileSystemInfo>();

        CollectAllFilesToLoad(copyFrom);

        CopyAllFiles();
    }

    private static void CollectAllFilesToLoad(string BaseDirectory)
    {
        if (Directory.Exists(BaseDirectory))
        {
            DirectoryInfo info = new DirectoryInfo(BaseDirectory);

            foreach (DirectoryInfo directory in info.GetDirectories())
            {
                filesToCopy.Add(directory);
            }

            foreach (FileInfo item in info.GetFiles())
            {
                filesToCopy.Add(item);
            }
        }
    }

    private static void CopyAllFiles()
    {
        foreach (var file in filesToCopy)
        {
            string outputPath = copyTo;
            string windowsFilename = file.FullName.Replace("/", "\\");

            outputPath += windowsFilename.Replace(copyFrom, "");

            outputPath = outputPath.Replace("\\", "/");
            string inputPath = file.FullName.Replace("\\", "/");

            FileUtil.CopyFileOrDirectory(inputPath, outputPath);
        }
    }

}
