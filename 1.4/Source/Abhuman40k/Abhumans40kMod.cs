using HarmonyLib;
using UnityEngine;
using Verse;


namespace Abhumans40k
{
    public class Abhumans40kMod : Mod
    {
        public static Harmony harmony;

        public Abhumans40kMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("Abhumans40k.Mod");
            harmony.PatchAll();
        }
    }
}