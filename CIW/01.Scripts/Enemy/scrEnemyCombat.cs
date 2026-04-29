using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class scrEnemyCombat : MonoBehaviour, IHittable
{
    public bool attackEnd = true;
    private Animator _animator;
    private readonly string _attackHash = "Attack";
    private readonly string _hitHash = "Hit";
    private readonly string _deadHash = "Dead";
    
    [SerializeField] private GameObject _objHpFillBar;
    public GameObject _arrow;
    [SerializeField] private TMP_Text _enemyText;
    [SerializeField] private TMP_Text _DamageTxt;

    public scrPlayerCombat _scrPlayer;
    private CombatManager _combatManager;

    private float _hp;
    private float _maxHp;

    public EnemySO EnemySO;

    public float HP
    {
        get => _hp;
        // hp�� 0�Ʒ��� �������� �ʵ���
        private set => _hp = Mathf.Clamp(value, 0, EnemySO?.hp ?? 100);
    }
    
    private void Awake()
    {
        _scrPlayer = FindObjectOfType<scrPlayerCombat>();
        _combatManager = FindObjectOfType<CombatManager>();
        _animator = GetComponent<Animator>();
    }

    public bool IsFullHp()
    {
        return HP == _maxHp;
    }
    private void Start()
    {
        _arrow.SetActive(false);
        _DamageTxt.SetText("");

        InitializeEnemy();
    }

    public void InitializeEnemy()
    {
        if (EnemySO == null)
        {
            Debug.LogError("EnemySO�� �������� �ʾҽ��ϴ�.");
            return;
        }

        HP = EnemySO.hp; // SO���� �ʱ� ü�� ����
        // UpdateHealthUI(); // ü�� �ٿ� �ؽ�Ʈ ������Ʈ
        float healthRatio = HP / EnemySO.hp;
        _objHpFillBar.transform.localScale = new Vector3(healthRatio, 1, 1);
        StartCoroutine(EnemyHpText(HP));
    }

    public void SetMaxHealth(float maxHealth)
    {
        HP = maxHealth;
        _maxHp = maxHealth;
        _objHpFillBar.transform.localScale = new Vector3(1, 1, 1);
        float healthRatio = HP / EnemySO.hp;
        _objHpFillBar.transform.localScale = new Vector3(healthRatio, 1, 1);
        Debug.Log($"���� �ִ� ü���� {maxHealth}�� �����Ǿ����ϴ�.");
    }

    public void GetDamage(float damage)
    {
        if (_combatManager.isTriple)
        {
            if (IsFullHp())
                damage *= 2;
            _combatManager.isTriple = false;
        }
        else if (_combatManager.isFourCard)
        {
            float hpPercentage = (HP / _maxHp) * 100f;
            if (hpPercentage <= 25f)
            {
                damage = HP;
            }
        }
        HP -= damage;
        UpdateHealthUI();
        _animator.SetTrigger(_hitHash);
        
        if (HP <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        float healthRatio = HP / EnemySO.hp;
        _objHpFillBar.transform.localScale = new Vector3(healthRatio, 1, 1);
        StartCoroutine(EnemyHpText(HP));
        StartCoroutine(EnemyDamageText());
    }

    private void Die()
    {
        // ���ó�� 
        Debug.Log($"{EnemySO.name}��(��) ����߽��ϴ�.");
        _animator.SetTrigger(_deadHash);
    }

    public void DeadEvent()
    {
        _combatManager.RemoveEnemy(this);
        Destroy(gameObject);
    }
        
    
    public void AttackTry()
    {
        if (_scrPlayer != null)
        {
            float damage = EnemySO?.damage ?? 0;
            Debug.Log($"{EnemySO.name}�� �÷��̾ �����Ͽ� {damage} ���ظ� �������ϴ�.");
            _scrPlayer.GetDamage(damage);
        }
        else
        {
            Debug.LogError("scrPlayerCombat ������Ʈ �� ã��!!!!");
        }
    }

    public void AttackEnd() => attackEnd = true;
    public void Attack()
    {
        attackEnd = false;
        _animator.SetTrigger(_attackHash);
    }

    private IEnumerator EnemyHpText(float hp)
    {
        _enemyText.SetText($"{hp} / {EnemySO.hp}");
        yield return new WaitForSeconds(0.5f);
        _DamageTxt.SetText("");
    }
    private IEnumerator EnemyDamageText()
    {
        _DamageTxt.SetText($"-{_combatManager.Damage}");
        _DamageTxt.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        _DamageTxt.SetText("");
    }
}
