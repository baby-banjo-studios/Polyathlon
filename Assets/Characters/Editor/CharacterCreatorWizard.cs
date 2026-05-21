using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CharacterCreatorWizard :  ScriptableWizard
{
    private const string characterFolder = "Assets/Characters";
    private const string previewFolder = "Assets/Characters/Previews";
    [Header("Wizard Settings")]
    public GameObject characterModel;
    public string characterName;
    public RagdollProfile ragdollProfile;


    [MenuItem("Polyathlon/Character Creator")]
    private static void MenuEntryCall()
    {
        DisplayWizard<CharacterCreatorWizard>("Create character", "Create"); 
    }

    private void OnWizardUpdate()
    {
        
    }

    private void OnWizardCreate()
    {
        if (characterModel == null)
        {
            Debug.LogError("Model cannot be null");
            return;
        }
        if (characterName == null)
        {
            characterName = characterModel.name;
        }
        // else
        // {
        //     string modelPath = AssetDatabase.GetAssetPath(characterModel);
        //     GameObject previewPrefab = AssetDatabase.LoadAssetAtPathPath<GameObject>(modelPath);
        // }
        // create preview prefab
        string previewPath = Path.Combine(previewFolder, characterName);
        GameObject previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(characterModel);
        previewInstance.name = characterName;

        // create ragdoll
        if (!CreateRagdoll(previewInstance))
        {
            Debug.LogError("Failed to create ragdoll");
            return;
        }

        // GameObject previewPrefab = PrefabUtility.SaveAsPrefabAsset(previewInstance, previewPath);
        // if (previewPrefab != null)
        // {
        //     Debug.Log(String.Format("Preview variant created: {0}", previewPath));
        // }
    }

    private bool CreateRagdoll(GameObject model)
    {
        Undo.RegisterCompleteObjectUndo(model, "Generate Ragdoll");
        RagdollUtility.CreateRagdoll(model, ragdollProfile);

        return true;
    }

    private void CreatePreview()
    {
        
    }
}
