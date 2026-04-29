using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;

public class scrPlayerCombat : MonoBehaviour, IHittable
{
    [SerializeField] private BoolEventChannelSO _deadEventChannel;
    public bool isSkip = false;
    
    [SerializeField] private BoolEventChannelSO _playerTurnEndChannel;
    private Animator _animator;
    private readonly string _moveHash = "Move";
    private readonly string _attackHash = "Attack";
    private readonly string _hitHash = "Hit";
    private readonly string _deadHash = "Dead";
    public void MoveAniOnOff(bool isOn)
    {
        _animator.SetBool(_moveHash, isOn);
    }
    
    [SerializeField] private GameObject _objHpFillBar;
    [SerializeField] private TMP_Text _playerHpText;
    [SerializeField] private TMP_Text _playerDamText;

    private scrEnemyCombat _targetEnemy;
    private CombatManager _combatManager;

    private float _hp;
    private float _getDam;

    public float HP
    {
        get => _hp;
        // hp�� 0�Ʒ��� �������� �ʵ���
        private set => _hp = Mathf.Clamp(value, 0, _combatManager.MaxHp);
    }

    private void Awake()
    {
        _targetEnemy = FindObjectOfType<scrEnemyCombat>();
        _combatManager = FindObjectOfType<CombatManager>();
        _animator = GetComponent<Animator>();
        _playerTurnEndChannel.OnValueEvent += AttackEnter;
    }

    private void OnDestroy()
    {
        _playerTurnEndChannel.OnValueEvent -= AttackEnter;
    }

    private void Start()
    {
        _hp = _combatManager.MaxHp;

        StartCoroutine(PlayerHpText(HP, Color.white));
    }
    
    public void AttackEnter(bool obj)
    {
        if(!obj) return;
        _animator.SetTrigger(_attackHash);
    }
    public void AttackEnd()
    {
        if (isSkip)
        {
            isSkip = false;
            CardManager.Instance.isPlayer = true;
            return;
        }
        DOVirtual.DelayedCall(3f, () =>_playerTurnEndChannel.RaiseEvent(false));
    }
    public void AttackTarget()
    {
        if (_combatManager.isMulti)
        {
            _combatManager.MultiAttack();
            return;
        }
        
        int enemyIndex = _combatManager.index;
        if (enemyIndex < 0 || enemyIndex >= _combatManager.ActiveEnemies.Count)
            return;

        _targetEnemy = _combatManager.ActiveEnemies[enemyIndex];
        Thunder.Instance.Attack(_targetEnemy.transform);
        Debug.Log($"�÷��̾� {_targetEnemy.EnemySO.id} Ÿ����");

        if (_targetEnemy != null)
        {
            Debug.Log("player damaged : " + _combatManager.Damage);
            _targetEnemy.GetDamage(_combatManager.Damage);
        }
        else
        {
            Debug.LogError("no targetting enemy");
        }
    }
    /// <summary>
    /// �÷��̾� ���� Ÿ�� ����
    /// </summary>
    /// <param name="enemyIndex"></param>
    public void AttackTarget(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= _combatManager.ActiveEnemies.Count)
            return;

        _targetEnemy = _combatManager.ActiveEnemies[enemyIndex];
        Debug.Log($"�÷��̾� {_targetEnemy.EnemySO.id} Ÿ����");

        if (_targetEnemy != null)
        {
            Debug.Log("player damaged : " + _combatManager.Damage);
            _targetEnemy.GetDamage(_combatManager.Damage);
        }
        else
        {
            Debug.LogError("no targetting enemy");
        }
    }
    

    public void SetMaxHealth(float heal)
    {
        _objHpFillBar.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        Debug.Log("set basic health - player : " + heal);
    }

    public void GetDamage(float dam)
    {
        _animator.Play(_hitHash);
        
        _getDam = dam;
        float getDamagedHp = HP - dam;
        Debug.Log($"HP : {HP} || dam : {dam} || getDamagedHp : {getDamagedHp}");
        HP = getDamagedHp;
        Debug.Log("now player hp : " + HP);
        float bar = HP / _combatManager.MaxHp;
        StartCoroutine(PlayerHpText(HP, Color.red));
        _objHpFillBar.GetComponent<Transform>().localScale = new Vector3(bar, 1, 1);

        if (HP <= 0)
        {
            CombatManager.Instance.isLoading = true;
            CardManager.Instance.isLoading = true;
            _animator.SetTrigger(_deadHash);
        }
        // Attack();
    }

    private void Die()
    {
        Time.timeScale = 1;
        _deadEventChannel.RaiseEvent(false);
    }

    public void Heal(float heal)
    {
        HP += heal;
        Debug.Log($"HP : {HP} || heal : {heal} || heal Hp : {HP}");
        Debug.Log("hp healed : " + HP);
        StartCoroutine(PlayerHpText(HP, Color.green));
        _objHpFillBar.GetComponent<Transform>().localScale = new Vector3(HP / _combatManager.MaxHp, 1, 1);
    }

    private IEnumerator PlayerHpText(float hp, Color txtColor)
    {
        _playerHpText.SetText($"{hp} / {_combatManager.MaxHp}");
        if (txtColor == Color.red)
        {
            _playerDamText.SetText($"-{_getDam}");
            _playerDamText.color = txtColor;
        }
        else if (txtColor == Color.green)
        {
            _playerDamText.SetText($"+{_combatManager.Heal}");
            _playerDamText.color = txtColor;
        }

        yield return new WaitForSeconds(1f);

        _playerDamText.SetText("");
    }
}
