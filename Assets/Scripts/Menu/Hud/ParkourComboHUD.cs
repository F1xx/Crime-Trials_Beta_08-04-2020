using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourComboHUD : BaseObject
{
    [Header("Combo Numbers"), SerializeField]
    TMPro.TMP_Text ComboText = null;
    [SerializeField, Space(15)]
    public Color FullTimerColor = Color.green;
    [SerializeField]
    public Color EmptyTimerColor = Color.red;

    [Header("Cooldowns"), SerializeField]
    UnityEngine.UI.Image SlideFillSprite = null;
    [SerializeField]
    UnityEngine.UI.Image JumpFillSprite = null;
    [SerializeField]
    UnityEngine.UI.Image ShootFillSprite = null;
    [SerializeField]
    UnityEngine.UI.Image WallRunFillSprite = null;
    [SerializeField, Space(15)]
    public Color CooldownComplete = Color.white;
    [SerializeField]
    public Color OnCooldown = Color.red;

    [Header("Backgrounds"), SerializeField]
    UnityEngine.UI.Image BackgroundImage = null;
    [SerializeField]
    Sprite LowScoreSprite = null;
    [SerializeField]
    Sprite MiddleScoreSprite = null;
    [SerializeField]
    Sprite HighScoreSprite = null;
    [SerializeField]
    Sprite MaxScoreSprite = null;

    float m_Combo = 0.0f;

    protected override void Awake()
    {
        base.Awake();

        ParkourComboManager.Instance();

        //listen to everything we care about
        Listen("OnComboChange", OnComboChange);
        Listen("OnSlideCombo", () => { OnCombo(SlideFillSprite); });
        Listen("OnDoubleJumpCombo", () => { OnCombo(JumpFillSprite); });
        Listen("OnShootCombo", () => { OnCombo(ShootFillSprite); });
        Listen("OnWallRunCombo", () => { OnCombo(WallRunFillSprite); });
        Resettext(); //update the text to be right
    }

    private void Start()
    {
        //using lambdas to pass in the sprite my function needs to know about
        ParkourComboManager pcm = ParkourComboManager.Instance();
        pcm.m_SlideCooldown.AddListener(() => { OnTimerComplete(SlideFillSprite); });
        pcm.m_DoubleJumpCooldown.AddListener(() => { OnTimerComplete(JumpFillSprite); });
        pcm.m_ShootCooldown.AddListener(() => { OnTimerComplete(ShootFillSprite); });
        pcm.m_WallrunCooldown.AddListener(() => { OnTimerComplete(WallRunFillSprite); });
    }

    private void Update()
    {
        if (m_Combo > 0.0f)
        {
            ComboText.color = Color.Lerp(FullTimerColor, EmptyTimerColor, ParkourComboManager.GetComboTimerPercentComplete());
        }

        ParkourComboManager pcm = ParkourComboManager.Instance();

        CheckIfRunningAndUpdateSprite(SlideFillSprite, pcm.m_SlideCooldown);
        CheckIfRunningAndUpdateSprite(JumpFillSprite, pcm.m_DoubleJumpCooldown);
        CheckIfRunningAndUpdateSprite(ShootFillSprite, pcm.m_ShootCooldown);
        CheckIfRunningAndUpdateSprite(WallRunFillSprite, pcm.m_WallrunCooldown);
    }

    void CheckIfRunningAndUpdateSprite(UnityEngine.UI.Image sprite, Timer toCheck)
    {
        if (toCheck.IsRunning)
        {
            float percentage = toCheck.GetPercentageComplete();

            sprite.fillAmount = percentage;


            //Vector3 tempscale = sprite.localScale;
            //tempscale.y = SpriteFullYScale * percentage;
            //sprite.localScale = tempscale;
        }
    }

    void OnComboChange()
    {
        m_Combo = ParkourComboManager.ComboMult;
        ComboText.text = "x" + m_Combo.ToString("F1");

        if (m_Combo == 0.0f)
        {
            ComboText.color = Color.white;

            SetBackground(null, 0.0f);
        }
        else if(m_Combo > 0.0f && m_Combo < 3.0f)
        {
            SetBackground(LowScoreSprite, 1.0f);
        }
        else if (m_Combo >= 3.0f && m_Combo < 6.0f)
        {
            SetBackground(MiddleScoreSprite, 1.0f);
        }
        else if (m_Combo >= 6.0f && m_Combo < 10.0f)
        {
            SetBackground(HighScoreSprite, 1.0f);
        }
        else if (m_Combo >= 10.0f)
        {
            SetBackground(MaxScoreSprite, 1.0f);
        }
    }

    void SetBackground(Sprite sprite, float alpha)
    {
        BackgroundImage.sprite = sprite;
        Color temp = BackgroundImage.color;

        if (temp.a != alpha)
        {
            temp.a = alpha;
            BackgroundImage.color = temp;
        }
    }

    void Resettext()
    {
        OnComboChange();
    }

    protected override void OnSoftReset()
    {
        Resettext();

        OnTimerComplete(JumpFillSprite);
        OnTimerComplete(ShootFillSprite);
        OnTimerComplete(SlideFillSprite);
        OnTimerComplete(WallRunFillSprite);
    }

    void OnCombo(UnityEngine.UI.Image sprite)
    {
        sprite.fillAmount = 0.0f;

        sprite.color = OnCooldown;
    }

    void OnTimerComplete(UnityEngine.UI.Image sprite)
    {
        sprite.fillAmount = 1.0f;

        sprite.color = CooldownComplete;
    }
}
