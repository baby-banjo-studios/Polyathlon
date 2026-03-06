using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SnapshotCamera : MonoBehaviour
{
    private Camera snapCam;

    public int resWidth;// = 256;
    public int resHeight;// = 256;

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

    public void TakeSnapshot(bool alsoCreateThumbnail)
    {
        Texture2D snapshot = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
        snapCam.Render();
        RenderTexture.active = snapCam.targetTexture;
        snapshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        byte[] bytes = snapshot.EncodeToPNG();
        DateTime dt = DateTime.Now;
        string filename = SnapshotName(false, dt);
        
        string directoryName = Path.GetDirectoryName(filename);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);    
        }

        File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Snapshot saved to {0}", filename));

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
            Texture2D thumb = new Texture2D(thumbWidth, thumbHeight, TextureFormat.RGBA32, false);
            thumb.ReadPixels(new Rect(0, 0, thumbWidth, thumbHeight), 0, 0);
            byte[] thumbBytes = thumb.EncodeToPNG();
            filename = SnapshotName(true, dt);
            directoryName = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);    
            }
            File.WriteAllBytes(filename, thumbBytes);
            Debug.Log(string.Format("Thumbnail saved to {0}", filename));
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private string SnapshotName(bool thumbnail, DateTime dt)
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