using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
    public class EditorSpriteAnimator
    {
        private int frameId;
        private Sprite[] _sprites;
        private Rect positionInAtlas;

        private double lastUpdateTime;
        private float animSpeed = 0.08f;

        public EditorSpriteAnimator(Sprite[] sprites)
        {
            _sprites = sprites;
            positionInAtlas = new Rect();
        }

        private void UpdateAnimation()
        {
            if (EditorApplication.timeSinceStartup < (lastUpdateTime + animSpeed))
            {
                return;
            }

            lastUpdateTime = EditorApplication.timeSinceStartup;

            frameId++;
            if (frameId >= _sprites.Length)
            {
                frameId = 0;
            }

            positionInAtlas.x = _sprites[frameId].textureRect.x / _sprites[frameId].texture.width;
            positionInAtlas.width = _sprites[frameId].textureRect.width / _sprites[frameId].texture.width;

            positionInAtlas.y = _sprites[frameId].textureRect.y / _sprites[frameId].texture.height;
            positionInAtlas.height = _sprites[frameId].textureRect.height / _sprites[frameId].texture.height;
        }

        public void Draw(Rect position)
        {
            UpdateAnimation();
            GUI.DrawTextureWithTexCoords(position, _sprites[frameId].texture, positionInAtlas, true);
        }
    }
}