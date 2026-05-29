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
        else
        {
            string modelPath = AssetDatabase.GetAssetPath(characterModel);
            GameObject previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }
        // create preview prefab
        string previewPath = previewFolder + "/" + characterName + "Preview.prefab";
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
            return;
        }
        
        AudioSource audioSource = previewInstance.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(audioMixerPath);
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(audioMixerGroupName);
        if (groups.Length < 0)
        {
            Debug.LogError(String.Format("Failed to find mixer group \"{0}\"", audioMixerGroupName));
            return;
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
            return;
        }

        GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(previewInstance, previewPath);
        if (newPrefab == null)
        {   
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", previewPath));
            return;
        }
        else
        {
            Debug.Log(String.Format("Preview variant created: {0}", previewPath));
        }
        
        AssetDatabase.Refresh();

        // create Player
        string playerPath = playerFolder + "/" + characterName + ".prefab";
        GameObject playerBasePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerBasePath);
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerBasePrefab);
        playerInstance.name = characterName;
        GameObject previewPrefab1 = AssetDatabase.LoadAssetAtPath<GameObject>(previewPath);
        GameObject previewInstance1 = (GameObject)PrefabUtility.InstantiatePrefab(previewPrefab1, playerInstance.transform);
        previewInstance1.transform.SetSiblingIndex(0);

        GameObject itemDropPoint = new GameObject(itemDropPointName);
        itemDropPoint.transform.parent = previewInstance1.transform;
        itemDropPoint.transform.localPosition = itemDropPointDefaultPosition;
        
        if (playerInstance.TryGetComponent(out Racer playerController))
        {
            playerController.characterMesh = previewInstance1.transform;
            playerController.hips = playerInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == hipsTransformName);
        }
        else
        {
            Debug.Log(String.Format("{0} does not have an NPC component", npcBasePath));
            return;
        }

        Movement[] movements = playerInstance.GetComponents<Movement>();
        for (int i = 0; i < movements.Length; i++)
        {
            movements[i].itemDropPoint = itemDropPoint.transform;
        }

        newPrefab = PrefabUtility.SaveAsPrefabAsset(playerInstance, playerPath);
        if (newPrefab == null)
        {   
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", playerPath));
            return;
        }
        else
        {
            Debug.Log(String.Format("Player variant created: {0}", playerPath));
        }

        // create NPC
        string npcPath = npcFolder + "/" + characterName + "NPC.prefab";
        GameObject npcBasePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(npcBasePath);
        GameObject npcInstance = (GameObject)PrefabUtility.InstantiatePrefab(npcBasePrefab);
        npcInstance.name = characterName + "NPC";
        GameObject previewInstance2 = (GameObject)PrefabUtility.InstantiatePrefab(previewPrefab1, npcInstance.transform);
        previewInstance2.transform.SetSiblingIndex(0);
        
        if (npcInstance.TryGetComponent(out Racer npc))
        {
            npc.characterMesh = npcInstance.transform;
            npc.hips = npcInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == hipsTransformName);
        }
        else
        {
            Debug.Log(String.Format("{0} does not have an NPC component", npcBasePath));
            return;
        }

        Movement[] movements1 = npcInstance.GetComponents<Movement>();
        for (int i = 0; i < movements1.Length; i++)
        {
            movements1[i].itemDropPoint = itemDropPoint.transform;
        }

        newPrefab = PrefabUtility.SaveAsPrefabAsset(npcInstance, npcPath);
        if (newPrefab == null)
        {   
            Debug.LogError(String.Format("Failed to create prefab \"{0}\"", npcPath));
            return;
        }
        else
        {
            Debug.Log(String.Format("NPC variant created: {0}", npcPath));
        }

        // cleanup
        DestroyImmediate(previewInstance);
        DestroyImmediate(playerInstance);
        DestroyImmediate(npcInstance);
    }

    private bool CreatePreview()
    {
        
        return true;
    }

    private bool CreatePlayer()
    {
        
        return true;
    }
    
    private bool CreateNPC()
    {
        
        return true;
    }

    private bool Cleanup()
    {
        return true;
    }

    private bool CreateRagdoll(GameObject model)
    {
        Undo.RegisterCompleteObjectUndo(model, "Generate Ragdoll");
        RagdollUtility.CreateRagdoll(model, ragdollProfile);

        return true;
    }
}
