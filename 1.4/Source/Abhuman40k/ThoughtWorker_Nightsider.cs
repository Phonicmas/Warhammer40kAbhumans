using RimWorld;
using Verse;

namespace Abhumans40k
{
    public class ThoughtWorker_Nightsider : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (pawn.story.traits.HasTrait(TraitDefOf.Kind))
            {
                return false;
            }
            if (other.genes != null)
            {
                if (!other.genes.HasGene(Abhumans40kDefOf.BEWH_NightsiderOtherwordly))
                {
                    return false;
                }
            }
            return true;
        }
    }
}