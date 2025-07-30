#nullable enable
using HarmonyLib;
using UnityEditor;

namespace Rumi.CustomBinding.Editor.Patches
{
    public static partial class Patches
    {
        public static readonly Harmony harmony = new Harmony("Rumi.CustomBinding");

        [InitializeOnLoadMethod]
        static void Awaken()
        {
            harmony.UnpatchSelf();
            harmony.PatchAll();
        }
    }
}
