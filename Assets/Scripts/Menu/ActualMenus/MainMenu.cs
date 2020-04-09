using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MenuBase
{
    [SerializeField]
    GameObject EasterEggPanel = null;
    [SerializeField]
    GameObject m_Knob = null;

    Quaternion m_StartRot;
    Quaternion m_EndRot;

    protected override void Start()
    {
        CanGoBackFromBaseMenu = false;
        base.Start();

        m_StartRot = new Quaternion(m_Knob.transform.rotation.x, m_Knob.transform.rotation.y, m_Knob.transform.rotation.z, m_Knob.transform.rotation.w);
    }

    protected override void Update()
    {
        base.Update();
    }

    public void ToggleEasterEgg()
    {
        EasterEggPanel.SetActive(!EasterEggPanel.activeInHierarchy);

        //rotate
        if(EasterEggPanel.activeInHierarchy)
        {
            Quaternion temp = m_StartRot;
            temp.eulerAngles = new Vector3(m_StartRot.eulerAngles.x, m_StartRot.eulerAngles.y, m_StartRot.eulerAngles.z - 35.0f);

            m_Knob.transform.rotation = temp;
        }
        else
        {
            m_Knob.transform.rotation = m_StartRot;
        }
    }
}
