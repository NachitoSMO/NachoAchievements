using HarmonyLib;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(TerrainObstacleTrigger))]
    internal class TerrainObstacleTriggerPatches
    {
        public static bool driverWhoCollidedIsSelf = false;

        [HarmonyPatch(nameof(TerrainObstacleTrigger.OnTriggerEnter))]
        [HarmonyPrefix]
        private static void OnCarHit(TerrainObstacleTrigger __instance, Collider other)
        {
            VehicleController component = other.GetComponent<VehicleController>();
            if (!(component == null) && component.IsOwner && component.averageVelocity.magnitude > 5f && Vector3.Angle(component.averageVelocity, __instance.transform.position - component.mainRigidbody.position) < 80f)
            {
                if (component.currentDriver == StartOfRound.Instance.localPlayerController) driverWhoCollidedIsSelf = true;
            }
        }
    }
}
