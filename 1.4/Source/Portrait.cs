using System;
using UnityEngine;
using Verse;

namespace PortraitsOfTheRim
{
    public class Portrait
    {
        public Pawn pawn;
        private Texture texture;
        public Texture PortraitTexture
        {
            get
            {
                if (texture is null)
                {
                    texture = CreateTexture();
                }
                return texture;
            }
        }

        private Texture CreateTexture()
        {
            throw new NotImplementedException();
            foreach (var layer in Core.layers)
            {
                if (Core.portraitElements.TryGetValue(layer, out var elements))
                {

                }
            }
        }
    }


}
