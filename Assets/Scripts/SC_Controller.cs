using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Controller : MonoBehaviour
{
    public void Btn_SinglePlayer() { SC_Logic.Instance.Btn_SinglePlayerLogic(); }

    public void Btn_Settings() { SC_Logic.Instance.Btn_SettingsLogic(); }

    public void Btn_Back() { SC_Logic.Instance.Btn_BackLogic(); }

    public void Slider_MusicVolume() { SC_Logic.Instance.Slider_MusicVolumeLogic(); }

    public void Slider_SfxVolume() { SC_Logic.Instance.Slider_SfxVolumeLogic(); }

    public void Btn_BackToMenu() { SC_Logic.Instance.Btn_BackToMenu(); }
}
