using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

namespace LC_HoardingBugSnacks.Patches
{
    public class BugSnacks : GrabbableObject
    {
        public override void Start()
        {
            base.Start();
            foreach (var g in FindObjectsOfType<GrabbableObject>())
            {
                HoardingBugSnacksMod.mls.LogMessage($"{g.itemProperties.itemName}" +
                    $"\n\tgrabAnim: {g.itemProperties.grabAnim}" +
                    $"\n\tpocketAnim: {g.itemProperties.pocketAnim}" +
                    $"\n\tuseAnim: {g.itemProperties.useAnim}" +
                    $"\n\tthrowAnim: {g.itemProperties.throwAnim}");
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                playerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
                MakeBugsDance(transform, 2, 10);
            }
        }

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            if (enemy != null && enemy is HoarderBugAI)
            {
                var bug = (HoarderBugAI)enemy;
                MakeBugsDance(bug.transform, 10, 10);
                BecomeFriends(bug, 15f, true);
            }
        }

        public static void BecomeFriends(HoarderBugAI mainBug, float friendAmount, bool allBugs = true)
        {
            mainBug.BecomeFriends(friendAmount);

            if (allBugs)
                foreach (HoarderBugAI bug in FindObjectsOfType<HoarderBugAI>())
                {
                    bug.BecomeFriends(friendAmount);
                }
        }
        public static void MakeBugsDance(Transform pos, int time, float range)
        {
            if(range > 0.5f)
            {
                var allBugs = FindObjectsOfType<HoarderBugAI>();
                foreach(var bug in allBugs)
                {
                    if(Vector3.Distance(bug.transform.position, pos.position) < range) bug.StartCoroutine(bug.Dance(time));
                }
            }


        }
    }

    public static class ExtensionMethod
    {
        public static void BecomeFriends(this HoarderBugAI original, float amount)
        {
            Type type = typeof(HoarderBugAI);

            // Get the private field
            FieldInfo field = type.GetField("annoyanceMeter", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                // Get the value of the private field
                float currentValue = (float)field.GetValue(original);

                // Modify the value of the private field
                field.SetValue(original, currentValue - amount);
            }
            original.angryTimer -= amount;
        }

        public static IEnumerator<WaitForEndOfFrame> Dance(this HoarderBugAI original, float time)
        {
            float Amplitude = 2;
            float Frequency = Mathf.PI * 10f;

            Transform spinTrans = original.gameObject.transform.Find("HoarderBugModel").Find("AnimContainer").Find("Armature");
            Transform chestTrans = spinTrans.Find("Abdomen").Find("Chest");
            Vector3 startRotation = spinTrans.localRotation.eulerAngles;

            float startY = chestTrans.position.y;
            while (time < 0)
            {
                chestTrans.localPosition = new Vector3(
                    chestTrans.localPosition.x, 
                    startY + Mathf.Sin(time * Frequency) * Amplitude,
                    chestTrans.localPosition.z);

                spinTrans.localRotation = Quaternion.Euler(startRotation.x, startRotation.y + time * 90f * Amplitude, startRotation.z);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
