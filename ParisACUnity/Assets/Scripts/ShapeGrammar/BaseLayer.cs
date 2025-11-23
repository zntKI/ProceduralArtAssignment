using System.Collections.Generic;
using UnityEngine;

public class BaseLayer : Shape
{
    private List<GameObject> _prefabs;

    public void Init(List<GameObject> prefabs)
    {
        _prefabs = new List<GameObject>(prefabs);
    }

    protected override void Execute()
    {
        HouseSettingsData houseSettings = Root.GetComponent<House>().SettingsDataCopy;

        // Create four walls:
        for (int i = 0; i < 4; i++)
        {
            Vector3 localPosition = new Vector3();
            switch (i)
            {
                case 0:
                    localPosition = new Vector3(0, 0, (houseSettings.Depth - 1) * 0.5f); // front
                    break;
                case 1:
                    localPosition = new Vector3(-(houseSettings.Width - 1) * 0.5f, 0, 0); // right
                    break;
                case 2:
                    localPosition = new Vector3(0, 0, -(houseSettings.Depth - 1) * 0.5f); // back
                    break;
                case 3:
                    localPosition = new Vector3((houseSettings.Width - 1) * 0.5f, 0, 0); // left
                    break;
            }

            localPosition += localPosition.normalized * 0.05f;

            // Offset aligned with the local rotation
            //Vector3 localPosition = transform.InverseTransformDirection(worldPosition);

            Quaternion localRotation = Quaternion.identity;
            List<GameObject> prefabsToUse = null;
            int numOfPrefabsToSpawn;
            Vector3 dirToSpawnPrefabs;
            if (i % 2 == 0)
            {
                localRotation = Quaternion.Euler(0, i * 90, 0);

                dirToSpawnPrefabs = localRotation * Vector3.right;

                prefabsToUse = new List<GameObject>(_prefabs);
                numOfPrefabsToSpawn = houseSettings.Width;
            }
            else
            {
                localRotation = Quaternion.Euler(0, i * -90, 0);

                dirToSpawnPrefabs = localRotation * Vector3.forward;

                prefabsToUse = new List<GameObject>() { _prefabs[0] };
                numOfPrefabsToSpawn = houseSettings.Depth;
            }

            Row row = CreateSymbol<Row>("BaseRow", localPosition, localRotation);
            row.Init(prefabsToUse, numOfPrefabsToSpawn, dirToSpawnPrefabs);

            row.Generate();
        }
    }
}