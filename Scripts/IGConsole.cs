using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class IGConsole : MonoBehaviour
{
    public bool startEnabled = true,    //If the console should be enabled by default
        disableInEditor = true;         //if the console should be disabled in the editor at all times
    
    //key used to toggle the console on and off
    public KeyCode toggleKey = KeyCode.F11; 
    //if the console is currently enabled or not
    bool isEnabled = true;

    //percentage of the screen the console covers
    public Vector2 ScreenSize = new Vector2(.33f, .3f);

    //list of all texts in the console
    List<Text> Texts = new List<Text>();

    bool initialized = false;
    Image PanelImg;

    //how opaque the console is
    float alpha = 0f;

    //how long to display the panel for
    public float timeToDisplay = 2f;

    //what types of messages to print
    public bool printMessages = false, printErrors = true;

    //outline colour of the messages
    public Color LogMessageColour = new Color(0, .64f, 1, .4f),
        ErrorMessageColour = new Color(1, 0, 0, .4f);

    //references to objects
    public GameObject Panel;
    public GameObject Txt;

    public void OnEnable()
    {
        //Disable the Object if it's in the editor and you don't want it
        if (Application.isEditor && disableInEditor)
        {
            gameObject.SetActive(false);
            return;
        }

        //Get the log messages
        Application.logMessageReceived += onLog;

        //only ever initialize once
        if (initialized) return;

        isEnabled = startEnabled;

        //setup the panel
        Panel.SetActive(true);
        PanelImg = Panel.GetComponent<Image>();
        ((RectTransform)Panel.transform).sizeDelta = new Vector2(Screen.width * ScreenSize.x, Screen.height * ScreenSize.y);

        //setup text
        GameObject cur;
        ((RectTransform)Txt.transform).sizeDelta = new Vector2(Screen.width * ScreenSize.x - 3, 18);
        Txt.GetComponent<Text>().text = "";

        //create the text objects to print to
        for (int i = 0; i < ((Screen.height * ScreenSize.y) / 18) - 1; ++i)
        {
            cur = (GameObject)Instantiate(Txt, Vector3.right * 3 + Vector3.down * i * 18 + Vector3.down * 2, Quaternion.identity);
            cur.GetComponent<RectTransform>().SetParent(Panel.GetComponent<RectTransform>());
            cur.GetComponent<RectTransform>().localScale = Vector3.one;
            cur.GetComponent<Text>().text = "";
            Texts.Add(cur.GetComponent<Text>());
        }

        //flip it so the bottom text is printed to first
        Texts.Reverse();

        Destroy(Txt);

        initialized = true;
    }

    public void OnDisable()
    {
        if (Application.isEditor && disableInEditor)
            return;
        Application.logMessageReceived -= onLog;
    }
	
	void Update () {

        //toggle the console on and off with the selected key
        if (Input.GetKeyDown(toggleKey))
        {
            isEnabled = !isEnabled;

            Panel.SetActive(isEnabled);

            //display the console if it was just enabled
            if (isEnabled)
                alpha = timeToDisplay * 2f;
        }

        setAlpha();
    }

    /// <summary>Sets the alpha of the various componets involved</summary>
    void setAlpha()
    {
        alpha -= Time.deltaTime * 2f;
        Color black = new Color(0, 0, 0, Mathf.Clamp01(alpha));

        PanelImg.color = new Color(1, 1, 1, Mathf.Clamp01(alpha) * (100f / 255f));

        for (int i = 0; i < Texts.Count; ++i)
            Texts[i].color = black;
    }

    /// <summary>when a log or error message is recieved, this is called</summary>

    void onLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error && printErrors)
            push(message, ErrorMessageColour);
        else if (type == LogType.Log && printMessages)
            push(message, LogMessageColour);
    }

    /// <summary>pushes a new message to the console</summary>
    void push(string s, Color outlineCol)
    {
        alpha = timeToDisplay * 2f;
        for (int i = Texts.Count - 1; i >= 0; --i)
        {
            if (i == 0)
            {
                Texts[i].text = s;
                Texts[i].GetComponent<Outline>().effectColor = outlineCol;
            }
            else
            {
                Texts[i].text = Texts[i - 1].text;
                Texts[i].GetComponent<Outline>().effectColor = Texts[i - 1].GetComponent<Outline>().effectColor;
            }
        }
    }
}
