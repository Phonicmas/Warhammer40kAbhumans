using Verse;

namespace Abhumans40k
{
    public class Hediff_StuntedIntellectEnhancer : Hediff_Implant
    {
        public override bool ShouldRemove => false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (pawn.genes == null)
            {
                return;
            }
            if (!pawn.genes.HasGene(Abhumans40kDefOf.BEWH_OgrynStuntedIntellect))
            {
                return;
            }
            pawn.genes.RemoveGene(pawn.genes.GetGene(Abhumans40kDefOf.BEWH_OgrynStuntedIntellect));
            pawn.genes.AddGene(Abhumans40kDefOf.BEWH_OgrynPatchedIntellect, true);

            base.PostAdd(dinfo);
        }

        public override void PostRemoved()
        {
            if (pawn.genes == null)
            {
                return;
            }
            if (!pawn.genes.HasGene(Abhumans40kDefOf.BEWH_OgrynPatchedIntellect))
            {
                return;
            }
            pawn.genes.RemoveGene(pawn.genes.GetGene(Abhumans40kDefOf.BEWH_OgrynPatchedIntellect));
            pawn.genes.AddGene(Abhumans40kDefOf.BEWH_OgrynStuntedIntellect, false);

            base.PostRemoved();
        }

    }
}