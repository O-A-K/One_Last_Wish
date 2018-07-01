using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour
{
    [SerializeField] private JB_DialogueStreamer dialogueStreamer;
    [SerializeField] private RectTransform responseContent;

    // Conversation variables
    public int emotionRange = 50;
    public int maxResponseOptions = 4;
    bool inConversation;
    bool conversationHalt;
    string currentEventName;
    int currentElementCluster;
    int currentElementIndex;
    List<int> possibleElements = new List<int>();
    List<ResponseButton> loadedResponses = new List<ResponseButton>();
    //[HideInInspector]
    List<string> activeMarkers = new List<string>();

    // UI
    [Header("UI")]
    [SerializeField] ResponseButton flavourTextPrefab;


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

            // display all the response options
            for (int i = 0; i < possibleElements.Count; i++)
            {
                ResponseButton _res = Instantiate(flavourTextPrefab, responseContent);
                _res.flavourText.text = dialogueStreamer.currentConvo[possibleElements[i]].flavourText;
                _res.indexCurrentConvo = possibleElements[i];
                loadedResponses.Add(_res);
            }
        }
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

    // Update is called once per frame
    void Update()
    {

    }
}
