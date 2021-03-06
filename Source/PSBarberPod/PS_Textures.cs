﻿using System;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    // Token: 0x02000069 RID: 105
    [StaticConstructorOnStartup]
    public static class Textures
    {
        // Token: 0x0600043A RID: 1082 RVA: 0x0001611D File Offset: 0x0001431D
        static Textures()
        {
            Textures.LoadTextures();
        }

        // Token: 0x1700014C RID: 332
        // (get) Token: 0x0600043B RID: 1083 RVA: 0x00016124 File Offset: 0x00014324
        public static bool Loaded
        {
            get
            {
                return Textures.loaded;
            }
        }

        // Token: 0x0600043C RID: 1084 RVA: 0x0001612B File Offset: 0x0001432B
        public static void Reset()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                Textures.LoadTextures();
            });
        }

        // Token: 0x0600043D RID: 1085 RVA: 0x00016154 File Offset: 0x00014354
        private static void LoadTextures()
        {
            Textures.loaded = false;
            Textures.Button = ContentFinder<Texture2D>.Get("UI/testure", true);
            //Textures.TextureAlternateRow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
            //Textures.TextureSkillBarFill = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
            Textures.loaded = true;
        }

        // Token: 0x0400025B RID: 603
        private static bool loaded;

        // Token: 0x0400025C RID: 604
        public static Texture2D Button;
    }
}
