using UnityEngine;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphRibbon : MonoBehaviour
    {
        private const int Segments = 40;
        private const float VisualLength = 4.6f;
        private const float VisualWidth = 1.55f;
        private const float VisualThickness = 0.24f;

        private Mesh activeMesh;
        private Mesh passiveMesh;
        private MeshRenderer activeRenderer;
        private MeshRenderer passiveRenderer;

        public void Initialize(Material activeMaterial, Material passiveMaterial)
        {
            activeMesh = new Mesh { name = "Active hygroscopic layer" };
            passiveMesh = new Mesh { name = "Passive structural layer" };
            activeRenderer = CreateLayer("Active Layer", activeMesh, activeMaterial);
            passiveRenderer = CreateLayer("Passive Layer", passiveMesh, passiveMaterial);
            if (activeRenderer.sharedMaterial.HasProperty("_FiberStrength"))
            {
                activeRenderer.sharedMaterial.SetFloat("_FiberStrength", 0.72f);
            }
            SetMorph(PineMorphInput.Baseline, PineMorphPhysics.Evaluate(PineMorphInput.Baseline), 0f);
        }

        public void SetMorph(PineMorphInput input, PineMorphResult result, float progress)
        {
            float activeThickness = VisualThickness * input.ActiveLayerFraction;
            float passiveThickness = VisualThickness - activeThickness;
            float bendRadians = Mathf.Clamp(result.OpeningAngleDeg * Mathf.Deg2Rad * progress, 0f, 2.35f);
            BuildLayer(activeMesh, bendRadians, activeThickness * 0.5f, activeThickness);
            BuildLayer(passiveMesh, bendRadians, -passiveThickness * 0.5f, passiveThickness);
            if (activeRenderer.sharedMaterial.HasProperty("_FiberAngle"))
            {
                activeRenderer.sharedMaterial.SetFloat("_FiberAngle", input.FiberAngleDeg);
            }
        }

        public void SetEmphasis(bool activeLayer)
        {
            SetGlossiness(activeRenderer.material, activeLayer ? 0.72f : 0.45f);
            SetGlossiness(passiveRenderer.material, activeLayer ? 0.35f : 0.68f);
        }

        private static void SetGlossiness(Material material, float value)
        {
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", value);
            }
        }

        private MeshRenderer CreateLayer(string objectName, Mesh mesh, Material material)
        {
            GameObject layer = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
            layer.transform.SetParent(transform, false);
            layer.GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = layer.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static void BuildLayer(Mesh mesh, float totalAngle, float normalOffset, float thickness)
        {
            int ringCount = Segments + 1;
            Vector3[] vertices = new Vector3[ringCount * 4];
            Vector3[] normals = new Vector3[ringCount * 4];
            Vector2[] uv = new Vector2[ringCount * 4];
            int[] triangles = new int[Segments * 24];

            for (int i = 0; i < ringCount; i++)
            {
                float t = i / (float)Segments;
                float theta = totalAngle * t;
                Vector3 center;
                if (totalAngle < 0.0001f)
                {
                    center = new Vector3(0f, 0f, VisualLength * t);
                }
                else
                {
                    float radius = VisualLength / totalAngle;
                    center = new Vector3(0f, radius * (1f - Mathf.Cos(theta)), radius * Mathf.Sin(theta));
                }

                Vector3 normal = new Vector3(0f, Mathf.Cos(theta), -Mathf.Sin(theta));
                Vector3 layerCenter = center + normal * normalOffset;
                Vector3 halfWidth = Vector3.right * (VisualWidth * 0.5f);
                Vector3 halfDepth = normal * (thickness * 0.5f);
                int v = i * 4;
                vertices[v] = layerCenter - halfWidth - halfDepth;
                vertices[v + 1] = layerCenter + halfWidth - halfDepth;
                vertices[v + 2] = layerCenter - halfWidth + halfDepth;
                vertices[v + 3] = layerCenter + halfWidth + halfDepth;
                normals[v] = -normal;
                normals[v + 1] = -normal;
                normals[v + 2] = normal;
                normals[v + 3] = normal;
                uv[v] = new Vector2(0f, t);
                uv[v + 1] = new Vector2(1f, t);
                uv[v + 2] = new Vector2(0f, t);
                uv[v + 3] = new Vector2(1f, t);
            }

            int ti = 0;
            for (int i = 0; i < Segments; i++)
            {
                int a = i * 4;
                int b = (i + 1) * 4;
                AddQuad(triangles, ref ti, a, b, a + 1, b + 1);
                AddQuad(triangles, ref ti, a + 2, a + 3, b + 2, b + 3);
                AddQuad(triangles, ref ti, a, a + 2, b, b + 2);
                AddQuad(triangles, ref ti, a + 1, b + 1, a + 3, b + 3);
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        private static void AddQuad(int[] triangles, ref int index, int a, int b, int c, int d)
        {
            triangles[index++] = a;
            triangles[index++] = b;
            triangles[index++] = c;
            triangles[index++] = c;
            triangles[index++] = b;
            triangles[index++] = d;
        }
    }
}
