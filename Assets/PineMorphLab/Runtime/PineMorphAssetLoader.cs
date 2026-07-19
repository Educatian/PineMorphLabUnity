using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AdieLab.PineMorphLab
{
    public static class PineMorphAssetLoader
    {
        public static GameObject InstantiateModel(string resourcePath, string name,
            Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                throw new MissingReferenceException($"Missing PineMorph model resource: {resourcePath}");
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = scale;
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return instance;
        }

        public static Image AddHudImage(RectTransform parent, string resourcePath, Rect rect)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                throw new MissingReferenceException($"Missing PineMorph HUD resource: {resourcePath}");
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
            GameObject item = new GameObject(texture.name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            RectTransform transform = item.GetComponent<RectTransform>();
            transform.anchorMin = new Vector2(0f, 1f);
            transform.anchorMax = new Vector2(0f, 1f);
            transform.pivot = new Vector2(0f, 1f);
            transform.anchoredPosition = new Vector2(rect.x, -rect.y);
            transform.sizeDelta = new Vector2(rect.width, rect.height);
            Image image = item.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }
    }
}
