using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;
using Core40k;

namespace Abhumans40k
{
    public class Ability_WarpEyeWarpTravel : VFECore.Abilities.Ability
    {
        private IntRange travelDurationRange = new IntRange(60, 60000);

        private int travelDuration = -2;

        private int travelledTime = -1;

        private GlobalTargetInfo[] targets;

        private List<Pawn> pawnsToSpawn;

        public override void DoAction()
        {
            Pawn pawn = PawnsToTeleport().FirstOrDefault((Pawn p) => p.IsQuestLodger());
            if (pawn != null)
            {
                Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), base.DoAction);
            }
            else
            {
                base.DoAction();
            }
        }

        private IEnumerable<Pawn> PawnsToTeleport()
        {
            Caravan caravan = pawn.GetCaravan();
            if (caravan != null)
            {
                foreach (Pawn pawn3 in caravan.pawns)
                {
                    yield return pawn3;
                }
                yield break;
            }
            bool homeMap = pawn.Map.IsPlayerHome;
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, GetRadiusForPawn(), useCenter: true))
            {
                if (thing is Pawn pawn2 && !pawn2.Dead && (pawn2.IsColonist || pawn2.IsPrisonerOfColony || (!homeMap && pawn2.RaceProps.Animal && (pawn2.Faction?.IsPlayer ?? false))))
                {
                    yield return pawn2;
                }
            }
        }

        private Pawn AlliedPawnOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault((Pawn p) => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer && !PawnsToTeleport().Contains(p));
        }

        public override bool CanHitTargetTile(GlobalTargetInfo target)
        {
            int range = Find.WorldGrid.TraversalDistanceBetween((CasterPawn.GetCaravan() != null) ? CasterPawn.GetCaravan().Tile : Caster.Map.Tile, target.Tile);
            if (range > GetRangeForPawn())
            {
                return false;
            }
            return true;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            this.targets = targets;

            Caravan caravan = pawn.GetCaravan();
            List<Pawn> list = PawnsToTeleport().ToList();

            //Teleport from map to somewhere
            if (caravan == null)
            {
                foreach (Pawn item in list)
                {
                    if (item.Spawned)
                    {
                        item.teleporting = true;
                        item.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
                    }
                }
            }
            else //Teleport from caravan to somewhere
            {
                caravan.Destroy();
            }

            pawnsToSpawn = list;
            travelDuration = travelDurationRange.RandomInRange;
            travelledTime = 0;
            base.Cast(targets);
        }

        public override void Tick()
        {
            if (travelDuration != -2)
            {
                if (travelledTime == travelDuration)
                {
                    ExitWarp();
                    string teleportedPawns = "";
                    for (int i = 0; i < pawnsToSpawn.Count; i++)
                    {
                        teleportedPawns += pawnsToSpawn[i].NameShortColored;
                        if (i == pawnsToSpawn.Count-2)
                        {
                            teleportedPawns += " and ";
                        }
                        else if ( i != pawnsToSpawn.Count-1)
                        {
                            teleportedPawns += ", ";
                        }
                    }

                    string timeSpent = "";
                    if (travelDuration/2500 == 1)
                    {
                        timeSpent = "1 hour";
                    }
                    else
                    {
                        timeSpent = (travelDuration / 2500) + "hours";
                    }

                    Letter_JumpTo letter = new Letter_JumpTo
                    {
                        lookTargets = Caster,
                        def = Abhumans40kDefOf.BEWH_WarpTravel,
                        Text = "WarpTravelMessage".Translate(Caster.Named("PAWN"), timeSpent, teleportedPawns),
                        title = "WarpTravelLetter".Translate()
                    };

                    Find.LetterStack.ReceiveLetter(letter);

                    travelDuration = -2;
                    travelledTime = -1;
                }
                else
                {
                    travelledTime++;
                }
            }
            base.Tick();
        }

        private void ExitWarp()
        {
            Caravan caravan = pawn.GetCaravan();
            Map targetMap = ((targets[0].WorldObject is MapParent mapParent) ? mapParent.Map : null);
            IntVec3 targetCell = IntVec3.Invalid;
            if (targetMap != null)
            {
                Pawn pawn = AlliedPawnOnMap(targetMap);
                if (pawn != null)
                {
                    IntVec3 position = pawn.Position;
                    if (true)
                    {
                        targetCell = position;
                    }
                }
            }
            //Another allied pawn is on map which is being teleported too
            if (targetCell.IsValid)
            {
                foreach (Pawn item in pawnsToSpawn)
                {
                    CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, (IntVec3 cell) => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                    GenSpawn.Spawn(item, result, targetMap);
                    item.teleporting = false;
                    if (item.drafter != null && item.IsColonistPlayerControlled)
                    {
                        item.drafter.Drafted = true;
                    }
                    item.Notify_Teleported();
                    if (item.IsPrisoner)
                    {
                        item.guest.WaitInsteadOfEscapingForDefaultTicks();
                    }
                    if ((item.IsColonist || item.RaceProps.packAnimal) && item.Map.IsPlayerHome)
                    {
                        item.inventory.UnloadEverything = true;
                    }
                }

                if (Find.WorldSelector.IsSelected(caravan))
                {
                    Find.WorldSelector.Deselect(caravan);
                }
                caravan?.Destroy();
            }
            //Teleport to friendly caravan on world map
            else if (targets[0].WorldObject is Caravan caravan2 && caravan2.Faction == pawn.Faction)
            {
                if (caravan != null)
                {
                    caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
                    caravan2.Notify_Merged(new List<Caravan> { caravan });
                    caravan.Destroy();
                }
                else
                {
                    foreach (Pawn item2 in pawnsToSpawn)
                    {
                        caravan2.AddPawn(item2, addCarriedPawnToWorldPawnsIfAny: true);
                    }
                }
            }
            //Teleport from world map to world map
            else if (caravan != null)
            {
                caravan.Tile = targets[0].Tile;
                caravan.pather.StopDead();
            }
            //Teleport to unoccupied world map tile
            else
            {
                CaravanMaker.MakeCaravan(pawnsToSpawn, pawn.Faction, targets[0].Tile, addToWorldPawnsIfNotAlready: false);
            }
        }

        public override void GizmoUpdateOnMouseover()
        {
            GenDraw.DrawRadiusRing(pawn.Position, GetRadiusForPawn(), Color.blue);
        }
    }
}