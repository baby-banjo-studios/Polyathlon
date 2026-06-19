using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterList", order = 1)]
public class CharacterList : ScriptableObject
{
    [SerializeField]
    private List<CharacterRegistry> characters;

    public CharacterRegistry[] GetCharacters()
    {
        return characters.ToArray();
    }

    public void AddCharacter(CharacterRegistry registry)
    {
        characters.Add(registry);
    }

}
