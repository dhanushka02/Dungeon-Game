using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// Visual pieces have no colliders, so they cannot block the key or player.
internal sealed class BarrelImpactEffects : MonoBehaviour
{
    private Transform[] pieces;
    private Vector3[] starts;
    private Vector3[] velocities;
    private Vector3[] sizes;
    private Vector3[] spins;
    private float floorY;

    public static void Cut(Transform barrel, Vector3 point, Vector3 normal, Material wood)
    {
        GameObject cut = Piece("Axe cut", wood, new Color(0.12f, 0.065f, 0.025f));
        // The barrel's convex collider sits slightly inside its visible wood panels.
        cut.transform.SetPositionAndRotation(point + normal * 0.035f,
            Quaternion.LookRotation(normal) * Quaternion.Euler(0f, 0f, -22f));
        cut.transform.localScale = new Vector3(0.025f, 0.17f, 0.004f);
        cut.transform.SetParent(barrel, true);
    }

    public static void Splinters(Vector3 point, Vector3 normal, float ground, Material wood, bool breaking)
    {
        int count = breaking ? 18 : 7;
        var effect = CreateEffect(breaking ? "Barrel breaking splinters" : "Barrel impact splinters", count, ground);
        for (int i = 0; i < count; i++)
        {
            GameObject chip = Piece("Wood splinter", wood,
                Color.Lerp(new Color(0.35f, 0.16f, 0.045f), new Color(0.8f, 0.48f, 0.18f), Random.value));
            chip.transform.SetParent(effect.transform);
            chip.transform.position = point + Random.insideUnitSphere * (breaking ? 0.18f : 0.025f);
            chip.transform.rotation = Random.rotation;
            chip.transform.localScale = new Vector3(Random.Range(0.015f, 0.04f),
                Random.Range(0.07f, breaking ? 0.28f : 0.13f), 0.018f);
            effect.pieces[i] = chip.transform;
            effect.starts[i] = chip.transform.position;
            effect.sizes[i] = chip.transform.localScale;
            effect.velocities[i] = normal * Random.Range(0.45f, 1.1f)
                + Random.insideUnitSphere * (breaking ? 1.5f : 0.6f) + Vector3.up * Random.Range(0.7f, 1.8f);
            effect.spins[i] = Random.insideUnitSphere * 550f;
        }
        effect.StartCoroutine(effect.Scatter());
    }

    public static void WallFragments(Bounds bounds, Vector3 normal, Material wood)
    {
        const int columns = 2;
        const int rows = 6;
        var effect = CreateEffect("Wooden wall fragments", columns * rows, bounds.min.y + 0.035f);
        Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
        float width = Vector3.Dot(bounds.size, new Vector3(Mathf.Abs(right.x), Mathf.Abs(right.y), Mathf.Abs(right.z)));
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            int i = row * columns + column;
            GameObject plank = Piece("Broken wall plank", wood, Color.white * Random.Range(0.65f, 0.95f));
            plank.transform.SetParent(effect.transform);
            plank.transform.position = bounds.center + right * (((column + 0.5f) / columns - 0.5f) * width)
                + Vector3.up * (((row + 0.5f) / rows - 0.5f) * bounds.size.y) + normal * 0.05f;
            plank.transform.rotation = Quaternion.LookRotation(normal);
            plank.transform.localScale = new Vector3(width / columns * 0.9f, bounds.size.y / rows * 0.8f, 0.08f);
            effect.pieces[i] = plank.transform;
            effect.starts[i] = plank.transform.position;
            effect.sizes[i] = plank.transform.localScale;
            effect.velocities[i] = normal * Random.Range(0.5f, 1.3f) + right * Random.Range(-0.8f, 0.8f)
                + Vector3.up * Random.Range(0.6f, 1.4f);
            effect.spins[i] = Random.insideUnitSphere * 250f;
        }
        effect.StartCoroutine(effect.Scatter());
    }

    private static BarrelImpactEffects CreateEffect(string label, int count, float ground)
    {
        var effect = new GameObject(label).AddComponent<BarrelImpactEffects>();
        effect.pieces = new Transform[count];
        effect.starts = new Vector3[count];
        effect.velocities = new Vector3[count];
        effect.sizes = new Vector3[count];
        effect.spins = new Vector3[count];
        effect.floorY = ground + 0.025f;
        return effect;
    }

    private static GameObject Piece(string label, Material wood, Color color)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = label;
        Collider collider = piece.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(collider);
        Renderer renderer = piece.GetComponent<Renderer>();
        if (wood != null) renderer.sharedMaterial = wood;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        var properties = new MaterialPropertyBlock();
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        renderer.SetPropertyBlock(properties);
        return piece;
    }

    private IEnumerator Scatter()
    {
        const float lifetime = 2.5f;
        for (float age = 0f; age < lifetime; age += Time.deltaTime)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                Vector3 position = starts[i] + velocities[i] * age + Vector3.down * (4.9f * age * age);
                bool landed = position.y <= floorY;
                position.y = Mathf.Max(position.y, floorY);
                if (!landed)
                {
                    pieces[i].position = position;
                    pieces[i].Rotate(spins[i] * Time.deltaTime, Space.Self);
                }
                else
                {
                    Vector3 settled = pieces[i].position;
                    settled.y = floorY;
                    pieces[i].position = settled;
                }
                pieces[i].localScale = sizes[i] * Mathf.Clamp01((lifetime - age) / 0.4f);
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
