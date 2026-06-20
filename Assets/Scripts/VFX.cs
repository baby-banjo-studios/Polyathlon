using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VFX : MonoBehaviour
{
    [SerializeField] private RectTransform screenOverlayCanvas;
    [SerializeField] private RectTransform overlayScaleTransform;
    [SerializeField] private RectTransform unscaledOverlayCanvas;
    [SerializeField] private RectTransform cameraSpaceCanvas;
    [SerializeField] private Camera playerCam;
    public GameObject speedLinesObj;
    public Image damageImage;
    public float damageFadeTime = 1f;

    [SerializeField] private GameObject targetLockObj;
    private Transform target;
    private bool targeting;

    private void Start()
    {
        SetSpeedLines(false);   
        SetTarget(null);
    }

    private void Update()
    {
        if (target != null && targeting)
        {
            Vector3 viewportPos = playerCam.WorldToViewportPoint(target.position);
            // if viewportPos.z is negative, then the target is not visible in the frame of the camera
            if (viewportPos.z >= 0)
            {
                targetLockObj.SetActive(true);
                float canvasX = playerCam.rect.x + viewportPos.x * playerCam.rect.width;
                float canvasY = playerCam.rect.y + viewportPos.y * playerCam.rect.height;
                Vector3 finalPosition = new Vector3(canvasX * unscaledOverlayCanvas.sizeDelta.x, canvasY * unscaledOverlayCanvas.sizeDelta.y, 0);
                targetLockObj.transform.position = finalPosition;
            }
            else
            {
                targetLockObj.SetActive(false);
            }
        }
    }

    public void SetScale(RectTransform referenceRectTransform)
    {
        overlayScaleTransform.pivot = referenceRectTransform.pivot;
        overlayScaleTransform.anchorMax = referenceRectTransform.anchorMax;
        overlayScaleTransform.anchorMin = referenceRectTransform.anchorMin;
        overlayScaleTransform.anchoredPosition = referenceRectTransform.anchoredPosition;
        overlayScaleTransform.localScale = referenceRectTransform.localScale;
        overlayScaleTransform.sizeDelta = referenceRectTransform.sizeDelta;
    }

    public void SetSpeedLines(bool enable)
    {
        speedLinesObj.SetActive(enable);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        if (target == null)
        {
            targeting = false;
            targetLockObj.SetActive(false);
        }   
        else
        {
            targeting = true;
            targetLockObj.SetActive(true);
        }
    }

    public void ShowDamage()
    {
        StartCoroutine(ShowDamageCoroutine());
    } 

    private IEnumerator ShowDamageCoroutine()
    {
        Color opaque = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 1f);
        Color transparent = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0f);

        float elapsedTime = 0f;
        while (elapsedTime < damageFadeTime)
        {
            elapsedTime += Time.deltaTime;
            damageImage.color = Color.Lerp(opaque, transparent, elapsedTime / damageFadeTime);
            yield return null;
        }
    }
}