using UnityEngine;

namespace Assets.Helpers
{
    public static class OutlineHelper
    {
        public static void AddDefaultOutline(GameObject go)
        {
            var outline = go.AddComponent<Outline>();
            outline.OutlineWidth = 0.25f;
            outline.OutlineMode = Outline.Mode.OutlineHidden;
            outline.OutlineColor = ColorConstants.WhiteAccent;
        }
    }
}