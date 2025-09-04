using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat System/Create a new attack")]
//ScriptableObject类：ScriptableObject是 Unity 中的一种特殊类，用于创建可保存数据的资产文件。
//与普通 C# 类不同，它的数据可以独立于场景存在，方便在多个对象间共享和复用。
public class AttackData : ScriptableObject
{

    [field: SerializeField] public string AnimName { get; private set; }
    [field: SerializeField] public AttackHitbox HitBoxToUse { get; private set; }
    [field: SerializeField] public float ImpactStartTime { get; private set; }
    [field: SerializeField] public float ImpactEndTime { get; private set; }
    
}

public enum AttackHitbox {LeftHand, RightHand, LeftFoot, RightFoot, Sword }