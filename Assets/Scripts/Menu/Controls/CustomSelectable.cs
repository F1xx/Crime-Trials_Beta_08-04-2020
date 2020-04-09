using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomSelectable : TooltipEventTrigger, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    Image SelectionImage = null;
    [SerializeField]
    float AlphaLevelWhenSelected = 0.4f;

    protected override void Awake()
    {
        base.Awake();

        OnDeselect(null);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        Color col = SelectionImage.color;
        col.a = 0.0f;

        SelectionImage.color = col;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        Color col = SelectionImage.color;
        col.a = AlphaLevelWhenSelected;

        SelectionImage.color = col;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }
}
