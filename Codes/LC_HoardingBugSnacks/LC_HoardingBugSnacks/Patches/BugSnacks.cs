using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;
using HoarderBud.Patches;
using HoarderBud;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace LC_HoardingBugSnacks.Patches
{
    public class BugSnacks : GrabbableObject
    {

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            if (enemy is HoarderBugAI)
            {
                var bug = (HoarderBugAI)enemy;
                BecomeFriends(bug, 15f);
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
            float Frequency = 5;

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

                spinTrans.localRotation = Quaternion.Euler(startRotation.x, startRotation.y + time, startRotation.z);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
