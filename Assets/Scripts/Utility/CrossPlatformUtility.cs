using System;
using System.IO;
using UnityEngine;
public class CrossPlatformUtility
{
    public static void OpenFileExplorer(string folderpath)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // running on windows
		string winPath = folderpath.Replace("/", "\\"); // windows explorer doesn't like forward slashes
        if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
		{
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", winPath);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Failed to open windows file explorer - {0}", e.Message));
            }
		}
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // running on mac
        string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes		
        if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
		{
            if (!macPath.StartsWith("\""))
            {
                macPath = "\"" + macPath;
            }
            if (!macPath.EndsWith("\""))
            {
                macPath = macPath + "\"";
            }
            try
            {
                System.Diagnostics.Process.Start("open", macPath);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Failed to open mac file explorer - {0}", e.Message));
            }
		}
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        // running on linux
		string linPath = folderpath.Replace("\\", "/"); // linux explorer doesn't like forward slashes
        if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
		{
            try
            {
                System.Diagnostics.Process.Start("xdg-open", linPath);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Failed to open linux file explorer - {0}", e.Message));
            }
		}
#else
        // running on something else
        Debug.Log(string.Format("Running on non-PC console, cannot open file explorer", e.Message));
#endif
    }
}