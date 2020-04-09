using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator_UnlockArms : SideWaysNavigator
{

    [SerializeField]
    UnityEngine.UI.Image displayImage = null;

    [SerializeField]
    Sprite[] loadedImages = null;

    [SerializeField]
    Sprite lockedArmImage = null;

    [SerializeField]
    TMPro.TMP_Text displayText = null;

    [SerializeField]
    string[] displayTextStrings = null;

    int ExpectedImageCount;
    bool IsFunctional = false;

    [SerializeField]
    int TrackingIndex = 0;


    protected override void Awake()
    {
        Options = new string[loadedImages.Length];
        for (int i = 0; i < loadedImages.Length; i++)
        {
            Options[i] = loadedImages[i].name;
        }

        base.Awake();

        TrackingIndex = CurrentIndex;
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        ExpectedImageCount = SQLManager.Instance().LoadedUnlocks["Arm"].Count;

        if (ExpectedImageCount != loadedImages.Length || lockedArmImage == null || displayTextStrings.Length != ExpectedImageCount)
        {
            Debug.LogWarning(string.Format("UnlockArms navigator failed to load."));
        }
        else
        {
            IsFunctional = true;
        }

        base.LoadSettings();

        TrackingIndex = CurrentIndex;
    }


protected override void ChangeTextToIndex()
    {
        if (IsFunctional)
        {
            if (SQLManager.Instance().LoadedUnlocks["Arm"][TrackingIndex] == false)
            {
                displayImage.sprite = lockedArmImage;
                displayText.text = "LOCKED";
            }
            else
            {
                displayImage.sprite = loadedImages[CurrentIndex];
                displayText.text = displayTextStrings[CurrentIndex];
            }
        }
    }

    public override void PreviousSetting()
    {
        if (IsFunctional)
        {
            TrackingIndex = (TrackingIndex - 1) % ExpectedImageCount;
            if (TrackingIndex < 0) TrackingIndex = ExpectedImageCount - 1;

            CheckIfIndexIsLocked();
        }
    }

    public override void NextSetting()
    {
        if (IsFunctional)
        {
            TrackingIndex = (TrackingIndex + 1) % ExpectedImageCount;

            CheckIfIndexIsLocked();
        }
    }

    private void CheckIfIndexIsLocked()
    {
        if (SQLManager.Instance().LoadedUnlocks["Arm"][TrackingIndex] == true)
        {
            SetNewIndex(TrackingIndex);
        }
        else
        {
            ChangeTextToIndex();
        }
    }

    protected override void CheckCurrentIndex()
    {

    }

    protected override void ResetDefaults()
    {
        TrackingIndex = PlayerPrefs.GetInt(PREF_KEY + "Default");
        SetNewIndex(TrackingIndex);
    }
}
