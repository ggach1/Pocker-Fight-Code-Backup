using System;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scrDead : MonoBehaviour
{
    [SerializeField] private GameObject _deadTxt;
    [SerializeField] private TextMeshProUGUI _afterTxt;
    [SerializeField] private BoolEventChannelSO _deadStartChannel;
    private string _sceneName = "TitleScene";

    private void Start()
    {
        _deadStartChannel.OnValueEvent += Dead;
        _afterTxt.text = "";
    }

    private void OnDisable()
    {
        _deadStartChannel.OnValueEvent -= Dead;
    }

    public void Dead(bool isDead)
    {
        if(!isDead) return;
        StartCoroutine(TxtShaking());
    }
    private IEnumerator TxtShaking()
    {
        _deadTxt.GetComponentInChildren<RectTransform>().DOScale(4.5f, 2f);
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(AutoAfterMove());
    }

    private IEnumerator AutoAfterMove()
    {
        _afterTxt.text = "타이틀로 이동합니다...";
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(_sceneName);
    }
}
