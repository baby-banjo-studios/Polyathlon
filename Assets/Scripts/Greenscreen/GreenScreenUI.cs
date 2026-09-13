using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BabyBanjo.Core.Greenscreen
{
    public class GreenScreenUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI modeButtonText, targetButtonText;
        [SerializeField]
        private Slider animationFrameSlider;
        [SerializeField]
        private TMP_Dropdown animationList;
        [SerializeField]
        private GreenScreenAnimationController animationController;
        [SerializeField]
        private TMP_InputField widthInput, heightInput;
        [SerializeField]
        private int defaultWidth = 256, defaultHeight = 256;
        [SerializeField]
        private RectTransform leftBorder, rightBorder, topBorder, bottomBorder;

        private List<AnimationClip> animationLookup;
        [SerializeField]
        private GreenscreenController controller;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            animationLookup = new List<AnimationClip>();
            widthInput.text = defaultWidth.ToString();
            heightInput.text = defaultHeight.ToString();
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            controller.ImageHeight = defaultHeight;
            controller.ImageWidth = defaultWidth;
        }

        public void SetMode(ControlMode newMode)
        {
            modeButtonText.text = string.Format("Controls: {0}", newMode);
        }

        public void SetTargetMode(TargetMode newMode)
        {
            targetButtonText.text = string.Format("Target: {0}", newMode);
        }

        public void LoadAnimations(IEnumerable<AnimationClip> clips)
        {
            animationList.ClearOptions();
            animationLookup.Clear();

            List<string> options = new List<string>();
            foreach (AnimationClip clip in clips)
            {
                options.Add(clip.name);
                animationLookup.Add(clip);
            }
            animationList.AddOptions(options);

            LoadAnimation(0);
        }

        public void LoadAnimation(int idx)
        {
            AnimationClip clip = animationLookup[idx];
            animationFrameSlider.maxValue = clip.frameRate * clip.length;
            animationController.LoadClip(clip);
            animationFrameSlider.value = 0;
        }


        public void OnSliderValueChanged()
        {
            int frame = (int)animationFrameSlider.value;
            animationController.SetFrame(frame);
        }

        public void Scrub(bool pos)
        {
            if (pos)
            {
                animationFrameSlider.value += 1;
            }
            else
            {
                animationFrameSlider.value -= 1;
            }
        }

        public void OnHeightWidthChanged()
        {
            if (int.TryParse(widthInput.text, out int width) && int.TryParse(heightInput.text, out int height))
            {
                controller.ImageHeight = height;
                controller.ImageWidth = width;
                float ratio = ((float)width) / height;

                //topBorder.position = new Vector3(0f, (Screen.height / 2f) - (topBorder.rect.width / 2f), 0f);
                //bottomBorder.position = new Vector3(0f, (-Screen.height / 2f) + (topBorder.rect.width / 2f), 0f);
                
                topBorder.anchoredPosition = new Vector2(0f, Screen.height / 2f); 
                topBorder.sizeDelta = new Vector2(Screen.height * ratio, 5f);
                bottomBorder.anchoredPosition = new Vector2(0f, -Screen.height / 2f);
                bottomBorder.sizeDelta = new Vector2(Screen.height * ratio, 5f);
                
                leftBorder.anchoredPosition = new Vector2(-ratio * Screen.height / 2f, 0f); 
                leftBorder.sizeDelta = new Vector2(5f, Screen.height); 
                rightBorder.anchoredPosition = new Vector2(ratio * Screen.height / 2f, 0f); 
                rightBorder.sizeDelta = new Vector2(5f, Screen.height); 
            }
        }
    }
}