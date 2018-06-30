using UnityEngine;
using System.Collections;

public class ResponseButton : MonoBehaviour
{
    public TMPro.TextMeshProUGUI flavourText;

    [Header("Aesthetics")]
    [SerializeField] private float growthSpeedMouseEnter = 5;
    [SerializeField] private Vector3 growthMaxMouseEnter = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private float shrinkSpeedMouseExit = 10;

    private bool mouseOver = false;
    private bool lerpingDone;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOver)
        {
            if (!lerpingDone && flavourText.rectTransform.localScale.x > growthMaxMouseEnter.x * .98f)      // cap amount of lerping to reduce canvas redraws
            {
                flavourText.rectTransform.localScale = growthMaxMouseEnter;
                lerpingDone = true;
            }
            else
                flavourText.rectTransform.localScale = Vector3.Lerp(flavourText.rectTransform.localScale, growthMaxMouseEnter, Time.deltaTime * growthSpeedMouseEnter);
        }
        else
        {
            if (!lerpingDone && flavourText.rectTransform.localScale.x < 1.02f)      // cap amount of lerping to reduce canvas redraws
                flavourText.rectTransform.localScale = Vector3.one;
            flavourText.rectTransform.localScale = Vector3.Lerp(flavourText.rectTransform.localScale, Vector3.one, Time.deltaTime * shrinkSpeedMouseExit);
        }
    }

    public void OnEnter()
    {
        if (!mouseOver)
        {
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
        Debug.Log("CLick");
    }
}
