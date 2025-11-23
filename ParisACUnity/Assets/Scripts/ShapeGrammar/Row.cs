using System.Collections.Generic;
using UnityEngine;

public class Row : Shape
{
    [SerializeField]
    private List<GameObject> prefabs;

    [SerializeField]
    [Min(0)]
    private List<int> facadePattern;

    private int _spawnAmount;
    private Vector3 _spawnDirection;

    public void Init(List<GameObject> prefabsToUse, int numPrefabsToSpawn, Vector3 dirToSpawnPrefabs)
    {
        prefabs = new List<GameObject>(prefabsToUse);
        _spawnAmount = numPrefabsToSpawn;
        _spawnDirection = dirToSpawnPrefabs;

        facadePattern = new List<int>(_spawnAmount);
        for (int i = 0; i < _spawnAmount; i++)
        {
            int index = RandomInt(prefabs.Count);
            facadePattern.Add(index);
        }
    }

    protected override void Execute()
    {
        for (int i = 0; i < _spawnAmount; i++)
        {
            int index = facadePattern[i % facadePattern.Count];
            index = Mathf.Clamp(index, 0, prefabs.Count - 1);

            SpawnPrefab(prefabs[index],
                _spawnDirection * (i - (_spawnAmount - 1) / 2f) // position offset from center
            );
        }
    }
}