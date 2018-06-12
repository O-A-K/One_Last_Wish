using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConvoElement
{
    //// For line - int , int , int , string , string , int , int
    //public ConvoElement(int _elementNumber, int _minRange, int _maxRange, string _eventName, string _lineText, int _emotionStateChange, int _leadsToLine)
    //{
    //    elementNumber = _elementNumber;
    //    lineType = LineType.line;
    //    minRange = _minRange;
    //    maxRange = _maxRange;
    //    eventName = _eventName;
    //    emotionStateChange = _emotionStateChange;
    //    lineText = _lineText;
    //    leadsToLine = _leadsToLine;
    //    responseFlavour = null;
    //}
    ////

    //// For response - int , int , int , string , int , int, string
    //public ConvoElement(int _elementNumber, int _minRange, int _maxRange, string _eventName, int _emotionStateChange, int _leadsToLine, string _responseFlavour)
    //{
    //    elementNumber = _elementNumber;
    //    lineType = LineType.response;
    //    minRange = _minRange;
    //    maxRange = _maxRange;
    //    eventName = _eventName;
    //    emotionStateChange = _emotionStateChange;
    //    leadsToLine = _leadsToLine;
    //    responseFlavour = _responseFlavour;
    //    lineText = null;
    //}
    ////

    public int elementNumber;
    public ElementType lineType;
    public int minRange;
    public int maxRange;
    public string requiredMarker;
    public string eventName;
    public string elementText;
    public int emotionStateChange;
    public int leadsToElement;
    public string addMarker;
}

public enum ElementType
{
    line,
    response
}

public class JB_DialogueStreamer : MonoBehaviour
{
    public TextAsset convo;
    // Splitters
    char lineSplitter = '\n';
    char fieldSplitter = ',';
    char subSplitter = '~';

    List<string> fullConvoSplit;
    string[] convoElementRawStrings = new string[10];

    List<ConvoElement> currentConvo = new List<ConvoElement>();
    string currentEventName;
    int currentElement;
    List<int> possibleElements = new List<int>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Setup Conversation

    public void SetupConversation()     // get conversation in CSV form, split it into ConvoElements and begin playing the conversation
    {
        currentElement = 1;     // begin with element number 1
        fullConvoSplit.Clear();
        fullConvoSplit = new List<string>(convo.text.Split(lineSplitter));    // split the CSV file by line to get each conversation element in its own array field

        for (int i = 0; i < fullConvoSplit.Count; i++)
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

        // get element type
        if (rawElement[1] == "l") convElement.lineType = ElementType.line;             // if it's a line
        else if (rawElement[1] == "r") convElement.lineType = ElementType.response;    // if it's a response
        else ErrorParsingConversation(lineNum, "get element type");

        // get min and max emotion ranges
        if (!Int32.TryParse(rawElement[2], out convElement.minRange))
            ErrorParsingConversation(lineNum, "get minimum emotion range");
        if (!Int32.TryParse(rawElement[3], out convElement.maxRange))
            ErrorParsingConversation(lineNum, "get maximum emotion range");

        // add required marker if relevant
        if (rawElement[4] == "") ErrorParsingConversation(lineNum, "get required marker (make sure to insert '-' if no marker is required)");
        else if (rawElement[4] == "-") convElement.requiredMarker = null;
        else convElement.requiredMarker = rawElement[4];

        // get the Wwise event name
        if (rawElement[5] == "") ErrorParsingConversation(lineNum, "get Wwise event name (nothing written in field)");
        else convElement.eventName = rawElement[5];

        // get the element's text (subtitles for line, response flavour text for responses)
        if (rawElement[6] == "") ErrorParsingConversation(lineNum, "get element text body (nothing written in field)");
        else convElement.elementText = rawElement[6];

        // get emotion state change of element
        if (!Int32.TryParse(rawElement[7], out convElement.emotionStateChange))
            ErrorParsingConversation(lineNum, "get emotion state change value");

        // get element which this one leads to
        if (!Int32.TryParse(rawElement[8], out convElement.leadsToElement))
            ErrorParsingConversation(lineNum, "get the element cluster which this one leads to");

        // get marker to add
        if (rawElement[4] == "") ErrorParsingConversation(lineNum, "get marker to add (make sure to insert '-' if no marker is added when encountering this element)");
        else if (rawElement[4] == "-") convElement.requiredMarker = null;
        else convElement.requiredMarker = rawElement[4];

        return convElement;
    }

    void ErrorParsingConversation(int lineNum, string failedAction)
    {
        Debug.LogError("Error parsing conversation from CSV on line " + lineNum + " while trying to " + failedAction + " you dingus");
    }

    #endregion

    void ()
    {

    }
}
