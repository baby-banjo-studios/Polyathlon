using UnityEngine;
using System.Collections.Generic;

public class CharSelectPreview : MonoBehaviour
{
    [SerializeField]
    private CharacterList characterList;

    private Dictionary<CharacterRegistry, GameObject> modelLookup;

    private void Awake()
    {
        modelLookup = new Dictionary<CharacterRegistry, GameObject>();

        foreach (CharacterRegistry registry in characterList.GetCharacters())
        {
            GameObject model = Instantiate(registry.previewObj, transform.position, transform.rotation, transform);
            modelLookup[registry] = model;
            model.SetActive(false);
        }
    }

    public void SelectCharacter(CharacterRegistry choice)
    {
        foreach (GameObject unselectedModel in modelLookup.Values)
        {
            unselectedModel.SetActive(false);
        }
        if (modelLookup.TryGetValue(choice, out GameObject selectedModel))
        {
            selectedModel.SetActive(true);
        }
    }
}