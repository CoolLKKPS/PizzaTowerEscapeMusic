using BepInEx;
using System.IO;
using System.Reflection;

namespace PizzaTowerEscapeMusic
{
    internal static class CustomManager
    {
        public static string GetFilePath(string path, string fallbackPath)
        {
            string[] directories = Directory.GetDirectories(Paths.PluginPath);
            for (int i = 0; i < directories.Length; i++)
            {
                string text = directories[i] + "/BGN-PizzaTowerEscapeMusic/" + path;
                if (File.Exists(text))
                {
                    return text;
                }
            }
            string text2 = Paths.PluginPath + "/BGN-PizzaTowerEscapeMusic_Custom/" + path;
            if (File.Exists(text2))
            {
                return text2;
            }
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + fallbackPath;
        }
    }
}
