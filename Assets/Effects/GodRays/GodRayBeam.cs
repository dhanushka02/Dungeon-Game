using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class GodRayBeam : MonoBehaviour
{
    [Min(0.1f)] public float length = 5.4f;
    [Min(0.01f)] public float topRadiusX = 0.18f;
    [Min(0.01f)] public float topRadiusZ = 0.08f;
    [Min(0.02f)] public float bottomRadiusX = 1.4f;
    [Min(0.02f)] public float bottomRadiusZ = 0.6f;
    [Range(8, 64)] public int sides = 32;

    private Mesh generatedMesh;

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnValidate()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
        {
            return;
        }

        if (generatedMesh == null)
        {
            generatedMesh = new Mesh
            {
                name = "God Ray Beam (Generated)",
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        else
        {
            generatedMesh.Clear();
        }

        int ringSize = sides + 1;
        var vertices = new Vector3[ringSize * 2];
        var normals = new Vector3[ringSize * 2];
        var uv = new Vector2[ringSize * 2];
        var triangles = new int[sides * 6];
        float halfLength = length * 0.5f;
        float averageSlope = ((bottomRadiusX + bottomRadiusZ) - (topRadiusX + topRadiusZ)) * 0.5f / length;

        for (int index = 0; index <= sides; index++)
        {
            float fraction = index / (float)sides;
            float angle = fraction * Mathf.PI * 2f;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            int topIndex = index;
            int bottomIndex = ringSize + index;

            vertices[topIndex] = new Vector3(cosine * topRadiusX, halfLength, sine * topRadiusZ);
            vertices[bottomIndex] = new Vector3(cosine * bottomRadiusX, -halfLength, sine * bottomRadiusZ);

            Vector3 normal = new Vector3(cosine, averageSlope, sine).normalized;
            normals[topIndex] = normal;
            normals[bottomIndex] = normal;
            uv[topIndex] = new Vector2(fraction, 0f);
            uv[bottomIndex] = new Vector2(fraction, 1f);
        }

        for (int side = 0; side < sides; side++)
        {
            int triangle = side * 6;
            int top = side;
            int nextTop = side + 1;
            int bottom = ringSize + side;
            int nextBottom = ringSize + side + 1;

            triangles[triangle] = top;
            triangles[triangle + 1] = nextTop;
            triangles[triangle + 2] = bottom;
            triangles[triangle + 3] = nextTop;
            triangles[triangle + 4] = nextBottom;
            triangles[triangle + 5] = bottom;
        }

        generatedMesh.vertices = vertices;
        generatedMesh.normals = normals;
        generatedMesh.uv = uv;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();
        filter.sharedMesh = generatedMesh;
    }

    private void OnDestroy()
    {
        if (generatedMesh == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedMesh);
        }
        else
        {
            DestroyImmediate(generatedMesh);
        }
    }
}
