using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 波次数据配置 - 定义关卡中的每一波敌人
/// </summary>
[CreateAssetMenu(fileName = "NewWaveData", menuName = "Arknights/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("波次描述")]
    public string waveName = "第一波";

    [Header("敌人生成组")]
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();

    [System.Serializable]
    public class EnemyGroup
    {
        [Tooltip("要生成的敌人预制体")]
        public GameObject enemyPrefab;

        [Tooltip("覆盖默认属性的数据（可选，如果不填则使用Prefab自带的）")]
        public EnemyData overrideData;

        [Tooltip("生成数量")]
        public int count = 1;

        [Tooltip("每两个敌人生成的间隔时间（秒）")]
        public float spawnInterval = 1.0f;

        [Tooltip("该组敌人开始前的等待时间（秒）")]
        public float preDelay = 0f;
    }
}