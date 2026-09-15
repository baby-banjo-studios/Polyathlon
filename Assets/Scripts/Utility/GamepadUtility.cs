using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadUtility
{
    public static string GetButtonFromInput(InputAction action, string scheme)
    {
        //foreach (InputBinding binding in action.bindings)
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            bool isBindingMatchingScheme = InputBinding.MaskByGroup(scheme).Matches(binding);
            if (isBindingMatchingScheme && !binding.isComposite && !binding.isPartOfComposite)
            {
                string s = action.GetBindingDisplayString();
                if (s == "Delta")
                {
                    s = "Mouse";
                }
                return GetFirstBindingInDisplayString(s);
            }
            
            if (i < action.bindings.Count - 1 && binding.isComposite)
            {
                InputBinding nextBinding = action.bindings[i + 1];
                bool isNextBindingMatchingScheme = InputBinding.MaskByGroup(scheme).Matches(nextBinding);
                if (isNextBindingMatchingScheme)
                {
                    //return binding.name;
                    // alternatively use this for built-in composite string constructor:
                    return GetFirstBindingInDisplayString(action.GetBindingDisplayString(i));
                }
            }
        }
        return string.Empty;
    }

    public static string GetFirstBindingInDisplayString(string multiBindingStr)
    {
        string[] multiBindingToks = multiBindingStr.Split('|');
        if (multiBindingToks.Length > 0)
        {
            return multiBindingToks[0].Trim();
        }
        return string.Empty;
    }
}