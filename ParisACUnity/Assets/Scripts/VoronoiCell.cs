using UnityEngine;

public class VoronoiCell : MonoBehaviour
{
    [SerializeField] private float debugSeedCubeSizeModifier = 0.3f;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.4f);
    
        var debugCubeSize = new Vector3(
            transform.parent.localScale.x * debugSeedCubeSizeModifier,
            .5f,
            transform.parent.localScale.z * debugSeedCubeSizeModifier
        );
    
        Gizmos.DrawCube(transform.position, debugCubeSize);
    }
}