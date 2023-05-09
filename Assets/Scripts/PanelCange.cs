using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class PanelCange : MonoBehaviour
{
    public GameObject Panel;

    void Start()
    {
        Panel.SetActive(false);
    }

    public void SettingView()
    {
        Panel.SetActive(true);
    }

    public void SettingOut()
    {
        Panel.SetActive(false);
    }
}