using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConvoElement
{
    // For line - int , int , int , string , string , int , int
    public ConvoElement(int _elementNumber, int _minRange, int _maxRange, string _eventName, string _lineText, int _emotionStateChange, int _leadsToLine)
    {
        elementNumber = _elementNumber;
        lineType = LineType.line;
        minRange = _minRange;
        maxRange = _maxRange;
        eventName = _eventName;
        emotionStateChange = _emotionStateChange;
        lineText = _lineText;
        leadsToLine = _leadsToLine;
        responseFlavour = null;
    }
    //

    // For response - int , int , int , string , int , int, string
    public ConvoElement(int _elementNumber, int _minRange, int _maxRange, string _eventName, int _emotionStateChange, int _leadsToLine, string _responseFlavour)
    {
        elementNumber = _elementNumber;
        lineType = LineType.response;
        minRange = _minRange;
        maxRange = _maxRange;
        eventName = _eventName;
        emotionStateChange = _emotionStateChange;
        leadsToLine = _leadsToLine;
        responseFlavour = _responseFlavour;
        lineText = null;
    }
    //

    public int elementNumber;
    public LineType lineType;
    public int minRange;
    public int maxRange;
    public string eventName;
    public string lineText;
    public string responseFlavour;
    public int emotionStateChange;
    public int leadsToLine;
}

public enum LineType
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

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupConversation()
    {
        fullConvoSplit.Clear();
        fullConvoSplit = new List<string>(convo.text.Split(lineSplitter));    // split the CSV file by line to get each conversation element in its own array field

        for (int i = 0; i < fullConvoSplit.Count; i++)
        {
            convoElementRawStrings = fullConvoSplit[i].Split(fieldSplitter);    // split the conversation element
            currentConvo.Add(SplitStringToConvoElement(convoElementRawStrings, i + 1));
        }
    }

    ConvoElement SplitStringToConvoElement(string[] rawElement, int lineNum)
    {
        ConvoElement convElement = new ConvoElement();
        if (System.Int32.TryParse(rawElement[0], out convElement.elementNumber))
        {

        }

        return convElement;
    }

    void ErrorParsingConversation(int lineNum)
    {
        Debug.LogError("Error parsing conversation from text asset on line " + lineNum + " you dingus");
    }

}
