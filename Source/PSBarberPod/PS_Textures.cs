using UnityEngine;
using Verse;

namespace PS_BarberPod;

[StaticConstructorOnStartup]
public static class Textures
{
    static Textures()
    {
        LoadTextures();
    }

    public static bool Loaded { get; private set; }

    public static void Reset()
    {
        LongEventHandler.ExecuteWhenFinished(LoadTextures);
    }

    private static void LoadTextures()
    {
        Loaded = false;
        ContentFinder<Texture2D>.Get("UI/testure");
        //Textures.TextureAlternateRow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
        //Textures.TextureSkillBarFill = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
        Loaded = true;
    }
}