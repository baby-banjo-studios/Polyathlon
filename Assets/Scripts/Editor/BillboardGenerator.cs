using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

public class BillboardGenerator : SnapshotCamera
{
    public MeshFilter objectToBillboard;
    protected float billboardHeight;// = 256;
    protected float billboardWidth;// = 256;
    private DateTime dt;
    public Material billboardMaterial; 
    public Material normalMapMaterial; 
    
    protected string FolderName { get => string.Format("{0}_{1}", objectToBillboard.name, dt.ToString("yyyyMMddHHmmssfff")); } 

    private void Start()
    {
        billboardHeight = objectToBillboard.mesh.bounds.size.y;
        billboardWidth = billboardHeight;
        dt = DateTime.Now;
        StartCoroutine(CreateBillboard());
    }

    private IEnumerator CreateBillboard()
    {
        // just to give scene a chance to render anything it may need to
        yield return null;
        
        // create folder for this billboard
        AssetDatabase.CreateFolder("Assets/Billboards", FolderName);

        // take snapshot of original tree
        RenderAndExport(resWidth, resHeight, SnapshotName(false, dt));
        yield return null;
        Debug.Break();

        // convert materials to shader to capture normals
        MeshRenderer meshRenderer = objectToBillboard.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material[] newMaterials = new Material[meshRenderer.materials.Length];
            Material[] oldMaterials = meshRenderer.materials;
            for (int i = 0; i < meshRenderer.materials.Length; i++)
            {
                newMaterials[i] = normalMapMaterial;
            }
            meshRenderer.materials = newMaterials;
        }
        yield return null;
        Debug.Break();

        // take snapshot of new tree
        RenderAndExport(resWidth, resHeight, SnapshotName(true, dt));
        yield return null;
        Debug.Break();

        // turn snapshots into a billboard
        CreateBillboardAsset();

        // break
        Debug.Break();
    }

    protected override string SnapshotName(bool thumbnail, DateTime dt)
    {
        return string.Format("{0}/Billboards/{1}/snap_{2}.png", Application.dataPath, FolderName, thumbnail ? "normal" : "texture");
    }

    private string GetAssetFilepath(string systemFilepath)
    {
        int assetIndex = systemFilepath.IndexOf("Assets/");
        string assetRelativePath = systemFilepath.Substring(assetIndex);
        return assetRelativePath;
    }

    private void CreateBillboardAsset()
    {
        // get regular texture
        string texturePath = GetAssetFilepath(SnapshotName(false, dt));
        AssetDatabase.ImportAsset(texturePath);
        Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        // get normal map
        string normalPath = GetAssetFilepath(SnapshotName(true, dt));
        AssetDatabase.ImportAsset(normalPath);
        Texture2D normalAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

        // create material for billboard
        Material uniqueMaterial = new Material(billboardMaterial);
        uniqueMaterial.mainTexture = textureAsset;            // 3. Assign the texture asset using Unity's default normal property ID
        uniqueMaterial.SetTexture("_BumpMap", normalAsset);
        uniqueMaterial.EnableKeyword("_NORMALMAP");

        string materialPath = string.Format("Assets/Billboards/{0}/GeneratedBillboardMat.mat", FolderName);
        materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
        AssetDatabase.CreateAsset(uniqueMaterial, materialPath);

        #if UNITY_EDITOR
        
        // 1. Create the asset instance in memory
        BillboardAsset billboard = new BillboardAsset();
        billboard.width = billboardWidth;
        billboard.height = billboardHeight;
        billboard.bottom = 0.0f;

        // 2. Define geometry and UV layouts
        Vector2[] vertices = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        ushort[] indices = { 0, 1, 2, 1, 3, 2 };
        Vector4[] texCoords = { new Vector4(0, 0, 1, 1) };

        billboard.SetVertices(vertices);
        billboard.SetIndices(indices);
        billboard.SetImageTexCoords(texCoords);
        billboard.material = uniqueMaterial;

        // 3. Define your target save path within the Assets directory
        string assetPath = string.Format("Assets/Billboards/{0}/GeneratedBillboard.asset", FolderName);

        // 4. Generate a unique path if a file already exists to prevent accidental overwrites
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        // 5. Create and write the physical file to disk
        AssetDatabase.CreateAsset(billboard, assetPath);

        // 6. Save changes and refresh the AssetDatabase to make it visible in the Project window
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully created billboard asset at: {assetPath}");
        
        #else
        Debug.LogWarning("Asset creation skipped. AssetDatabase cannot be used outside the Unity Editor.");
        #endif
    }
}