using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Linq;

public class CharacterCreatorWizard :  ScriptableWizard
{
    private const string characterFolder = "Assets/Characters";
    private const string previewFolder = characterFolder + "/Previews";
    private const string playerFolder = characterFolder + "/Players";
    private const string playerBasePath = characterFolder + "/PlayerBase.prefab";
    private const string npcFolder = characterFolder + "/NPCs";
    private const string npcBasePath = characterFolder + "/NPCBase.prefab";
    private const string ragdollProfileDefaultPath = characterFolder + "/RagdollProfiles/Mixamo.asset";
    private const string characterListPath = characterFolder + "/CharacterList.asset";
    private const string RegistriesFolderPath = characterFolder + "/Registries";
    private const string audioMixerPath = "Assets/Audio/Mixer.mixer";
    private const string animatorPath = "Assets/Animation/Standard.controller";
    private const string audioMixerGroupName = "Footsteps";
    private const string backpackMountPath = characterFolder + "/BackpackMount.prefab";
    private const string backpackMountParentTransform = "mixamorig:Spine1";
    private static readonly Vector3 backpackMountDefaultPosition = new Vector3(0f, 0.117f, -0.129f);
    private static readonly Vector3 backpackMountDefaultRotation = new Vector3(7f, 0f, 0f);
    private const string itemDropPointName = "ItemDropPoint";
    private static readonly Vector3 itemDropPointDefaultPosition = new Vector3(0f, 0.5f, -1f);
    private const string hipsTransformName = "mixamorig:Hips";

    
    [Header("Wizard Settings")]
    public GameObject characterModel;
    public string characterName;
    public RagdollProfile ragdollProfile;
    public bool previewOnly;
    public bool addToRoster;


    [MenuItem("Polyathlon/Character Creator")]
    private static void MenuEntryCall()
    {
        DisplayWizard<CharacterCreatorWizard>("Create character", "Create");
    }

    private void OnWizardUpdate()
    {
        if (ragdollProfile == null)
        {
            ragdollProfile = AssetDatabase.LoadAssetAtPath<RagdollProfile>(ragdollProfileDefaultPath);
        }
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
        else
        {
            string modelPath = AssetDatabase.GetAssetPath(characterModel);
            GameObject previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        string previewPath = previewFolder + "/" + characterName + "Preview.prefab";
        string playerPath = playerFolder + "/" + characterName + ".prefab";
        string npcPath = npcFolder + "/" + characterName + "NPC.prefab";

        if (!CreatePreview(previewPath))
        {
            return;
        }

        if (!previewOnly)
        {
            if (!CreatePlayer(playerPath, previewPath))
            {
                return;
            }
            if (!CreateNPC(npcPath, previewPath))
            {
                return;
            }
            
            if (addToRoster)
            {
                if (!AddToRoster(previewPath, playerPath, npcPath))
                {
                    return;
                }
            }
        }
    }

    private bool CreatePreview(string previewPath)
    {
        GameObject previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(characterModel);
        previewInstance.name = characterName + "Preview";

        if (!previewInstance.TryGetComponent(out Animator animator))
        {
            animator = previewInstance.AddComponent<Animator>();
        }
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorPath);
        animator.runtimeAnimatorController = controller;

        // create ragdoll
        if (!CreateRagdoll(previewInstance))
        {
            Debug.LogError("Failed to create ragdoll");
            DestroyImmediate(previewInstance);
            return false;
        }
        
        AudioSource audioSource = previewInstance.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(audioMixerPath);
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(audioMixerGroupName);
        if (groups.Length < 0)
        {
            Debug.LogError(String.Format("Failed to find mixer group \"{0}\"", audioMixerGroupName));
            DestroyImmediate(previewInstance);
            return false;
        }
        else
        {
            audioSource.outputAudioMixerGroup = groups[0];
        }

        Ragdoll ragdoll = previewInstance.AddComponent<Ragdoll>();
        PlayerAnimationEvents playerAnimationEvents = previewInstance.AddComponent<PlayerAnimationEvents>();

        GameObject backpackMountPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(backpackMountPath);
        Transform backpackMountParent = previewInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == backpackMountParentTransform);
        if (backpackMountParent != null)
        {
            GameObject backpackMountInstance = (GameObject)PrefabUtility.InstantiatePrefab(backpackMountPrefab, backpackMountParent);
            backpackMountInstance.transform.localPosition = backpackMountDefaultPosition;
            backpackMountInstance.transform.localEulerAngles = backpackMountDefaultRotation;
        }
        else
        {
            Debug.LogError(String.Format("Failed to find transform \"{0}\"", backpackMountParentTransform));
            DestroyImmediate(previewInstance);
            return false;
        }

        GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(previewInstance, previewPath);
        if (newPrefab == null)
        {   
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", previewPath));
            DestroyImmediate(previewInstance);
            return false;
        }
        else
        {
            Debug.Log(String.Format("Preview variant created: {0}", previewPath));
        }

        DestroyImmediate(previewInstance);
        AssetDatabase.Refresh();
        return true;
    }

    private bool CreateRagdoll(GameObject model)
    {
        Undo.RegisterCompleteObjectUndo(model, "Generate Ragdoll");
        RagdollUtility.CreateRagdoll(model, ragdollProfile);

        return true;
    }

    private bool CreatePlayer(string playerPath, string previewPath)
    {
        bool success = CreateRacer(playerBasePath, previewPath, playerPath, "");
        if (success)
        {
            Debug.Log(String.Format("Player variant created: {0}", playerPath));
        }
        else
        {
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", playerPath));
        }

        return success;
    }

    private bool CreateNPC(string npcPath, string previewPath)
    {        
        bool success = CreateRacer(npcBasePath, previewPath, npcPath, "NPC");
        if (success)
        {
            Debug.Log(String.Format("NPC variant created: {0}", npcPath));
        }
        else
        {
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", npcPath));
        }

        return success;
    }
    
    private bool CreateRacer(string racerBasePath, string previewPath, string racerSavePath, string prefabSuffix)
    {
        GameObject racerBasePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(racerBasePath);
        GameObject racerInstance = (GameObject)PrefabUtility.InstantiatePrefab(racerBasePrefab);
        racerInstance.name = characterName + prefabSuffix;

        GameObject previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(previewPath);
        GameObject previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(previewPrefab, racerInstance.transform);
        previewInstance.transform.SetSiblingIndex(0);
        
        if (racerInstance.TryGetComponent(out Racer racer))
        {
            racer.characterMesh = previewInstance.transform;
            racer.hips = previewInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == hipsTransformName);
        }
        else
        {
            Debug.Log(String.Format("{0} does not have a Racer component", racerBasePath));
            DestroyImmediate(racerInstance);
            return false;
        }

        GameObject itemDropPoint = new GameObject(itemDropPointName);
        itemDropPoint.transform.parent = previewInstance.transform;
        itemDropPoint.transform.localPosition = itemDropPointDefaultPosition;

        Movement[] movements1 = racerInstance.GetComponents<Movement>();
        for (int i = 0; i < movements1.Length; i++)
        {
            movements1[i].itemDropPoint = itemDropPoint.transform;
        }

        GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(racerInstance, racerSavePath);
        if (newPrefab == null)
        {   
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", racerSavePath));
            DestroyImmediate(racerInstance);
            return false;
        }

        DestroyImmediate(racerInstance);
        AssetDatabase.Refresh();
        return true;
    }

    private bool AddToRoster(string previewPath, string playerPath, string npcPath)
    {
        string registryPath = RegistriesFolderPath + "/" + characterName + ".asset";
        CharacterRegistry registryInstance = CreateInstance<CharacterRegistry>();
        registryInstance.name = characterName;
        registryInstance.displayName = characterName;
        
        registryInstance.previewObj = AssetDatabase.LoadAssetAtPath<GameObject>(previewPath);
        registryInstance.playerObj = AssetDatabase.LoadAssetAtPath<GameObject>(playerPath);
        registryInstance.npcObj = AssetDatabase.LoadAssetAtPath<GameObject>(npcPath);

        try
        {
            AssetDatabase.CreateAsset(registryInstance, registryPath);
        }
        catch (Exception e)
        {
            Debug.LogError(String.Format("Failed to create registry \"{0}\" : {1}", registryPath, e.ToString()));
            return false;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CharacterList characterList = AssetDatabase.LoadAssetAtPath<CharacterList>(characterListPath);
        if (characterList == null)
        {
            Debug.LogError(String.Format("Failed to get CharacterList at {0}", characterListPath));
            return false;
        }

        CharacterRegistry registryAsset = AssetDatabase.LoadAssetAtPath<CharacterRegistry>(registryPath);
        if (registryAsset == null)
        {
            Debug.LogError(String.Format("Failed to get CharacterRegistry at {0}", registryPath));
            return false;
        }
        
        characterList.AddCharacter(registryAsset);
        EditorUtility.SetDirty(characterList);
        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();

        return true;
    }
}
