using UnityEngine;

public class Stack : Shape
{
    public GameObject prefab;
    public int HeightRemaining;

    public void Initialize(GameObject pPrefab, int pHeightRemaining)
    {
        prefab = pPrefab;
        HeightRemaining = pHeightRemaining;
    }

    protected override void Execute()
    {
        // Spawn the (box) prefab as child of this game object:
        // (Optional parameters: localPosition, localRotation, alternative parent)
        GameObject box = SpawnPrefab(prefab);

        // Example: fat box:
        //box.transform.localScale = new Vector3(3, 3, 3);

        if (HeightRemaining > 0)
        {
            Stack newStack = null;
            
            if (HeightRemaining > 8)
            {
                HeightRemaining = 8;
            }

            for (int i = 0; i < 2; i++)
            {
                float multiplier = i % 2 == 0 ? -1f : 1f;
                newStack = CreateSymbol<Stack>("stack", new Vector3(multiplier * 0.25f, box.transform.localScale.y, 0f),
                    Quaternion.Euler(0f, 90f, -multiplier * 45f));
                newStack.Initialize(prefab, HeightRemaining - 1);
                newStack.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                newStack.Generate(buildDelay);
            }
            /**/
        }
    }
}