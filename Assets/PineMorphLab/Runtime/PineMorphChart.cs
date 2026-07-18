using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphChart : MaskableGraphic
    {
        private readonly List<PineMorphResult> results = new List<PineMorphResult>();

        public void SetResults(IReadOnlyList<PineMorphResult> source)
        {
            results.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                results.Add(source[i]);
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            AddRect(vh, r, new Color(0.035f, 0.07f, 0.08f, 0.96f));
            for (int i = 1; i < 4; i++)
            {
                float y = Mathf.Lerp(r.yMin + 16f, r.yMax - 20f, i / 4f);
                AddLine(vh, new Vector2(r.xMin + 18f, y), new Vector2(r.xMax - 16f, y), 1f,
                    new Color(0.22f, 0.34f, 0.34f, 0.55f));
            }

            float bottom = r.yMin + 18f;
            float top = r.yMax - 22f;
            AddLine(vh, new Vector2(r.xMin + 18f, Mathf.Lerp(bottom, top, 45f / 120f)),
                new Vector2(r.xMax - 16f, Mathf.Lerp(bottom, top, 45f / 120f)), 1.5f,
                new Color(0.20f, 0.78f, 0.71f, 0.5f));
            AddLine(vh, new Vector2(r.xMin + 18f, Mathf.Lerp(bottom, top, 75f / 120f)),
                new Vector2(r.xMax - 16f, Mathf.Lerp(bottom, top, 75f / 120f)), 1.5f,
                new Color(0.20f, 0.78f, 0.71f, 0.5f));
            AddLine(vh, new Vector2(r.xMin + 18f, Mathf.Lerp(bottom, top, 180f / 320f)),
                new Vector2(r.xMax - 16f, Mathf.Lerp(bottom, top, 180f / 320f)), 1.5f,
                new Color(0.98f, 0.63f, 0.20f, 0.5f));
            AddLine(vh, new Vector2(r.xMin + 18f, Mathf.Lerp(bottom, top, 3.5f / 8f)),
                new Vector2(r.xMax - 16f, Mathf.Lerp(bottom, top, 3.5f / 8f)), 1.5f,
                new Color(0.91f, 0.32f, 0.28f, 0.5f));

            if (results.Count == 0)
            {
                return;
            }

            Color angle = new Color(0.20f, 0.78f, 0.71f, 1f);
            Color response = new Color(0.98f, 0.63f, 0.20f, 1f);
            Color stress = new Color(0.91f, 0.32f, 0.28f, 1f);
            for (int i = 0; i < results.Count; i++)
            {
                float x = Mathf.Lerp(r.xMin + 26f, r.xMax - 24f, i / 4f);
                float angleY = Mathf.Lerp(bottom, top, Mathf.Clamp01(results[i].OpeningAngleDeg / 120f));
                float responseY = Mathf.Lerp(bottom, top, Mathf.Clamp01(results[i].ResponseTimeSeconds / 320f));
                float stressY = Mathf.Lerp(bottom, top, Mathf.Clamp01(results[i].PeakStressMPa / 8f));
                AddDisc(vh, new Vector2(x - 7f, angleY), 4.2f, angle);
                AddDisc(vh, new Vector2(x, responseY), 4.2f, response);
                AddDisc(vh, new Vector2(x + 7f, stressY), 4.2f, stress);
                if (i > 0)
                {
                    float previousX = Mathf.Lerp(r.xMin + 26f, r.xMax - 24f, (i - 1f) / 4f);
                    PineMorphResult previous = results[i - 1];
                    AddLine(vh, new Vector2(previousX - 7f,
                            Mathf.Lerp(bottom, top, Mathf.Clamp01(previous.OpeningAngleDeg / 120f))),
                        new Vector2(x - 7f, angleY), 2.5f, angle);
                    AddLine(vh, new Vector2(previousX,
                            Mathf.Lerp(bottom, top, Mathf.Clamp01(previous.ResponseTimeSeconds / 320f))),
                        new Vector2(x, responseY), 2.5f, response);
                    AddLine(vh, new Vector2(previousX + 7f,
                            Mathf.Lerp(bottom, top, Mathf.Clamp01(previous.PeakStressMPa / 8f))),
                        new Vector2(x + 7f, stressY), 2.5f, stress);
                }
            }
        }

        private static void AddRect(VertexHelper vh, Rect rect, Color color)
        {
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.up);
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color, Vector2.one);
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color, Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddLine(VertexHelper vh, Vector2 from, Vector2 to, float width, Color color)
        {
            Vector2 normal = new Vector2(-(to.y - from.y), to.x - from.x).normalized * width * 0.5f;
            int start = vh.currentVertCount;
            vh.AddVert(from - normal, color, Vector2.zero);
            vh.AddVert(from + normal, color, Vector2.up);
            vh.AddVert(to + normal, color, Vector2.one);
            vh.AddVert(to - normal, color, Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddDisc(VertexHelper vh, Vector2 center, float radius, Color color)
        {
            const int sides = 12;
            int centerIndex = vh.currentVertCount;
            vh.AddVert(center, color, new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(center + direction * radius, color, direction * 0.5f + Vector2.one * 0.5f);
                if (i > 0)
                {
                    vh.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
                }
            }
        }
    }
}
