using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Jason.CustomFunctions;
using static Jason.Plugin;
using static Jason.DescriptionFunctions;
using static Jason.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Jason
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // trait0:
                // -150% All Damage.
                // All Damage +0.3% for each stack of Bleed and Poison in play, up to a maximum of +300% All Damage.

            }


            else if (_trait == trait2a)
            {
                // trait2a
                // When you damage a monster, deal Shadow Damage equal to 15% of its Bleed stacks and Holy Damage equal to 15% of its Poison stacks
                if (!IsLivingNPC(_target))
                {
                    float multiplier = 0.15f;
                    int holyDamage = Mathf.RoundToInt(_target.GetAuraCharges("poison") * multiplier);
                    int shadowDamage = Mathf.RoundToInt(_target.GetAuraCharges("bleed") * multiplier);
                    _target.IndirectDamage(Enums.DamageType.Shadow, shadowDamage);
                    _target.IndirectDamage(Enums.DamageType.Holy, holyDamage);
                }
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // When you play a card that costs 2 or more, 
                // reduce by 1 the cost of your highest cost card that costs 2 or more.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (CanIncrementTraitActivations(traitId) && MatchManager.Instance.energyJustWastedByHero >= 2)
                {
                    CardData highCost = GetRandomHighestCostCard(Enums.CardType.None);
                    if (highCost.GetCardFinalCost() >= 2)
                    {
                        ReduceCardCost(ref highCost, amountToReduce: 1, isPermanent: true);
                    }
                }

            }

            else if (_trait == trait4a)
            {
                // trait 4a;

                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Castigate - Monster resists -5% per unique curse. All Damage +1 for each unique Curse on monsters.
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (IsLivingNPC(_target))
                {
                    _target?.SetAuraTrait(_character, "harbingerofdoom", 1);
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait0:
                // Poison and bleed cannot be prevented by immunities or buffer, 
                // nor can they be dispelled (even when specified).

                // trait2b:

                // trait 4a;

                // trait 4b:

                case "bleed":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.Preventable = false;
                        __result.Removable = false;
                    }
                    // traitOfInterest = trait4b;
                    // if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    // {
                    //     __result.MaxCharges = -1;
                    //     __result.MaxMadnessCharges = -1;
                    // }
                    break;
                case "poison":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.Preventable = false;
                        __result.Removable = false;
                    }
                    // traitOfInterest = trait4b;
                    // if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    // {
                    //     __result.MaxCharges = -1;
                    //     __result.MaxMadnessCharges = -1;
                    // }

                    break;
                case "vitality":

                    break;
            }
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), "GetTraitDamagePercentModifiers")]
        // public static void GetTraitDamagePercentModifiersPostfix(Enums.DamageType DamageType, ref float __result, Character __instance)
        // {
        //     // ___useCache = false;

        //     if (!IsLivingHero(__instance) || MatchManager.Instance == null)
        //     {
        //         return;
        //     }

        //     // All Damage +0.3% for each stack of Bleed and Poison in play, up to a maximum of +300% All Damage.
        //     string traitOfInterest = trait0;

        //     if (__instance.HaveTrait(traitOfInterest))
        //     {
        //         int nStacks = CountAllStacks("bleed") + CountAllStacks("poison");
        //         __result += Mathf.Clamp(0.3f * nStacks, 0, 300);
        //     }
        // }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), "GetTraitDamageFlatModifiers")]
        // public static void GetTraitDamageFlatModifiersPostfix(Enums.DamageType DamageType, ref int __result, Character __instance)
        // {
        //     // ___useCache = false;

        //     if (!IsLivingHero(__instance) || MatchManager.Instance == null)
        //     {
        //         return;
        //     }
        //     // trait4a
        //     // Castigate - foreach unique curse on monsters gain +1 all damage
        //     string traitOfInterest = trait4a;
        //     if (__instance.HaveTrait(traitOfInterest))
        //     {
        //         HashSet<string> curses = [];
        //         // foreach (Hero hero in MatchManager.Instance.GetTeamHero())
        //         // {
        //         //     curses.UnionWith(hero.GetCurseList());
        //         // }
        //         foreach (NPC npc in MatchManager.Instance.GetTeamNPC())
        //         {
        //             curses.UnionWith(npc.GetCurseList());
        //         }
        //         float multiplier = 0.5f;
        //         __result += Mathf.RoundToInt(multiplier * curses.Count());
        //     }
        // }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetItemResistModifiers")]
        public static void GetItemResistModifiersPostfix(Character __instance, ref int __result, Enums.DamageType type)
        {
            // Trait 4a, reduce resists for each unique curse on enemies
            if (MatchManager.Instance == null || IsLivingHero(__instance) || !AtOManager.Instance.TeamHaveTrait(trait4a))
            {
                return;
            }
            // LogDebug("Handling trait4a - Reducing Resists resists");
            __result -= __instance.GetCurseList().Count * 5;

        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPostfix(
            Character __instance,
            Enums.EventActivation theEvent,
            Character target = null,
            int auxInt = 0,
            string auxString = "")
        {
            // Harbinger of Doom
            if (theEvent == Enums.EventActivation.BeginTurn && __instance.HasEffect("harbingerofdoom"))
            {
                List<string> curseList = __instance.GetCurseList();
                int nToApply = 2 * __instance.GetAuraCharges("harbingerofdoom");
                Character[] charactersToApplyTo = null;
                if (IsLivingNPC(__instance))
                {
                    charactersToApplyTo = MatchManager.Instance.GetTeamNPC();

                }
                if (IsLivingHero(__instance))
                {
                    charactersToApplyTo = MatchManager.Instance.GetTeamHero();
                }
                if (charactersToApplyTo == null || charactersToApplyTo.Length == 0)
                {
                    return;
                }
                foreach (Character character in charactersToApplyTo.Where(c => c != null && c.Alive && c != __instance))
                {
                    foreach (string curse in curseList)
                    {
                        character.SetAura(character, Globals.Instance.GetAuraCurseData(curse), nToApply, useCharacterMods: false, canBePreventable: false);
                    }
                }
            }
        }




        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static bool HealAuraCursePrefix(ref Character __instance, AuraCurseData AC)
        // {
        //     // LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitId = trait2a;
        //     if (AtOManager.Instance.TeamHaveTrait(traitId) && (AC == GetAuraCurseData("bleed") || AC == GetAuraCurseData("poison")) && !__instance.IsHero)
        //     {
        //         return false;
        //     }
        //     return true;

        // }

    }
}

