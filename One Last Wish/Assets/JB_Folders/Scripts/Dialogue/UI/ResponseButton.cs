using UnityEngine;
using System.Collections;

public class ResponseButton : MonoBehaviour
{
    public TMPro.TextMeshProUGUI flavourText;
    [HideInInspector]
    public int indexCurrentConvo;
    [HideInInspector]
    public string eventName;

    [Header("Aesthetics")]
    [SerializeField] private Color textColour;
    [SerializeField] private float fadeInLength = 1;
    [SerializeField] private float fadeOutLength = .5f;
    [SerializeField] private float growthSpeedMouseEnter = 5;
    [SerializeField] private Vector3 growthMaxMouseEnter = new Vector3(2, 2, 2);
    [SerializeField] private float shrinkSpeedMouseExit = 10;

    private bool mouseOver = false;
    private bool lerpingDone = false;
    private bool fadeIn = false;
    private float fadeTime = 0;
    private float fadeProgress = 0;
    private bool fadingDone = true;

    void Update()
    {
        // fade in or out
        FadeManager();
        // lerp text on mouse input
        TextLerpManager();
    }

    #region Mouse Input

    public void OnEnter()
    {
        if (!mouseOver)
        {
            ConversationManager.cm.PlaySound("responseMouseEnter");
            lerpingDone = false;
            mouseOver = true;
        }
    }

    public void OnExit()
    {
        if (mouseOver)
        {
            lerpingDone = false;
            mouseOver = false;
        }
    }

    public void OnClick()
    {
        ConversationManager.cm.LoadSubtitles(eventName, indexCurrentConvo);
    }

    #endregion

    #region Fading

    public void FadeManager()
    {
        if (!fadingDone)
        {
            fadeTime += Time.deltaTime;
            if (fadeIn)
            {
                fadeProgress = fadeTime / fadeInLength;
                flavourText.color = Color.Lerp(Color.clear, textColour, fadeProgress);
            }
            else
            {
                fadeProgress = fadeTime / fadeOutLength;
                flavourText.color = Color.Lerp(textColour, Color.clear, fadeProgress);
            }

            if (fadeProgress >= 1)
            {
                fadingDone = true;
            }
        }
    }

    public void CallFadeIn(float _delay)
    {
        Invoke("StartFadeIn", _delay);
    }

    void StartFadeIn()
    {
        fadeIn = true;
        fadingDone = false;
        fadeTime = 0;
        fadeProgress = 0;
    }

    public void CallFadeOut(float _delay)
    {
        Invoke("StartFadeOut", _delay);
    }

    void StartFadeOut()
    {
        fadeIn = false;
        fadingDone = false;
        fadeTime = 0;
        fadeProgress = 0;
    }

    #endregion

    #region Text Lerp

    void TextLerpManager()
    {
        if (mouseOver)
        {
            if (!lerpingDone && flavourText.rectTransform.localScale.x > growthMaxMouseEnter.x * .999f)      // cap amount of lerping to reduce canvas redraws
            {
                flavourText.rectTransform.localScale = growthMaxMouseEnter;
                lerpingDone = true;
            }
            else
                flavourText.rectTransform.localScale = Vector3.Lerp(flavourText.rectTransform.localScale, growthMaxMouseEnter, Time.deltaTime * growthSpeedMouseEnter);
        }
        else
        {
            if (!lerpingDone && flavourText.rectTransform.localScale.x < 1.001f)      // cap amount of lerping to reduce canvas redraws
                flavourText.rectTransform.localScale = Vector3.one;
            flavourText.rectTransform.localScale = Vector3.Lerp(flavourText.rectTransform.localScale, Vector3.one, Time.deltaTime * shrinkSpeedMouseExit);
        }
    }

    #endregion
}
