using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

#pragma warning disable 649
#pragma warning disable CA2211 // Non-constant fields should not be visible

namespace OutfitManager.DefOfs
{
    [DefOf]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public static class WorkTypeDefOf
    {
        public static WorkTypeDef Art;
        public static WorkTypeDef BasicWorker;
        public static WorkTypeDef Cleaning;
        public static WorkTypeDef Construction;
        public static WorkTypeDef Cooking;
        public static WorkTypeDef Crafting;
        public static WorkTypeDef Doctor;
        public static WorkTypeDef Firefighter;
        public static WorkTypeDef Growing;
        public static WorkTypeDef Handling;
        public static WorkTypeDef Hauling;
        public static WorkTypeDef Hunting;
        public static WorkTypeDef Mining;
        public static WorkTypeDef Patient;
        public static WorkTypeDef PatientBedRest;
        public static WorkTypeDef PlantCutting;
        public static WorkTypeDef Research;
        public static WorkTypeDef Smithing;
        public static WorkTypeDef Tailoring;
        public static WorkTypeDef Warden;
    }
}