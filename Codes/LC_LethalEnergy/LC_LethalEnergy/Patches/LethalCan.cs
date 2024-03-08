using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;

namespace LC_LethalEnergy
{
    public class LethalCan : GrabbableObject
    {

        public bool drinking;

        public float fill = 1f;
        public float duration = 20f;

        public float timeToDrink = 2f;

        public AudioSource audio;
        public Coroutine DrinkCoroutine;
        public PlayerControllerB previousPlayerHeldBy;

        public const string DrinkAnimation = "HoldMask";


        public override void Start()
        {
            base.Start();
            audio = GetComponent<AudioSource>(); 
            //DrinkAnimation = itemProperties.useAnim;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                isBeingUsed = true;
                if (fill <= 0f)
                {
                    previousPlayerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
                    return;
                }
                previousPlayerHeldBy.activatingItem = true;
                previousPlayerHeldBy.playerBodyAnimator.SetBool(DrinkAnimation, true);
                previousPlayerHeldBy = playerHeldBy;
                DrinkCoroutine = StartCoroutine(StartDrink());
            }
            else
            {
                isBeingUsed = false;
                if (DrinkCoroutine != null) StopCoroutine(DrinkCoroutine);
                Stop();
            }
        }
        public override void Update()
        {
            base.Update();

            if (drinking)
            {
                if (previousPlayerHeldBy == null || !isHeld || fill <= 0f)
                {
                    Stop();
                    //audio.Stop();
                }
                //previousPlayerHeldBy.
                fill -= Time.deltaTime / timeToDrink;
                PlayerPatches.Caffeine += Time.deltaTime / timeToDrink;
                PlayerPatches.Duration += duration * Time.deltaTime / timeToDrink;
            }
        }


        private IEnumerator<WaitForSeconds> StartDrink()
        {
            //previousPlayerHeldBy.activatingItem = true;
            //previousPlayerHeldBy.playerBodyAnimator.SetBool(DrinkAnimation, true);
            yield return new WaitForSeconds(0.75f);
            drinking = true;

            ////audio.PlayOneShot(twistCanSFX);
        }



        public override void EquipItem()
        {
            base.EquipItem();
            if (playerHeldBy != null)
            {
                previousPlayerHeldBy = playerHeldBy;
            }
        }
        public override void DiscardItem()
        {
            if (DrinkCoroutine != null)
            {
                StopCoroutine(DrinkCoroutine);
            }
            Stop();

            if (previousPlayerHeldBy != null)
            {
                previousPlayerHeldBy.activatingItem = false;
            }
            base.DiscardItem();
        }


        public void Stop()
        {
            drinking = false;
            previousPlayerHeldBy.activatingItem = false;
            audio.Stop();
            playerHeldBy.playerBodyAnimator.ResetTrigger("shakeItem");
            previousPlayerHeldBy.playerBodyAnimator.SetBool(DrinkAnimation, false);
        }
    }


    public static class PlayerPatches
    {
        public static float BaseSpeed;
        public static float Caffeine;
        public static float DecayRate = 5f;
        public static float Duration;

        public static float Boost = .25f;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]

        public static void StartPatch(PlayerControllerB __instance)
        {
            BaseSpeed = __instance.movementSpeed;
            Caffeine = 0;
            Duration = 0;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]

        public static void UpdatePatch(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return;
            __instance.movementSpeed = BaseSpeed + BaseSpeed * Caffeine * Boost;
            if (Caffeine > 0)
            {
                __instance.moveInputVector = Quaternion.Euler(0, Mathf.Sin(Time.time) * Caffeine * 5f, 0) * __instance.moveInputVector;

                if (Duration > 0)
                {
                    Duration -= Time.deltaTime;
                }
                else Caffeine -= Time.deltaTime / DecayRate;


                if (Caffeine > 3f)
                {
                    __instance.DamagePlayer(10, true, true, CauseOfDeath.Crushing, 1, false, default(Vector3));
                    HUDManager.Instance.DisplayTip("Warning", "High caffeine blood content", true);
                }
                if (Caffeine > 4f)
                {
                    __instance.DamagePlayer(9999,true,true,CauseOfDeath.Crushing,1, false, default(Vector3));
                }


                Caffeine = Mathf.Clamp(Caffeine, 0, 5);
            }
        }

        /*public static void ChangeSpeed(this PlayerControllerB player)
        {
            player.movementSpeed = BaseSpeed * 2;
            
        }*/


    }
}
