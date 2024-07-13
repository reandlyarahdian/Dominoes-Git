using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Logic : MonoBehaviour
{
    private string apiKey = "API Key goes here";        // built for using AppWarp
    private string secretKey = "Secret key goes here";  // built for using AppWarp

    private Dictionary<string, GameObject> unityObjects;
    private Stack<SC_GlobalEnums.Screens> screens_stack;
    private SC_GlobalEnums.Screens currScreen;
    private SC_GlobalEnums.GameMode gameMode;

    private List<string> roomIds;
    private string roomId;
    private int roomIndex;
    private Dictionary<string, object> passedParams;

    private int betValue;

    static SC_Logic instance;

    public static SC_Logic Instance
    {
        get
        {
            if(instance == null)
                instance = GameObject.Find("SC_Logic").GetComponent<SC_Logic>();

            return instance;
        }
    }

    void Awake() { Init(); }

    public void Btn_SinglePlayerLogic()
    {
        gameMode = SC_GlobalEnums.GameMode.SinglePlayer;
        ChangeScreen(SC_GlobalEnums.Screens.Game);
    }

    internal void Btn_BackToMenu()
    {
        ChangeScreen(SC_GlobalEnums.Screens.MainMenu);

        if (GameObject.Find("Screen_GameOver") != null)
            GameObject.Find("Screen_GameOver").SetActive(false);
    }

    public void Btn_SettingsLogic()
    {
        ChangeScreen(SC_GlobalEnums.Screens.Settings);
    }

    public void Slider_MusicVolumeLogic()
    {
        unityObjects["Txt_MusicValue"].GetComponent<Text>().text
            = "Music: " + (int)unityObjects["Slider_MusicVolume"].GetComponent<Slider>().value;
    }

    public void Slider_SfxVolumeLogic()
    {
        unityObjects["Txt_SfxValue"].GetComponent<Text>().text
            = "SFX: " + (int)unityObjects["Slider_SfxVolume"].GetComponent<Slider>().value;
    }

    public void Btn_BackLogic()
    {
        SC_GlobalEnums.Screens tempScreen = screens_stack.Pop();
        unityObjects["Screen_" + tempScreen].SetActive(true);
        unityObjects["Screen_" + currScreen].SetActive(false);
        currScreen = tempScreen;
    }

    private void Init()
    {
        screens_stack = new Stack<SC_GlobalEnums.Screens>();
        currScreen = SC_GlobalEnums.Screens.MainMenu;

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _objs = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _objs)
        unityObjects.Add(g.name, g);

        unityObjects["Screen_Settings"].SetActive(false);
        unityObjects["Screen_Game"].SetActive(false);
        unityObjects["SC_GameLogic"].SetActive(false);
    }

    private void ChangeScreen(SC_GlobalEnums.Screens _newScreen)
    {
        // If the screen has changed, do stack logic
        if (currScreen != _newScreen)
        {
            unityObjects["Screen_" + _newScreen].SetActive(true);
            unityObjects["Screen_" + currScreen].SetActive(false);
            screens_stack.Push(currScreen);
            currScreen = _newScreen;
        }

        // Initing game in case of game screen
        if (currScreen == SC_GlobalEnums.Screens.Game)
        {
            unityObjects["SC_GameLogic"].SetActive(true);
            SC_GameLogic.Instance.InitGame(gameMode);
        }
    }

}
