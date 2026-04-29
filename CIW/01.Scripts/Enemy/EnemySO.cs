using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    None,
    Cat,
    KingCat
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "SO/EnemySO")]
public class EnemySO : ScriptableObject
{
    public string id;
    public float hp;
    public float damage;
    public GameObject prefab;
}
