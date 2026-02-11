using UnityEngine;
using UnityEngine.UI;

public class UIUtility
{
    public static Rect GetVisibleRegion(ScrollRect scrollRect)
    {        
        Rect contentRect = scrollRect.content.rect;
        Rect viewportRect = scrollRect.viewport.rect;

        float xOffset = contentRect.width * scrollRect.horizontalNormalizedPosition;
        float yOffset = (contentRect.height - viewportRect.height) * -(1 - scrollRect.verticalNormalizedPosition);
        return new Rect(xOffset, yOffset - viewportRect.size.y, viewportRect.size.x, viewportRect.size.y);
    }

    public static float GetNormalizedScrollAmountForYValToBeVisible(float yVal, bool isTop, ScrollRect scrollRect)
    {
        Rect contentRect = scrollRect.content.rect;
        Rect viewportRect = scrollRect.viewport.rect;
        float scrollableHeight = contentRect.height - viewportRect.height;

        if (isTop)
        {
            float normalizedPosition = 1 - (-yVal / scrollableHeight);
            return normalizedPosition;
        }
        else
        {
            float normalizedPosition = 1 - ((-yVal - viewportRect.height) / scrollableHeight);
            return normalizedPosition;
        }
    }
}