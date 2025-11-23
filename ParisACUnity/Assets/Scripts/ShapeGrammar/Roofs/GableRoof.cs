using System.Collections.Generic;
using UnityEngine;

public class GableRoof : Roof
{
    [SerializeField] [Min(1)] protected int currentWidth;
    [SerializeField] [Min(1)] protected int currentHeight;

    private void Init(int remainingWidth, int remainingHeight)
    {
        this.currentWidth = remainingWidth;
        this.currentHeight = remainingHeight;
    }

    protected override void Execute()
    {
        House house = Root.GetComponent<House>();
        HouseSettingsData houseSettings = house.SettingsDataCopy;

        if (!house.hasFirstRoofGenerated)
        {
            Init(houseSettings.Width, Mathf.CeilToInt(houseSettings.Width / 2.0f));
            house.hasFirstRoofGenerated = true;
        }

        if (currentHeight == 1 && currentWidth == 1) // Generate only gable piece
        {
            Row roofRow = CreateSymbol<Row>("RoofRow");

            roofRow.Init(new List<GameObject>() { houseSettings.roofTop }, houseSettings.Depth, Vector3.forward);
            roofRow.Generate();
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

            int nextWidth = currentWidth - 2, nextHeight = currentHeight - 1;
            if (nextWidth != 0 && nextHeight != 0) // If not last, create and generate next Roof
            {
                GableRoof roofGen =
                    CreateSymbol(houseSettings.roofGenType, new Vector3(0, 1, 0)) as GableRoof;
                roofGen.Init(nextWidth, nextHeight);
                roofGen.Generate();
            }
        }
    }
}