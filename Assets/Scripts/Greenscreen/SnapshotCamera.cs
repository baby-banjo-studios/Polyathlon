using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SnapshotCamera : MonoBehaviour
{
    private Camera snapCam;

    public int resWidth;// = 256;
    public int resHeight;// = 256;

    // protected DateTime dt;

    private void Awake()
    {
        snapCam = GetComponent<Camera>();
        SetResolution(resWidth, resHeight);
    }

    public void SetResolution(int width, int height)
    {
        resWidth = width;
        resHeight = height;

        snapCam.targetTexture = new RenderTexture(resWidth, resHeight, 24);
    }

    public void RenderAndExport(int width, int height, string filename)
    {
        Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        snapCam.Render();
        RenderTexture.active = snapCam.targetTexture;
        snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        byte[] bytes = snapshot.EncodeToPNG();
        
        string directoryName = Path.GetDirectoryName(filename);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);    
        }

        File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Snapshot saved to {0}", filename));
    }

    public void TakeSnapshot(bool alsoCreateThumbnail)
    {
        DateTime dt = DateTime.Now;
        
        RenderAndExport(resWidth, resHeight, SnapshotName(false, dt));

        // also create thumbnail for gallery if needed
        if (alsoCreateThumbnail)
        {
            int maxDim = 128;
            int thumbWidth, thumbHeight;
            if (resHeight > resWidth)
            {
                thumbWidth = (int)(((double)resWidth / (double)resHeight) * maxDim);
                thumbHeight = maxDim;
            }
            else
            {
                thumbWidth = maxDim;
                thumbHeight = (int)(((double)resHeight / (double)resWidth) * maxDim);
            }

            RenderTexture thumbRT = new RenderTexture(thumbWidth, thumbHeight, 24);
            Graphics.Blit(RenderTexture.active, thumbRT);
            RenderTexture.active = thumbRT;
           
            RenderAndExport(thumbWidth, thumbHeight, SnapshotName(true, dt));
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    protected virtual string SnapshotName(bool thumbnail, DateTime dt)
    {
        string basePath;
#if UNITY_EDITOR
        basePath = Application.dataPath;
#else
        basePath = Application.persistentDataPath;
#endif
        return string.Format("{0}/Snapshots/{1}snap_{2}.png", Application.dataPath, thumbnail ? "Thumbnails/" : "", dt.ToString("yyyyMMddHHmmssfff"));
    }
}