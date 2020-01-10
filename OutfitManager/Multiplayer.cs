using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Multiplayer.API;
using RimWorld;
using Verse;

namespace OutfitManager
{
    [StaticConstructorOnStartup]
    public static class ExtendedOutfitProxy
    {
        private static readonly ISyncField[] ExtendedOutfitFields;
        private static readonly ISyncField[] ProxyFields;

        private static int targetOutfitId;
        private static StatDef targetStat;
        private static float targetWeight;

        [SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline",
            Justification = "<Pending>")]
        static ExtendedOutfitProxy()
        {
            if (!MP.enabled)
            {
                return;
            }
            ProxyFields = new[]
            {
                MP.RegisterSyncField(typeof(ExtendedOutfitProxy), nameof(targetWeight)).SetBufferChanges()
                    .PostApply(Update)
            };
            ExtendedOutfitFields = new[]
            {
                MP.RegisterSyncField(typeof(ExtendedOutfit), nameof(ExtendedOutfit.TargetTemperaturesOverride)),
                MP.RegisterSyncField(typeof(ExtendedOutfit), nameof(ExtendedOutfit.TargetTemperatures)),
                MP.RegisterSyncField(typeof(ExtendedOutfit), nameof(ExtendedOutfit.PenalizeTaintedApparel)),
                MP.RegisterSyncField(typeof(ExtendedOutfit), nameof(ExtendedOutfit.AutoWorkPriorities))
            };
            MP.RegisterSyncMethod(typeof(ExtendedOutfit), nameof(ExtendedOutfit.AddStatPriority));
            MP.RegisterSyncMethod(typeof(ExtendedOutfit), nameof(ExtendedOutfit.RemoveStatPriority));
            MP.RegisterSyncMethod(typeof(ExtendedOutfitProxy), nameof(SetStat));
            MP.RegisterSyncWorker<ExtendedOutfit>(ExtendedOutfitSyncer);
        }

        private static void ExtendedOutfitSyncer(SyncWorker sync, ref ExtendedOutfit outfit)
        {
            if (sync.isWriting)
            {
                sync.Bind(ref outfit.uniqueId);
            }
            else
            {
                var uid = 0;
                sync.Bind(ref uid);
                var currentOutfit = Current.Game.outfitDatabase.AllOutfits.Find(o => o.uniqueId == uid);
                if (currentOutfit is ExtendedOutfit extendedOutfit)
                {
                    outfit = extendedOutfit;
                }
            }
        }

        // That's why it gets its own SyncMethod, SyncFields suffer from buffers
        private static void SetStat(int uid, StatDef stat, float weight)
        {
            targetOutfitId = uid;
            targetStat = stat;
            Update(null, weight);
        }

        // For sliders, we must buffer weight but stat must be accurate
        public static void SetStatPriority(int selectedOutfitId, StatDef stat, float weight)
        {
            if (targetOutfitId != selectedOutfitId || !targetStat.Equals(stat)) // Forces any changes
            {
                SetStat(selectedOutfitId, stat, weight);
            }
            else // Buffers the rest
            {
                targetWeight = weight;
            }
        }

        private static void Update(object arg1, object arg2)
        {
            var weight = (float) arg2;
            if (!(Current.Game.outfitDatabase.AllOutfits.Find(o => o.uniqueId == targetOutfitId) is ExtendedOutfit
                outfit))
            {
                throw new InvalidOperationException("Not an ExtendedOutfit");
            }
            var statPriority = outfit.StatPriorities.FirstOrDefault(sp => sp.Stat == targetStat);
            if (statPriority == null)
            {
                outfit.AddStatPriority(targetStat, weight);
            }
            else
            {
                statPriority.Weight = weight;
            }
        }

        private static void Watch(this ISyncField[] fields, object target = null)
        {
            foreach (var field in fields)
            {
                field.Watch(target);
            }
        }

        public static void Watch(ref ExtendedOutfit outfit)
        {
            ProxyFields.Watch();
            ExtendedOutfitFields.Watch(outfit);
        }
    }
}