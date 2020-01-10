using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;
using StatDefOf = OutfitManager.DefOfs.StatDefOf;
using WorkTypeDefOf = OutfitManager.DefOfs.WorkTypeDefOf;

namespace OutfitManager
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class WorkPriorities : WorldComponent
    {
        private static List<WorktypePriorities> _worktypePriorities;

        public WorkPriorities(World world) : base(world)
        {
            Log.Message("WorldComponent created!");
        }

        private static List<StatPriority> DefaultPriorities(WorkTypeDef worktype)
        {
            var stats = new List<StatPriority>();
            if (worktype == WorkTypeDefOf.Art)
                // Work :: SculptingSpeed :: Sculpting speed
            {
                stats.Add(new StatPriority(StatDefOf.SculptingSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.BasicWorker)
                // Work :: UnskilledLaborSpeed :: Unskilled labor speed
            {
                stats.Add(new StatPriority(StatDefOf.UnskilledLaborSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Cleaning)
            {
                // Basics :: MoveSpeed :: Move speed
                // Work :: WorkSpeedGlobal :: Global work speed
                stats.Add(new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.WorkSpeedGlobal, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Cooking)
            {
                // Work :: CookSpeed :: Cooking speed
                // Work :: FoodPoisonChance :: Food poison chance
                // Work :: ButcheryFleshSpeed :: Butchery speed
                // Work :: ButcheryFleshEfficiency :: Butchery efficiency
                stats.Add(new StatPriority(StatDefOf.CookSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.FoodPoisonChance, OutfitStatPriority.MajorNegative));
                stats.Add(new StatPriority(StatDefOf.ButcheryFleshSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.ButcheryFleshEfficiency, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Construction)
            {
                /**
                 * Work :: ConstructionSpeed :: Construction speed
                 * Work :: ConstructSuccessChance :: Construct success chance
                 * Work :: FixBrokenDownBuildingSuccessChance :: Repair success chance
                   Work :: SmoothingSpeed :: Smoothing speed
                 */
                stats.Add(new StatPriority(StatDefOf.ConstructionSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.ConstructSuccessChance, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.FixBrokenDownBuildingSuccessChance,
                    OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.SmoothingSpeed, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Crafting)
            {
                // Work :: DrugSynthesisSpeed :: Drug synthesis speed
                // Work :: DrugCookingSpeed :: Drug cooking speed
                // Work :: ButcheryMechanoidSpeed :: Mechanoid disassembly speed
                // Work :: ButcheryMechanoidEfficiency :: Mechanoid disassembly efficiency
                stats.Add(new StatPriority(StatDefOf.WorkSpeedGlobal, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.DrugSynthesisSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.DrugCookingSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.ButcheryMechanoidSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.ButcheryMechanoidEfficiency, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Doctor)
            {
                /**
                 * Work :: MedicalTendSpeed :: Medical tend speed
                 * Work :: MedicalTendQuality :: Medical tend quality
                 * Work :: MedicalOperationSpeed :: Medical operation speed
                 * Work :: MedicalSurgerySuccessChance :: Medical surgery success chance
                 */
                stats.Add(new StatPriority(StatDefOf.MedicalTendSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.MedicalTendQuality, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.MedicalOperationSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.MedicalSurgerySuccessChance, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Firefighter)
                // Basics :: MoveSpeed :: Move speed
            {
                stats.Add(new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Growing)
            {
                // Work :: PlantWorkSpeed :: Plant work speed
                // Work :: PlantHarvestYield :: Plant harvest yield
                stats.Add(new StatPriority(StatDefOf.PlantWorkSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.PlantHarvestYield, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Handling)
            {
                /**
                 * Basics :: MoveSpeed :: Move speed
                 * Social :: TameAnimalChance :: Tame animal chance
                 * Social :: TrainAnimalChance :: Train animal chance
                 * Work :: AnimalGatherSpeed :: Animal gather speed
                 * Work :: AnimalGatherYield :: Animal gather yield
                 */
                stats.Add(new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.TameAnimalChance, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.TrainAnimalChance, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.AnimalGatherSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.AnimalGatherYield, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Hauling)
            {
                /**
                 * Basics :: CarryingCapacity :: Carrying capacity
                 * Basics :: MoveSpeed :: Move speed
                 */
                stats.Add(new StatPriority(StatDefOf.CarryingCapacity, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Hunting)
            {
                /**
                 * Basics :: MoveSpeed :: Move speed
                 * Combat :: ShootingAccuracyPawn :: Shooting accuracy
                 * Combat :: AimingDelayFactor :: Aiming time
                 * Work :: HuntingStealth :: Hunting stealth
                 * Weapon :: AccuracyTouch :: Accuracy (close)
                 * Weapon :: AccuracyShort :: Accuracy (short)
                 * Weapon :: AccuracyMedium :: Accuracy (medium)
                 * Weapon :: AccuracyLong :: Accuracy (long)
                 * Weapon :: RangedWeapon_Cooldown :: Ranged cooldown
                 * Weapon :: RangedWeapon_DamageMultiplier :: Damage multiplier
                 */
                stats.Add(new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.ShootingAccuracyPawn, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.AimingDelayFactor, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.HuntingStealth, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.AccuracyTouch, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.AccuracyShort, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.AccuracyMedium, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.AccuracyLong, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.RangedWeapon_Cooldown, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.RangedWeapon_DamageMultiplier, OutfitStatPriority.MinorPositive));
            }
            if (worktype == WorkTypeDefOf.Mining)
            {
                /**
                 * Work :: MiningSpeed :: Mining speed
                 * Work :: MiningYield :: Mining yield
                 */
                stats.Add(new StatPriority(StatDefOf.MiningSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.MiningYield, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Patient || worktype == WorkTypeDefOf.PatientBedRest)
            {
                //
            }
            if (worktype == WorkTypeDefOf.PlantCutting)
            {
                // Work :: PlantWorkSpeed :: Plant work speed
                // Work :: PlantHarvestYield :: Plant harvest yield
                stats.Add(new StatPriority(StatDefOf.PlantWorkSpeed, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.PlantHarvestYield, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Research)
                // Work :: ResearchSpeed :: Research speed
            {
                stats.Add(new StatPriority(StatDefOf.ResearchSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Smithing)
            {
                // Work :: SmithingSpeed :: Smithing speed
                // Work :: SmeltingSpeed :: Smelting speed
                stats.Add(new StatPriority(StatDefOf.SmeltingSpeed, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.SmithingSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Tailoring)
                // Work :: TailoringSpeed :: Tailoring speed
            {
                stats.Add(new StatPriority(StatDefOf.TailoringSpeed, OutfitStatPriority.MajorPositive));
            }
            if (worktype == WorkTypeDefOf.Warden)
            {
                /**
                 * Social :: NegotiationAbility :: Negotiation ability
                 * Social :: TradePriceImprovement :: Trade price improvement
                 * Social :: SocialImpact :: Social impact
                 */
                stats.Add(new StatPriority(StatDefOf.NegotiationAbility, OutfitStatPriority.MajorPositive));
                stats.Add(new StatPriority(StatDefOf.TradePriceImprovement, OutfitStatPriority.MinorPositive));
                stats.Add(new StatPriority(StatDefOf.SocialImpact, OutfitStatPriority.MajorPositive));
            }
            return stats;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _worktypePriorities, "worktypePriorities", LookMode.Deep);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (_worktypePriorities.NullOrEmpty())
            {
                _worktypePriorities = new List<WorktypePriorities>();

                // initialize with defaults
                foreach (var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                {
                    _worktypePriorities.Add(new WorktypePriorities(worktype, DefaultPriorities(worktype)));
                }
            }
        }

        public static List<StatPriority> WorktypeStatPriorities(Pawn pawn)
        {
            // get stat weights for each non-zero work priority.
            var worktypeStats = DefDatabase<WorkTypeDef>.AllDefsListForReading
                .Select(wtd => new {priority = pawn?.workSettings?.GetPriority(wtd) ?? 0, worktype = wtd})
                .Where(x => x.priority > 0).Select(x =>
                    new {x.priority, x.worktype, weights = WorktypeStatPriorities(x.worktype)}).ToList();

            // no work assigned.
            if (!worktypeStats.Any())
            {
                return new List<StatPriority>();
            }

            // normalize worktype priorities;
            // 1 - get the range (usually within 1-4, but may be up to 1-9 with Work Tab)
            var range = new IntRange(worktypeStats.Min(s => s.priority), worktypeStats.Max(s => s.priority));
            var weights = new Dictionary<StatDef, StatPriority>();
            var sumOfWeights = 0f;
            foreach (var worktype in worktypeStats)
            {
                // 2 - base to 0 (subtract minimum), scale to 0-1 (divide by maximum-minimum)
                // 3 - invert, so that 1 is 1, and max is 0.
                var normalizedPriority = range.min == range.max
                    ? 1
                    : 1 - (worktype.priority - range.min) / (range.max - range.min);
                foreach (var weight in worktype.weights)
                {
                    if (weights.TryGetValue(weight.Stat, out var statPriority))
                    {
                        statPriority.Weight += normalizedPriority * weight.Weight;
                    }
                    else
                    {
                        statPriority = new StatPriority(weight.Stat, normalizedPriority * weight.Weight);
                        weights.Add(weight.Stat, statPriority);
                    }
                    sumOfWeights += statPriority.Weight;
                }
            }

            // 4 - multiply weights by constant c, so that sum of weights is 10
            if (weights.Any() && sumOfWeights != 0)
            {
                foreach (var weight in weights)
                {
                    weight.Value.Weight *= 10 / sumOfWeights;
                }
            }
            return weights.Values.ToList();
        }

        public static List<StatPriority> WorktypeStatPriorities([NotNull] WorkTypeDef worktype)
        {
            if (worktype == null)
            {
                throw new ArgumentNullException(nameof(worktype));
            }
            var worktypePriorities = _worktypePriorities.Find(wp => wp.Worktype == worktype);
            if (worktypePriorities != null)
            {
                return worktypePriorities.Priorities;
            }
            Log.Warning(
                $"OutfitManager :: Created worktype stat priorities for '{worktype.defName}' after initial init. This should never happen!");
            worktypePriorities = new WorktypePriorities(worktype, DefaultPriorities(worktype));
            _worktypePriorities.Add(worktypePriorities);
            return worktypePriorities.Priorities;
        }
    }

    public class WorktypePriorities : IExposable
    {
        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public List<StatPriority> Priorities = new List<StatPriority>();

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public WorkTypeDef Worktype;

        [UsedImplicitly]
        public WorktypePriorities()
        {
            // used by ExposeData
        }

        public WorktypePriorities(WorkTypeDef worktype, List<StatPriority> priorities)
        {
            Worktype = worktype;
            Priorities = priorities;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Worktype, "worktype");
            Scribe_Collections.Look(ref Priorities, "statPriorities", LookMode.Deep);
        }
    }
}