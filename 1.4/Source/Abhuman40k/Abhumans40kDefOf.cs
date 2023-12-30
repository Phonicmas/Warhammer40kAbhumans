using RimWorld;
using Verse;

namespace Abhumans40k
{
    [DefOf]
    public static class Abhumans40kDefOf
    {
        public static GeneDef BEWH_SquatGrudgy;
        public static GeneDef BEWH_NightsiderOtherwordly;

        public static GeneDef BEWH_OgrynStuntedIntellect;
        public static GeneDef BEWH_OgrynPatchedIntellect;

        public static DamageDef BEWH_WarpGaze;

        public static VFECore.Abilities.AbilityDef BEWH_WarpEyeWarpTravel;

        public static LetterDef BEWH_WarpTravel;

        static Abhumans40kDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Abhumans40kDefOf));
        }
    }
}