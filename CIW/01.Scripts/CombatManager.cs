using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public struct EnemySpawnPer
{
    public int OneSpawnPer;
    public int TwoSpawnPer;
    public int ThreeSpawnPer;
}
public class CombatManager : MonoSingleton<CombatManager>
{
    [SerializeField] private TextMeshPro stageTxt;
    public bool isTriple=false, isFourCard=false;
    public bool isSkip = false;
    [SerializeField] private Transform allTrm;
    public bool isLoading = false;
    [SerializeField] private EnemyListSO _enemyListSo;
    public List<EnemySpawnPer> enemySpawnPerList;
    public int stageNum = 1;
    public int index;
    public bool isMulti;
    [SerializeField] private BoolEventChannelSO _playerTurnEndChannel;
    [SerializeField] private BoolEventChannelSO _roundEndChannel;

    [Header("Scripts")]
    public scrPlayerCombat scrPlayerCombat;

    #region Player

    [Header("Player")]
    public float MaxHp = 100f;
    public float Damage = 10f; // �÷��̾� ������ - ī��� ���� ����
    public float Heal = 5f; // �÷��̾� ȸ�� - ī��� ���Ό��

    #endregion

    #region Enemy

    [Header("Enemy")]
    public List<scrEnemyCombat> ActiveEnemies = new List<scrEnemyCombat>(); // ���� ���ʹ� �������� SO
    public List<Transform> Spawnpoints;

    #endregion

    public bool IsFullHP()
    {
        return ActiveEnemies[index].IsFullHp();
    }
    
    private void Start()
    {
        scrPlayerCombat.SetMaxHealth(MaxHp);
        _playerTurnEndChannel.OnValueEvent += AttackEnemy;
        SpawnEnemy();
    }

    private void OnDestroy()
    {
        _playerTurnEndChannel.OnValueEvent -= AttackEnemy;
    }

    private void AttackEnemy(bool obj)
    {
        if(obj == true) return;
        EnemiesAttack();
    }

    private void Update()
    {
        IndexSetting();
        DrawArrow();
    }

    public void HealPlayer()
    {
        scrPlayerCombat.Heal(Heal);
    }

    private void IndexSetting()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            index = Mathf.Clamp(0,0,ActiveEnemies.Count-1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            index = Mathf.Clamp(1, 0, ActiveEnemies.Count-1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            index = Mathf.Clamp(2,0,ActiveEnemies.Count-1);
    }

    private int _beforeNum=-1;
    private void DrawArrow()
    {
        if (isLoading)
        {
            _beforeNum = -1;
            index = 0;
            return;
        }
        if (_beforeNum != index)
        {
            index = Mathf.Clamp(index,0,ActiveEnemies.Count-1);
            if (_beforeNum > -1&&ActiveEnemies.Count-1>=_beforeNum)
            {
                ActiveEnemies[_beforeNum]._arrow.SetActive(false);
            }
            else
            {
                _beforeNum= -1;
            }
            ActiveEnemies[index]._arrow.SetActive(true);
            
            _beforeNum = index;
        }
    }

    public void SkipStage()
    {
        isMulti = true;
        isSkip = true;
    }
    public void MultiAttack()
    {
        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            // ������ ���� ��ü ����
            scrPlayerCombat.AttackTarget(i);
        }

        isMulti = false;
    }

    private int GetSpawnCount()
    {
        if (stageNum % 5 == 0) return 4;
        EnemySpawnPer per = enemySpawnPerList[stageNum / 5];
        int ran = Random.Range(0, 101);
        if (ran <= per.OneSpawnPer)
            return 1;
        if(ran <= per.TwoSpawnPer)
            return 2;
        if (ran <= per.ThreeSpawnPer)
            return 3;

        return 1;
    }
    
    /// <summary>
    /// ���� ���������� ��ȯ
    /// </summary>
    public void SpawnEnemy()
    {
        int spawnCount = GetSpawnCount();
        ActiveEnemies.Clear(); // ���� �� ����Ʈ �ʱ�ȭ

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = Spawnpoints[i];
            EnemySO randEnemySO;
            if (spawnCount == 4)
            {
                randEnemySO = _enemyListSo.GetBossSO(stageNum);
                i = 4;
            }
            else
            {
                randEnemySO=_enemyListSo.GetEnemyList(stageNum);
            }

            GameObject newEnemy = Instantiate(randEnemySO.prefab, spawnPoint.position, Quaternion.identity);
            if (stageNum == 25)
            {
                newEnemy.transform.position = new Vector3(spawnPoint.position.x,spawnPoint.position.y+0.23f,spawnPoint.position.z);
            }
            scrEnemyCombat enemyCombat = newEnemy.GetComponent<scrEnemyCombat>();
            if (enemyCombat != null)
            {
                enemyCombat.EnemySO = randEnemySO; // SO ����
                enemyCombat.SetMaxHealth(randEnemySO.hp); // �ִ� ü�� ����
                ActiveEnemies.Add(enemyCombat);
            }
            else
            {
                Debug.LogError("scrEnemyCombat ������Ʈ �� ã��!!!!");
            }
        }
    }

    /// <summary>
    /// ������ ���ʴ�� �÷��̾ ����
    /// </summary>
    public void EnemiesAttack()
    {
        if (ActiveEnemies.Count == 0||isLoading)
            return;

        StartCoroutine(EnemyAttackSequence());
    }

    /// <summary>
    /// �����ϰ� 1�� �� �ٽ� ����
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyAttackSequence()
    {
        foreach (var enemy in ActiveEnemies)
        {
            if (enemy != null)
            {
                enemy.Attack();
                yield return new WaitUntil(() => enemy.attackEnd);
            }
        }

        CardManager.Instance.isPlayer = true;
    }


    public void RemoveEnemy(scrEnemyCombat enemy)
    {
        ActiveEnemies.Remove(enemy);
        _beforeNum = -1;
        if (ActiveEnemies.Count == 0)
        {
            CardManager.Instance.isLoading = true;
            isLoading = true;
            if (isSkip)
            {
                if (stageNum % 5 <= 2)
                {
                    _roundEndChannel.RaiseEvent(true);
                }
                stageNum  = Mathf.Clamp(stageNum+3, 0, 25);
                isSkip = false;
            }
            else
                stageNum++;
            if (stageNum >= 26)
            {
                WinText.Instance.WinAction();
                Time.timeScale = 1;
                _fadeChannel.RaiseEvent(false);
                return; 
            }
            NextStage();
        }

        stageTxt.text = $"{stageNum} / 25";
    }

    [SerializeField] private BoolEventChannelSO _fadeChannel;
    private void NextStage()
    {
        scrPlayerCombat.MoveAniOnOff(true);
        allTrm.DOMoveX(allTrm.position.x + 4f, 5f)
            .SetEase(Ease.Linear).OnComplete(()=>
            {
                CardManager.Instance.isPlayer = true;
                PlayerHand.Instance.CardAlignmentEvent(true);
                CardManager.Instance.isLoading = false;
                isLoading = false;
                SpawnEnemy();
                _beforeNum = -1;
                if (stageNum % 5 == 1)
                {     
                    _roundEndChannel.RaiseEvent(true);
                }
                scrPlayerCombat.MoveAniOnOff(false);
            });
    }
}
