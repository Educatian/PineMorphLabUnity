using UnityEngine;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphInspectable : MonoBehaviour
    {
        private Renderer[] renderers;
        private MaterialPropertyBlock propertyBlock;

        public string Label { get; private set; }
        public string Detail { get; private set; }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        public void Configure(string label, string detail, params Renderer[] targetRenderers)
        {
            Label = label;
            Detail = detail;
            renderers = targetRenderers;
        }

        public void SetSelected(bool selected)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer target in renderers)
            {
                if (target == null || target.sharedMaterial == null)
                {
                    continue;
                }

                target.GetPropertyBlock(propertyBlock);
                Color baseColor = target.sharedMaterial.color;
                propertyBlock.SetColor("_Color", selected
                    ? Color.Lerp(baseColor, Color.white, 0.42f)
                    : baseColor);
                target.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
