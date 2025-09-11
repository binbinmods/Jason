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
                // Blood from a Stone: Bleed +2, Poison +2. 
                // Poison and bleed cannot be prevented by immunities or buffer, nor can they be dispelled (even when specified). 

                // if (CanIncrementTraitActivations(traitId) && _castedCard.HasCardType(Enums.CardType.Defense))// && MatchManager.Instance.energyJustWastedByHero > 0)
                // {
                //     LogDebug($"Handling Trait {traitId}: {traitName}");
                //     // _character?.ModifyEnergy(1);
                //     // DrawCards(1);
                //     IncrementTraitActivations(traitId);
                // }
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Castigate. All Damage +1 for each unique Curse in play. On hit, gain 1 Vitality.
                LogDebug($"Handling Trait {traitId}: {traitName}");

                _character?.SetAuraTrait(_character, "vitality", 1);

            }

            else if (_trait == trait4a)
            {
                // trait 4a;

                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Verdict - Poison and Bleed cannot be restricted. 
                // Deal Bonus single hit damage equal to 25% of current Poison and Bleed on target. 
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
                // trait2a:
                // Poison and bleed cannot be prevented by immunities or buffer, 
                // nor can they be dispelled (even when specified).

                // trait2b:

                // trait 4a;
                // Max Vitality charges +50. 
                // At the start of every round, gain 10% to All Resistances. 

                // trait 4b:
                // Poison and Bleed cannot be restricted

                case "bleed":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.Preventable = false;
                        __result.Removable = false;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.MaxCharges = -1;
                        __result.MaxMadnessCharges = -1;
                    }
                    break;
                case "poison":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.Preventable = false;
                        __result.Removable = false;
                    }
                    traitOfInterest = trait4a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.MaxCharges = -1;
                        __result.MaxMadnessCharges = -1;
                    }

                    break;
                case "vitality":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.RemoveAuraCurse = (AuraCurseData)null;
                        // __result.RemoveAuraCurse2 = null;
                    }
                    traitOfInterest = trait4a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.MaxMadnessCharges += 50;
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetTraitDamagePercentModifiers")]
        public static void GetTraitDamagePercentModifiersPostfix(Enums.DamageType DamageType, ref float __result, Character __instance)
        {
            // ___useCache = false;

            if (!IsLivingHero(__instance) || MatchManager.Instance == null)
            {
                return;
            }

            // All Damage +0.3% for each stack of Bleed and Poison in play, up to a maximum of +300% All Damage.
            string traitOfInterest = trait0;

            if (__instance.HaveTrait(traitOfInterest))
            {
                int nStacks = CountAllStacks("bleed") + CountAllStacks("poison");
                __result += Mathf.Clamp(0.3f * nStacks, 0, 300);
            }






        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetTraitDamageFlatModifiers")]
        public static void GetTraitDamageFlatModifiersPostfix(Enums.DamageType DamageType, ref int __result, Character __instance)
        {
            // ___useCache = false;

            if (!IsLivingHero(__instance) || MatchManager.Instance == null)
            {
                return;
            }
            // foreach unique curse gain +1 all damage
            string traitOfInterest = trait2b;
            if (__instance.HaveTrait(traitOfInterest))
            {
                HashSet<string> curses = [];
                foreach (Hero hero in MatchManager.Instance.GetTeamHero())
                {
                    curses.UnionWith(hero.GetCurseList());
                }
                foreach (NPC npc in MatchManager.Instance.GetTeamNPC())
                {
                    curses.UnionWith(npc.GetCurseList());
                }
                float multiplier = 0.5f;
                __result += Mathf.RoundToInt(multiplier * curses.Count());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetItemResistModifiers")]
        public static void GetItemResistModifiersPostfix(Character __instance, ref int __result, Enums.DamageType type)
        {
            if (MatchManager.Instance == null || !IsLivingHero(__instance) || !__instance.HaveTrait(trait4a))
            {
                return;
            }
            LogDebug("Handling trait4a - Bonus resists");
            __result += (MatchManager.Instance.GetCurrentRound() - 1) * 10;

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





        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        // public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        // {
        //     // LogInfo("executing SetDescriptionNewPostfix");
        //     if (__instance == null)
        //     {
        //         LogDebug("Null Card");
        //         return;
        //     }
        //     if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
        //     {
        //         LogError($"missing card Id {__instance.Id}");
        //         return;
        //     }


        //     if (__instance.CardName == "Mind Maze")
        //     {
        //         StringBuilder stringBuilder1 = new StringBuilder();
        //         LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
        //         string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
        //         stringBuilder1.Append(currentDescription);
        //         // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
        //         stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
        //         BinbinNormalizeDescription(ref __instance, stringBuilder1);
        //     }
        // }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        public static bool HealAuraCursePrefix(ref Character __instance, AuraCurseData AC)
        {
            // LogInfo($"HealAuraCursePrefix {subclassName}");
            string traitId = trait2a;
            if (AtOManager.Instance.TeamHaveTrait(traitId) && (AC == GetAuraCurseData("bleed") || AC == GetAuraCurseData("poison")) && !__instance.IsHero)
            {
                return false;
            }
            return true;

        }

    }
}

