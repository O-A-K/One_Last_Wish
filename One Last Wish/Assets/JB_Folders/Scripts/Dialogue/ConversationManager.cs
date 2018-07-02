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

    // AK
    [Header("Wwise stuff")]
    public AK.Wwise.CallbackFlags MyCallbackFlags = null;
    AkEventCallbackData m_callbackData;

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

    public void StartConversation()
    {
        dialogueStreamer.SetupConversation();
        currentElementCluster = 1;
        inConversation = true;
        LoadFirstLine();
        responseContent.gameObject.SetActive(true);
    }

    void LoadFirstLine()
    {
        // if previous response options in hierarchy, delete
        if (loadedResponses.Count > 0)
        {
            for (int i = loadedResponses.Count - 1; i >= 0; i--)
            {
                Destroy(loadedResponses[i].gameObject);
                loadedResponses.RemoveAt(i);
            }
        }

        // if a cap element check whether to load flavour text or subtitles, else load wife's subtitles
        if (dialogueStreamer.currentConvo[0].lineType == Personae.cap)
        {
            if (dialogueStreamer.currentConvo[0].firstLineType == ElementType.line)
            {
                // load subtitles

            }
            else
            {
                // load response options
                LoadCurrentResponseCluster();
            }
        }
        else
        {
            // load subtitles
        }
    }

    void LoadCurrentResponseCluster()
    {
        possibleElements.Clear();

        // ASSUMES ELEMENTS WITH SAME ELEMENT NUMBER ARE ALL CONTIGUOUS IN THE CSV
        bool foundCluster = false;
        for (int i = 0; i < dialogueStreamer.currentConvo.Count; i++)
        {
            if (dialogueStreamer.currentConvo[i].elementNumber == currentElementCluster)
            {
                if (!foundCluster) foundCluster = true;

                // if this element number matches the currentElementCluster and is cap's element 
                if (dialogueStreamer.currentConvo[i].lineType == Personae.cap)
                {
                    // and is within the emotion range add it to possible elements list
                    if (emotionRange >= dialogueStreamer.currentConvo[i].minEmotionRange
                        && emotionRange <= dialogueStreamer.currentConvo[i].maxEmotionRange)
                    {
                        possibleElements.Add(i);
                    }
                }
                else Debug.LogError("Element on line " + i + 1 + "needs flavour text or is incorrectly numbered, ya dingleberry");

            }
            else if (foundCluster) break;   // once no longer in cluster can stop searching
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
    }

    public void LoadSubtitles(string _eventName, int _elementIndex)
    {
        // get the element to load
        currentElementIndex = _elementIndex;
        currentEventName = _eventName;

        // prep UI elements
        responseContent.gameObject.SetActive(false);
        subtitleText.gameObject.SetActive(true);
        currentSubtitleSegment = 0;
        subtitleText.text = dialogueStreamer.currentConvo[currentElementIndex].subtitleText[0];
        PlayVoiceLine(currentEventName);
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

    public void PlaySound(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }

    void PlayVoiceLine(string eventName)
    {
        m_callbackData = new AkEventCallbackData();
        m_callbackData.callbackFunc.Add("Callback");
        m_callbackData.callbackGameObj.Add(gameObject);
        m_callbackData.uFlags = 1;
        m_callbackData.callbackFlags.Add(0);
        m_callbackData.callbackFlags.Add(1);
        m_callbackData.callbackFlags.Add(2);

        AkSoundEngine.PostEvent(eventName, gameObject, (uint)m_callbackData.uFlags, Callback, null);
    }

    void MarkerCallback(AkEventCallbackMsg callbackInfo)
    {
        Debug.Log("Marker callback successful");

        switch (callbackInfo.type)
        {
            case AkCallbackType.AK_Marker:
                //var MarkerCallbackInfo = callbackInfo.info as AkMarkerCallbackInfo;
                currentSubtitleSegment++;
                if (currentSubtitleSegment < dialogueStreamer.currentConvo[currentElementIndex].subtitleText.Length)
                {
                    subtitleText.text = dialogueStreamer.currentConvo[currentElementIndex].subtitleText[currentSubtitleSegment];
                }
                break;
            case AkCallbackType.AK_EndOfEvent:
                Debug.Log("End of subtitle reached");
                break;
        }
    }

    private void Callback(object in_cookie, AkCallbackType in_type, object in_callbackInfo)
    {
        Debug.Log("Marker callback successful");

        switch (in_type)
        {
            case AkCallbackType.AK_Marker:
                //var MarkerCallbackInfo = callbackInfo.info as AkMarkerCallbackInfo;
                Debug.Log("Marker reached");
                break;
        }
    }
}
