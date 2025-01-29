using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DontGetFooled
{
    public static class AssetHelper
    {
        // Sorry Rost, I copied that from your mod
        public static T LoadAsset<T>(string name) where T : Object
        {
            return (from x in Resources.FindObjectsOfTypeAll<T>()
                    where x.name.ToLower() == name.ToLower()
                    select x).First();
        }
        // Extension methods actually useful, but don't overuse them like Rost did, it will end bad...
        public static Sprite ConvertToSprite(this Texture2D texture, float offset, float pixelsPerUnit)
        {
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, offset), pixelsPerUnit);
        }
    }
}
