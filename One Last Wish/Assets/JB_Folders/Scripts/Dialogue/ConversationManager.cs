using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour
{
    [SerializeField] private JB_DialogueStreamer dialogueStreamer;
    [SerializeField] private RectTransform responseContent;

    // Conversation variables
    bool inConversation;
    bool conversationHalt;
    string currentEventName;
    int currentElementCluster;
    int currentElementIndex;
    List<int> possibleElements = new List<int>();
    //[HideInInspector]
    List<string> activeMarkers = new List<string>();

    // UI
    [Header("UI")]
    [SerializeField] Text flavourTextPrefab;


    void StartConversation()
    {
        dialogueStreamer.SetupConversation();
        currentElementCluster = 1;
        inConversation = true;
        LoadFirstLine();
    }

    void LoadFirstLine()
    {
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
        Debug.Log("Loading current response cluster");
        possibleElements.Clear();

        // ASSUMES ELEMENTS WITH SAME ELEMENT NUMBER ARE ALL CONTIGUOUS IN THE CSV
        bool foundCluster = false;
        for (int i = 0; i < dialogueStreamer.currentConvo.Count; i++)
        {
            if (dialogueStreamer.currentConvo[i].elementNumber == currentElementCluster)
            {
                if (!foundCluster) foundCluster = true;

                // if this element number matches the currentElementCluster and is cap's element, add it to possible elements list
                if (dialogueStreamer.currentConvo[i].lineType == Personae.cap)
                    possibleElements.Add(i);
                else Debug.LogError("Element on line " + i + 1 + "needs flavour text or is incorrectly numbered, ya dingleberry");

            }
            else if (foundCluster) break;   // once no longer in cluster can stop searching
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
