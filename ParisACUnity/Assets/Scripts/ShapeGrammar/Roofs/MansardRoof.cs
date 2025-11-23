using System.Collections.Generic;
using UnityEngine;

public class MansardRoof : Roof
{
    private int maxSlopeLayers;
    private int amountSlopeLayers;
    private int currentSlopeLayer;

    private int currentWidth;

    private void Init(int maxSlopeLayers, int amountSlopeLayers, int currentSlopeLayer, int currentWidth)
    {
        this.maxSlopeLayers = maxSlopeLayers;
        this.amountSlopeLayers = amountSlopeLayers;
        this.currentSlopeLayer = currentSlopeLayer;

        this.currentWidth = currentWidth;
    }

    protected override void Execute()
    {
        House house = Root.GetComponent<House>();
        HouseSettingsData houseSettings = house.SettingsDataCopy;

        if (!house.hasFirstRoofGenerated)
        {
            int maxSlopeLayers = Mathf.FloorToInt(houseSettings.Width / 2.0f);
            int randomAmountSlopeLayers = RandomInt(1, maxSlopeLayers);
            Init(maxSlopeLayers, randomAmountSlopeLayers, 1, houseSettings.Width);
            house.hasFirstRoofGenerated = true;
        }

        if (currentWidth == 1 || currentSlopeLayer > amountSlopeLayers) // Generate only flat piece
        {
            for (int i = 0; i < currentWidth; i++)
            {
                Vector3 _spawnDirection = Vector3.right;
                Row roofRow = CreateSymbol<Row>("RoofRow", _spawnDirection * (i - (currentWidth - 1) / 2f));

                roofRow.Init(new List<GameObject>() { houseSettings.roofTop }, houseSettings.Depth, Vector3.forward);
                roofRow.Generate();
            }
        }
        else // Generate roofs from both sides
        {
            // Create two roof rows from both sides:
            for (int i = 0; i < 2; i++)
            {
                Vector3 localPosition = new Vector3();
                switch (i)
                {
                    case 0:
                        localPosition = new Vector3(-(currentWidth - 1) * 0.5f, 0, 0); // right
                        break;
                    case 1:
                        localPosition = new Vector3((currentWidth - 1) * 0.5f, 0, 0); // left
                        break;
                }

                //localPosition += localPosition.normalized * 0.05f;

                Quaternion localRotation = Quaternion.Euler(0, 90 * (i == 0 ? -1 : 1), 0);
                Row roofRow = CreateSymbol<Row>("RoofRow", localPosition, localRotation);

                Vector3 dirToSpawnPrefabs = localRotation * Vector3.forward;
                roofRow.Init(houseSettings.getRoofTypes(), houseSettings.Depth, dirToSpawnPrefabs);

                roofRow.Generate();
            }

            if (currentWidth > 2) // Generate walls in between if space
            {
                for (int i = 0; i < 3; i += 2)
                {
                    Vector3 localPosition = new Vector3();
                    switch (i)
                    {
                        case 0:
                            localPosition = new Vector3(0, 0, (houseSettings.Depth - 1) * 0.5f); // front
                            break;
                        case 2:
                            localPosition = new Vector3(0, 0, -(houseSettings.Depth - 1) * 0.5f); // back
                            break;
                    }

                    Quaternion localRotation = Quaternion.Euler(0, i * 90, 0);
                    Row roofRow = CreateSymbol<Row>("RoofRow", localPosition, localRotation);

                    Vector3 dirToSpawnPrefabs = localRotation * Vector3.right;
                    roofRow.Init(new List<GameObject>() { houseSettings.wallType, houseSettings.windowType },
                        currentWidth - 2, dirToSpawnPrefabs);

                    roofRow.Generate();
                }
            }

            MansardRoof roofGen =
                CreateSymbol(houseSettings.roofGenType, new Vector3(0, 1, 0)) as MansardRoof;
            roofGen.Init(maxSlopeLayers, amountSlopeLayers, currentSlopeLayer + 1, currentWidth - 2);
            roofGen.Generate();
        }
    }
}