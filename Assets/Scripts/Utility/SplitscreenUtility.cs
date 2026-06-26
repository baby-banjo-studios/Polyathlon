using UnityEngine;
using UnityEngine.UI;
public static class SplitscreenUtility
{
    public static void ScaleTransform(RectTransform transformToScale, int playerIndex, int maxPlayers, Vector2 maxScreenSize)
    {
        if (maxPlayers == 1)
        {
            transformToScale.anchorMax = new Vector2(0.5f, 0.5f);
            transformToScale.anchorMin = new Vector2(0.5f, 0.5f);
            transformToScale.anchoredPosition = new Vector3(0, 0, 0);
            transformToScale.localScale = new Vector3(1, 1, 1);
            transformToScale.sizeDelta = new Vector2(maxScreenSize.x, maxScreenSize.y);
        }
        else if (maxPlayers < 3)
        {
            switch (playerIndex)
            {
                case 0:
                    transformToScale.pivot = new Vector2(0, 0.5f);
                    transformToScale.anchorMax = new Vector2(0, 0.5f);
                    transformToScale.anchorMin = new Vector2(0, 0.5f);
                    break;
                case 1:
                    transformToScale.pivot = new Vector2(1, 0.5f);
                    transformToScale.anchorMax = new Vector2(1, 0.5f);
                    transformToScale.anchorMin = new Vector2(1, 0.5f);
                    break;
            }
            transformToScale.anchoredPosition = new Vector3(0, 0, 0);
            transformToScale.localScale = new Vector3(1, 1, 1);
            transformToScale.sizeDelta = new Vector2(maxScreenSize.x / 2, maxScreenSize.y);
            //scaleTransform.localScale = new Vector3(0.5f, 1, 1);

        }
        else
        {
            switch (playerIndex)
            {
                case 0:
                    transformToScale.pivot = new Vector2(0, 1);
                    transformToScale.anchorMax = new Vector2(0, 1);
                    transformToScale.anchorMin = new Vector2(0, 1);
                    break;
                case 1:
                    transformToScale.pivot = new Vector2(1, 1);
                    transformToScale.anchorMax = new Vector2(1, 1);
                    transformToScale.anchorMin = new Vector2(1, 1);
                    break;
                case 2:
                    transformToScale.pivot = new Vector2(0, 0);
                    transformToScale.anchorMax = new Vector2(0, 0);
                    transformToScale.anchorMin = new Vector2(0, 0);
                    break;
                case 3:
                    transformToScale.pivot = new Vector2(1, 0);
                    transformToScale.anchorMax = new Vector2(1, 0);
                    transformToScale.anchorMin = new Vector2(1, 0);
                    break;

            }
            transformToScale.anchoredPosition = new Vector3(0, 0, 0);
            transformToScale.localScale = new Vector3(0.5f, 0.5f, 1);
            transformToScale.sizeDelta = new Vector2(maxScreenSize.x, maxScreenSize.y);
            //scaleTransform.sizeDelta = new Vector2(scaleTransform.sizeDelta.x / 2, scaleTransform.sizeDelta.y / 2);
        }
    }
}