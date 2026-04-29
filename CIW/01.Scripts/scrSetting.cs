using DG.Tweening;
using UnityEngine;

public class scrSetting : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void PressX()
    {
        _rect.DOAnchorPos(new Vector2(0, 1125), 2f).SetEase(Ease.OutQuart);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
