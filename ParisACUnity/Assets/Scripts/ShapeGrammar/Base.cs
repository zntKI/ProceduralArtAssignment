using System.Collections.Generic;
using UnityEngine;

public class Base : Shape
{
    [SerializeField] [Min(1)] private int remainingHeight;

    public void Init(int remainingHeight)
    {
        this.remainingHeight = remainingHeight;
    }

    protected override void Execute()
    {
        House house = Root.GetComponent<House>();
        HouseSettingsData houseSettings = house.SettingsDataCopy;

        // Generate middle layer
        BaseLayer baseLayer = CreateSymbol<BaseLayer>("BaseLayer");

        List<GameObject> prefabsToUse;
        if (!house.hasFirstBaseGenerated)
        {
            prefabsToUse = new List<GameObject>()
                { houseSettings.wallType, houseSettings.doorType, houseSettings.windowType };
            house.hasFirstBaseGenerated = true;
        }
        else
            prefabsToUse = new List<GameObject>() { houseSettings.wallType, houseSettings.windowType };

        baseLayer.Init(prefabsToUse);

        baseLayer.Generate();

        if (remainingHeight == 0) // Generate roof layer
        {
            Roof roofGen = CreateSymbol(houseSettings.roofGenType, new Vector3(0, 1, 0)) as Roof;
            roofGen.Generate();
        }
        else // Generate another structure
        {
            Base basee = CreateSymbol<Base>("Base", new Vector3(0, 1, 0));
            basee.Init(remainingHeight - 1);
            basee.Generate();
        }
    }
}