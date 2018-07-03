using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager cm;

    [SerializeField] private JB_DialogueStreamer dialogueStreamer;
    [SerializeField] private RectTransform responseContent;
    [SerializeField] private TMPro.TextMeshProUGUI subtitleText;

    // Conversation variables
    public int emotionRange = 50;
    public int maxResponseOptions = 4;
    bool inConversation;
    bool conversationHalt;
    string currentEventName;
    int currentElementCluster;
    int currentElementIndex;
    int currentSubtitleSegment;
    List<int> possibleElements = new List<int>();
    List<ResponseButton> loadedResponses = new List<ResponseButton>();
    //[HideInInspector]
    List<string> activeMarkers = new List<string>();

    // UI
    [Header("UI")]
    [SerializeField] ResponseButton flavourTextPrefab;
    [SerializeField] float delayBetweenResponseFades = .3f;

    [Header("Subtitles")]
    [SerializeField] private Color subtitleColour;
    [SerializeField] private float subtitleFadeLength = 2;
    private float subtitleFadeProgress;
    private float subtitleFadeTime;
    private bool firstSubtitleReached;

    private void Awake()
    {
        if (cm)
        {
            Destroy(gameObject);
        }
        else
        {
            cm = this;
        }
    }

    #region Conversation Loader

    public void StartConversation()
    {
        dialogueStreamer.SetupConversation();
        currentElementCluster = 1;
        currentElementIndex = 0;
        inConversation = true;
        LoadNextLine();
    }

    void LoadNextLine()
    {
        // get possible elements to display/play
        possibleElements.Clear();

        // ASSUMES ELEMENTS WITH SAME ELEMENT NUMBER ARE ALL CONTIGUOUS IN THE CSV
        bool foundCluster = false;
        for (int i = 0; i < dialogueStreamer.currentConvo.Count; i++)
        {
            if (dialogueStreamer.currentConvo[i].elementNumber == currentElementCluster)
            {
                if (!foundCluster) foundCluster = true;

                // is within the emotion range add it to possible elements list
                if (emotionRange >= dialogueStreamer.currentConvo[i].minEmotionRange
                    && emotionRange <= dialogueStreamer.currentConvo[i].maxEmotionRange)
                {
                    possibleElements.Add(i);
                }
                else Debug.LogError("Element in cluster  " + currentElementCluster + " needs flavour text or is incorrectly numbered, ya dingleberry");

            }
            else if (foundCluster) break;   // once no longer in cluster can stop searching
        }

        if (possibleElements.Count > 0)
        {
            // if next element is a cap response element load responses else load subtitles
            if (dialogueStreamer.currentConvo[possibleElements[0]].lineType == Personae.cap
                 && dialogueStreamer.currentConvo[possibleElements[0]].firstLineType == ElementType.response)
            {
                // load response options
                LoadCurrentResponseCluster();
            }
            else
            {
                // load subtitles
                LoadCurrentSubtitleCluster();
            }
        }
        else
        {
            // TODO end of conversation
            Debug.Log("End of conversation");
        }
    }

    void LoadCurrentResponseCluster()
    {
        // if previous response options still in hierarchy, delete
        if (loadedResponses.Count > 0)
        {
            for (int i = loadedResponses.Count - 1; i >= 0; i--)
            {
                Destroy(loadedResponses[i].gameObject);
                loadedResponses.RemoveAt(i);
            }
        }

        // if some response options were found
        if (possibleElements.Count > 0)
        {
            // if there are more possible elements reduce them
            if (possibleElements.Count > maxResponseOptions)
            {
                ReducePossibleElementsToMaxAllowed();
            }

            float fadeDelay = 0;
            // display all the response options
            for (int i = 0; i < possibleElements.Count; i++)
            {
                ResponseButton _res = Instantiate(flavourTextPrefab, responseContent);
                _res.flavourText.text = dialogueStreamer.currentConvo[possibleElements[i]].flavourText;
                _res.indexCurrentConvo = possibleElements[i];
                _res.eventName = dialogueStreamer.currentConvo[possibleElements[i]].eventName;
                _res.CallFadeIn(fadeDelay);
                fadeDelay += delayBetweenResponseFades;
                loadedResponses.Add(_res);
            }
        }

        responseContent.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(false);
    }

    void LoadCurrentSubtitleCluster()
    {
        // if some subtitle options were found
        if (possibleElements.Count > 0)
        {
            // randomly remove until only one is left
            while (possibleElements.Count > 1)
            {
                possibleElements.RemoveAt(Random.Range(0, possibleElements.Count));
            }
            currentElementIndex = possibleElements[0];
        }
        else
        {
            Debug.LogError("No possible elements found");
            return;
        }

        responseContent.gameObject.SetActive(false);
        LoadSubtitles(dialogueStreamer.currentConvo[currentElementIndex].eventName, currentElementIndex);
    }

    void ReducePossibleElementsToMaxAllowed()
    {
        int toReduce = possibleElements.Count - maxResponseOptions;

        // randomly reduce until left with the max amount of response options
        while (toReduce > 0)
        {
            possibleElements.RemoveAt(Random.Range(0, possibleElements.Count));
            toReduce--;
        }
    }

    void PrepNextElement()
    {
        // check if last element leads to another
        if (dialogueStreamer.currentConvo[currentElementIndex].leadsToElement == 0)
        {
            // TODO end of conversation
            Debug.Log("End of conversation");
        }
        else currentElementCluster = dialogueStreamer.currentConvo[currentElementIndex].leadsToElement;

        LoadNextLine();
    }

    #endregion

    public void PlaySound(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }

    #region Subtitles

    public void LoadSubtitles(string _eventName, int _elementIndex)
    {
        // get the element to load
        currentElementIndex = _elementIndex;
        currentEventName = _eventName;

        // prep UI elements
        responseContent.gameObject.SetActive(false);
        currentSubtitleSegment = 0;
        firstSubtitleReached = false;
        PlayVoiceLine(currentEventName);
    }

    void PlayVoiceLine(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject, (uint)AkCallbackType.AK_Marker | (uint)AkCallbackType.AK_EndOfEvent, VoiceCallback, null);
    }

    private void VoiceCallback(object in_cookie, AkCallbackType in_type, object in_callbackInfo)
    {
        switch (in_type)
        {
            case AkCallbackType.AK_Marker:
                // once beginning of event is reached, display first subtitle else show next segment
                if (!firstSubtitleReached)
                {
                    firstSubtitleReached = true;
                    subtitleText.text = dialogueStreamer.currentConvo[currentElementIndex].subtitleText[0];
                    subtitleText.gameObject.SetActive(true);
                    StopCoroutine("FadeSubtitles");
                    subtitleText.color = subtitleColour;
                }
                else
                {
                    // move to next subtitle segment
                    currentSubtitleSegment++;
                    if (currentSubtitleSegment < dialogueStreamer.currentConvo[currentElementIndex].subtitleText.Length)
                    {
                        subtitleText.text = dialogueStreamer.currentConvo[currentElementIndex].subtitleText[currentSubtitleSegment];
                    }
                }
                break;
            case AkCallbackType.AK_EndOfEvent:
                // fade out
                StartCoroutine("FadeSubtitles");
                PrepNextElement();
                break;
        }
    }

    IEnumerator FadeSubtitles()
    {
        subtitleFadeProgress = subtitleFadeTime = 0;

        while (subtitleFadeProgress < 1)
        {
            subtitleFadeTime += Time.deltaTime;
            subtitleFadeProgress = subtitleFadeTime / subtitleFadeLength;

            subtitleText.color = Color.Lerp(subtitleColour, Color.clear, subtitleFadeProgress);

            yield return null;
        }

        subtitleText.gameObject.SetActive(false);
    }
    #endregion
}
