using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Kiosk : MonoBehaviour
{
    [System.Serializable]
    public class KioskScreen
    {
        public string screenName = "Screen";
        [Tooltip("The top image component that fades out.")]
        public Image frontImage;
        [Tooltip("The bottom image component that reveals the next slide.")]
        public Image backImage;
        [Tooltip("Slides specific to this screen. If left empty, uses shared fallback slides.")]
        public Sprite[] customSlides;

        [HideInInspector] public int currentIndex = 0;
        [HideInInspector] public Sprite[] activeSlides;
    }

    public enum DisplayMode
    {
        Synchronized,    // Both sides switch slides at the exact same moment
        Alternating,     // Side B switches halfway through Side A's display time (offset)
        Independent      // Both sides run on completely independent timers
    }

    [Header("Screen References")]
    [SerializeField]
    private KioskScreen sideA = new KioskScreen { screenName = "Side A (Front)" };
    [SerializeField]
    private KioskScreen sideB = new KioskScreen { screenName = "Side B (Back)" };

    [Header("Shared Fallback Slides")]
    [Tooltip("Slides used if a screen does not have its own custom slides array populated.")]
    [SerializeField] private Sprite[] sharedSlides;

    [Header("Timing Settings")]
    [SerializeField]
    private DisplayMode displayMode = DisplayMode.Synchronized;
    [SerializeField]
    private float displayDuration = 5.0f;
    [SerializeField]
    private float fadeDuration = 1.0f;

    private Coroutine coroutineSideA;
    private Coroutine coroutineSideB;

    private void Start()
    {
        if (!InitializeScreen(sideA) || !InitializeScreen(sideB))
        {
            Debug.LogError("DualSideKioskSlideshow: Failed to initialize one or both kiosk screens.");
            return;
        }

        StartSlideshows();
    }

    private bool InitializeScreen(KioskScreen screen)
    {
        if (screen.frontImage == null || screen.backImage == null)
        {
            Debug.LogError($"DualSideKioskSlideshow: Front or Back image missing on {screen.screenName}.");
            return false;
        }

        // Determine active slide deck (custom or shared fallback)
        if (screen.customSlides != null && screen.customSlides.Length > 0)
        {
            screen.activeSlides = screen.customSlides;
        }
        else
        {
            screen.activeSlides = sharedSlides;
        }

        if (screen.activeSlides == null || screen.activeSlides.Length == 0)
        {
            Debug.LogError($"DualSideKioskSlideshow: No slides assigned for {screen.screenName}.");
            return false;
        }

        // Set initial slide
        screen.currentIndex = 0;
        screen.frontImage.sprite = screen.activeSlides[0];
        screen.frontImage.color = SetAlpha(screen.frontImage.color, 1f);

        return true;
    }

    private void StartSlideshows()
    {
        switch (displayMode)
        {
            case DisplayMode.Synchronized:
                coroutineSideA = StartCoroutine(SynchronizedLoop());
                break;

            case DisplayMode.Alternating:
                coroutineSideA = StartCoroutine(SingleScreenLoop(sideA, 0f));
                // Delay Side B by half of the display duration for an alternating look
                coroutineSideB = StartCoroutine(SingleScreenLoop(sideB, displayDuration / 2f));
                break;

            case DisplayMode.Independent:
                coroutineSideA = StartCoroutine(SingleScreenLoop(sideA, 0f));
                coroutineSideB = StartCoroutine(SingleScreenLoop(sideB, 0f));
                break;
        }
    }

    // Handles both screens fading simultaneously
    private IEnumerator SynchronizedLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayDuration);

            int nextA = (sideA.currentIndex + 1) % sideA.activeSlides.Length;
            int nextB = (sideB.currentIndex + 1) % sideB.activeSlides.Length;

            // Prepare back images
            sideA.backImage.sprite = sideA.activeSlides[nextA];
            sideA.backImage.color = SetAlpha(sideA.backImage.color, 1f);

            sideB.backImage.sprite = sideB.activeSlides[nextB];
            sideB.backImage.color = SetAlpha(sideB.backImage.color, 1f);

            // Cross-fade both screens at the same time
            float elapsed = 0f;
            Color initA = sideA.frontImage.color;
            Color initB = sideB.frontImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

                sideA.frontImage.color = SetAlpha(initA, alpha);
                sideB.frontImage.color = SetAlpha(initB, alpha);

                yield return null;
            }

            // Swap front images and reset alpha
            sideA.frontImage.sprite = sideA.activeSlides[nextA];
            sideA.frontImage.color = SetAlpha(initA, 1f);
            sideA.currentIndex = nextA;

            sideB.frontImage.sprite = sideB.activeSlides[nextB];
            sideB.frontImage.color = SetAlpha(initB, 1f);
            sideB.currentIndex = nextB;
        }
    }

    // Handles individual screen rotation loop
    private IEnumerator SingleScreenLoop(KioskScreen screen, float initialDelay)
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        while (true)
        {
            yield return new WaitForSeconds(displayDuration);

            if (screen.activeSlides.Length <= 1) continue;

            int nextIndex = (screen.currentIndex + 1) % screen.activeSlides.Length;

            screen.backImage.sprite = screen.activeSlides[nextIndex];
            screen.backImage.color = SetAlpha(screen.backImage.color, 1f);

            float elapsed = 0f;
            Color initColor = screen.frontImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                screen.frontImage.color = SetAlpha(initColor, alpha);
                yield return null;
            }

            screen.frontImage.sprite = screen.activeSlides[nextIndex];
            screen.frontImage.color = SetAlpha(initColor, 1f);
            screen.currentIndex = nextIndex;
        }
    }

    private Color SetAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void OnDisable()
    {
        if (coroutineSideA != null) StopCoroutine(coroutineSideA);
        if (coroutineSideB != null) StopCoroutine(coroutineSideB);
    }
}