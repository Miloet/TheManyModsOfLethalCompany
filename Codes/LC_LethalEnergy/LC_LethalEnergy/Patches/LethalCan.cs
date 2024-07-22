using UnityEngine;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using Unity.Netcode;

namespace LC_LethalEnergy
{
    public class LethalCan : GrabbableObject
    {
        public bool drinking;

        public float fill = 1f;
        public float duration = 20f;

        public float timeToDrink = 2f;
        public float timeToStart = 0.75f;

        public AudioSource audio;
        public Coroutine DrinkCoroutine;
        public PlayerControllerB previousPlayerHeldBy;

        public const string DrinkAnimation = "HoldMask";

        public static Item normalProp;
        public static Item storeProp;
        public static Vector3 normalPos;
        public static Vector3 normalRot;

        public static Item drinkingProp;
        public static Vector3 drinkingPos;
        public static Vector3 drinkingRot;

        float animationLerp;

        private const int baseValue = 8;
        public const int emptyValue = 1;

        public static AudioClip drinkingSFX;

        public FillHandler fillHandler;
        public bool loaded = false;

        public bool store = false;

        public override void Start()
        {
            if(!store) SetScrapValue(baseValue);

            if (normalProp == null)
            {
                if (!store) normalProp = itemProperties;

                normalPos = itemProperties.positionOffset;
                normalRot = itemProperties.rotationOffset;
                drinkingPos = drinkingProp.positionOffset;
                drinkingRot = drinkingProp.rotationOffset;
            }

            base.Start();

            fallTime = 1f;
            hasHitGround = true;

            audio = GetComponent<AudioSource>();

            //FallToGround(true);
            //DrinkAnimation = itemProperties.useAnim;

            if (!IsServer) StartCoroutine(RequestData());
        }
        public IEnumerator RequestData()
        {
            yield return new WaitForSeconds(5f);

            if (!loaded) fillHandler.RequestFillDataServerRpc();
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
            if (isHeld && playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                if (playerHeldBy.playerBodyAnimator.GetBool(DrinkAnimation)) animationLerp = Mathf.MoveTowards(animationLerp, 1f, Time.deltaTime * 3f / timeToStart);
                else animationLerp = Mathf.MoveTowards(animationLerp, 0f, Time.deltaTime * 3f / timeToStart);

                animationLerp = Mathf.Clamp01(animationLerp);

                if (animationLerp > 0.1f) ChangeItem(drinkingProp);
                else ChangeItem(!store ? normalProp : storeProp);

                if (itemProperties == drinkingProp)
                {
                    drinkingProp.positionOffset = Vector3.Lerp(normalPos, drinkingPos, animationLerp);
                    drinkingProp.rotationOffset = Vector3.Lerp(normalRot, drinkingRot, animationLerp);
                }


                if (drinking)
                {
                    if (previousPlayerHeldBy == null || !isHeld || fill <= 0f)
                    {
                        Stop();
                        itemUsedUp = true;
                        //audio.Stop();
                    }
                    //previousPlayerHeldBy.
                    fill -= Time.deltaTime / timeToDrink;
                    PlayerPatches.Caffeine += Time.deltaTime / timeToDrink;
                    PlayerPatches.Duration += duration * Time.deltaTime / timeToDrink;
                }
            }
        }

        public void ChangeItem(Item to)
        {
            if (itemProperties != to)
            {
                itemProperties = to;
            }
        }


        private IEnumerator<WaitForSeconds> StartDrink()
        {
            yield return new WaitForSeconds(timeToStart);
            drinking = true;
            audio.PlayOneShot(drinkingSFX, 0.75f);

            ////audio.PlayOneShot(twistCanSFX);
        }

        public IEnumerator<WaitForSeconds> CrushCan()
        {
            Vector3 origin = Vector3.one;
            Vector3 target = new Vector3(1, 0.3f, 1f);
            float time = 0;

            while (time < 1f)
            {
                transform.localScale = Vector3.Lerp(origin, target, time);

                time += Time.deltaTime*4f;
                yield return null;
            }
            originalScale = target;
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
            animationLerp = 0;
            if (DrinkCoroutine != null)
            {
                StopCoroutine(DrinkCoroutine);
            }
            Stop();
            fillHandler.SendFillServerRpc(fill);
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


    public class FillHandler : NetworkBehaviour
    {
        public LethalCan lethalCan;

        [ServerRpc(RequireOwnership = false)]
        public void SendFillServerRpc(float fill)
        {
            SendFillClientRpc(fill);
        }
        [ClientRpc]
        public void SendFillClientRpc(float Fill)
        {
            lethalCan.loaded = true;
            lethalCan.fill = Fill;
            if (lethalCan.fill <= 0)
            {
                lethalCan.SetScrapValue(LethalCan.emptyValue);
                lethalCan.itemUsedUp = true;
                lethalCan.StartCoroutine(lethalCan.CrushCan());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestFillDataServerRpc()
        {
            RequestFillDataClientRpc();
        }

        [ClientRpc]
        public void RequestFillDataClientRpc()
        {
            if (IsServer) SendFillServerRpc(lethalCan.fill);
        }
    }


    public static class PlayerPatches
    {
        public static float BaseSpeed;
        public static float Caffeine;
        public static float DecayRate = 5f;
        public static float Duration;

        public static bool warn;
        public static bool kill;

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
                //__instance.moveInputVector = Quaternion.Euler(0, Mathf.Sin(Time.time) * Caffeine * 5f, 0) * __instance.moveInputVector;

                if (Duration > 0)
                {
                    Duration -= Time.deltaTime;
                }
                else Caffeine -= Time.deltaTime / DecayRate;


                if (Caffeine > 3f && !warn)
                {
                    warn = true;
                    __instance.DamagePlayer(10, true, true, CauseOfDeath.Crushing, 1, false, default(Vector3));
                    HUDManager.Instance.DisplayTip("Warning", "High caffeine blood content", true);
                }
                if (Caffeine > 4f && !kill)
                {
                    kill= true;
                    __instance.DamagePlayer(9999,true,true,CauseOfDeath.Crushing,1, false, Vector3.up);
                }


                Caffeine = Mathf.Clamp(Caffeine, 0, 5);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        [HarmonyPostfix]
        public static void ResetCaffine(PlayerControllerB __instance)
        {
            if(__instance.IsOwner && __instance.health <= 0)
            {
                warn = false;
                kill = false;
                Caffeine = 0;
                Duration = 0;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevelWait")]
        [HarmonyPostfix]
        public static void ResetCaffeine()
        {
            warn=false;
            kill=false;
            Caffeine = 0;
            Duration = 0;
        }
    }
}
