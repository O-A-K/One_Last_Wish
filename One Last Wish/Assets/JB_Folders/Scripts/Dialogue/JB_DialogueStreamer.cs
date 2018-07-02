using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConvoElement
{
    public int elementNumber;
    public Personae lineType;
    public string eventName;
    public string[] subtitleText;
    public int minEmotionRange;
    public int maxEmotionRange;
    public string[] requiredMarkers;
    public string[] resultantMarkers;
    public string flavourText;
    public float postDelay;
    public int emotionEffect;
    public int leadsToElement;
    public ElementType firstLineType;  // only for first line
}

public enum Personae
{
    cap,
    wife
}

public enum ElementType
{
    line,
    response
}

public class JB_DialogueStreamer : MonoBehaviour
{
    [SerializeField] private TextAsset convo;
    // Splitters
    char lineSplitter = '\n';
    char fieldSplitter = ',';
    char subSplitter = '~';

    List<string> fullConvoSplit = new List<string>();
    string[] convoElementRawStrings;

    [HideInInspector]
    public List<ConvoElement> currentConvo = new List<ConvoElement>();
    //string currentEventName;
    //int currentElementCluster;
    //int currentElementIndex;
    //List<int> possibleElements = new List<int>();
    

    #region Setup Conversation

    public void SetupConversation()     // get conversation in CSV form, split it into ConvoElements and begin playing the conversation
    {
        currentConvo.Clear();
        fullConvoSplit.Clear();
        fullConvoSplit = new List<string>(convo.text.Split(lineSplitter));    // split the CSV file by line to get each conversation element in its own array field

        for (int i = 1; i < fullConvoSplit.Count; i++)
        {
            convoElementRawStrings = fullConvoSplit[i].Split(fieldSplitter);                // split the conversation element
            currentConvo.Add(SplitStringToConvoElement(convoElementRawStrings, i + 1));     // create ConvoElement for each line of dialogue
        }
    }

    ConvoElement SplitStringToConvoElement(string[] rawElement, int lineNum)    // take the string array and turn it into a ConvoElement
    {
        ConvoElement convElement = new ConvoElement();

        // get element number
        if (!Int32.TryParse(rawElement[0], out convElement.elementNumber))
            ErrorParsingConversation(lineNum, "get element number");    // if it can't parse the first string to a number send error

        // get the Wwise event name
        if (rawElement[1] == "") ErrorParsingConversation(lineNum, "get Wwise event name (nothing written in field)");
        else convElement.eventName = rawElement[1];

        // get the element's subtitles
        if (rawElement[2] == "") ErrorParsingConversation(lineNum, "get element text body (nothing written in field)");
        else convElement.subtitleText = rawElement[2].Split(subSplitter);

        // get min and max emotion ranges
        if (!Int32.TryParse(rawElement[3], out convElement.minEmotionRange))
            ErrorParsingConversation(lineNum, "get minimum emotion range");
        if (!Int32.TryParse(rawElement[4], out convElement.maxEmotionRange))
            ErrorParsingConversation(lineNum, "get maximum emotion range");

        // add required markers if relevant
        if (rawElement[5] == "") ErrorParsingConversation(lineNum, "get required marker (make sure to insert '-' if no markers are required)");
        else if (rawElement[5] == "-") convElement.requiredMarkers = null;
        else convElement.requiredMarkers = rawElement[5].Split(subSplitter);

        // add resulting markers if relevant
        if (rawElement[6] == "") ErrorParsingConversation(lineNum, "get required marker (make sure to insert '-' if no markers are required)");
        else if (rawElement[6] == "-") convElement.resultantMarkers = null;
        else convElement.resultantMarkers = rawElement[6].Split(subSplitter);

        // get response flavour text (if none then it means this element is wife's line)
        if (rawElement[7] == "") convElement.lineType = Personae.wife;
        else
        {
            convElement.lineType = Personae.cap;
            convElement.flavourText = rawElement[7];
        }

        // get the minimum delay in seconds to wait until next interaction
        if (rawElement[8] == "") ErrorParsingConversation(lineNum, "get post delay (no input was detected)");
        else if (!float.TryParse(rawElement[8], out convElement.postDelay))
            ErrorParsingConversation(lineNum, "get post delay. Input was non-numerical");

        // get emotion state change of element
        if (!Int32.TryParse(rawElement[9], out convElement.emotionEffect))
            ErrorParsingConversation(lineNum, "get emotion state change value");

        // get element which this one leads to
        if (!Int32.TryParse(rawElement[10], out convElement.leadsToElement))
            ErrorParsingConversation(lineNum, "get the element cluster which this one leads to");

        // for first element only, get element type (line 'l' or response 'r') if a captain element
        if (lineNum == 2 && convElement.lineType == Personae.cap)
        {
            if (rawElement[11].Length == 2) // if just a character
            {
                if (rawElement[11][0] == 'l') convElement.firstLineType = ElementType.line;
                else if (rawElement[11][0] == 'r') convElement.firstLineType = ElementType.response;
                else ErrorParsingConversation(lineNum, "get first line element type (ensure it is either 'l' or 'r')");
            }
            else
                ErrorParsingConversation(lineNum, "get first line element type (make sure it is only one character in length)");
        }

        return convElement;
    }

    void ErrorParsingConversation(int lineNum, string failedAction) // outlines error when parsing CSV file
    {
        Debug.LogError("Error parsing conversation from text asset, " + convo.name + ", on line " + lineNum + " while trying to " + failedAction + " you dingus");
    }

    #endregion

}
