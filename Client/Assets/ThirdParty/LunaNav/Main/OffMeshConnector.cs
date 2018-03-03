using UnityEngine;

public class OffMeshConnector : MonoBehaviour
{
    public Vector3 StartPosition { get { return gameObject.transform.position; } }
    public Vector3 EndPosition;
    public float Radius = 0.6f;
    public bool Bidirectional = true;
    public short Area;
    public int Flags;
    public long Id;

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(StartPosition, Radius);
        Gizmos.DrawWireSphere(EndPosition, Radius);
        DrawArc(0.25f, new Color(192f/255f, 0, 128f/255f, 192f/255f));
    }

    private void DrawArc(float height, Color color)
    {
        Color old = Gizmos.color;
        Gizmos.color = color;
        int numArcPoints = 8;
        float pad = 0.05f;
        float arcPtsScale = (1.0f - pad*2)/(float) numArcPoints;
        float dx = EndPosition.x - StartPosition.x;
        float dy = EndPosition.y - StartPosition.y;
        float dz = EndPosition.z - StartPosition.z;

        float len = Mathf.Sqrt(dx*dx + dy*dy + dz*dz);
        Vector3 prev;
        Vector3 dVec = new Vector3(dx, dy, dz);
        EvalArc(StartPosition, dVec, len * height, pad, out prev);

        for (int i = 1; i <= numArcPoints; i++)
        {
            float u = pad + i*arcPtsScale;
            Vector3 pt;
            EvalArc(StartPosition, dVec, len * height, u, out pt);
            Gizmos.DrawLine(prev, pt);
            prev = pt;
        }

        if (Bidirectional)
        {
            
        }

        Gizmos.color = old;
    }

    private void EvalArc(Vector3 p0, Vector3 pd, float height, float u, out Vector3 prev)
    {
        prev = new Vector3(p0.x + pd.x*u, p0.y + pd.y*u + height*(1-(u*2-1)*(u*2-1)), p0.z + pd.z*u);
    }
}