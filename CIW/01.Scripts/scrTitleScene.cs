using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class scrTitleScene : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _fadeInChannel;
    [SerializeField] private Ease _easyType;

    [SerializeField] private GameObject _title;
    public List<GameObject> Buttons;
    [SerializeField] private GameObject _cards;
    [SerializeField] private GameObject _setting;

    private bool _isStart = false;
    private void Start()
    {
        StartCoroutine(TitleMove());
    }

    public void Play()
    {
        if(_isStart) return;
        _isStart = true;
        _fadeInChannel.RaiseEvent(false);
        DOVirtual.DelayedCall(1.5f, ()=>SceneManager.LoadScene("GameScene"));
    }

    public void Setting()
    {
        _setting.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -5), 2f).SetEase(_easyType);
    }

    public void Exit()
    {
        Application.Quit();
    }

    private IEnumerator TitleMove()
    {
        yield return new WaitForSeconds(0.5f);
        _title.GetComponentInChildren<RectTransform>().DOAnchorPos(new Vector2(5, 0),  2f).SetEase(_easyType);
        yield return new WaitForSeconds(0.5f);
        _cards.GetComponentInChildren<RectTransform>().DOAnchorPos(new Vector2(5, 0), 2f).SetEase(_easyType);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].GetComponentInChildren<RectTransform>().DOAnchorPos(new Vector2(0, 5), 1.5f).SetEase(_easyType);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
