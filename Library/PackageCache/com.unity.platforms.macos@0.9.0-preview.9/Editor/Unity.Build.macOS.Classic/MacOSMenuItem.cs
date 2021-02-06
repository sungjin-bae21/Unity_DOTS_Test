using Bee.Core;
using System.IO;
using Unity.Build.Classic;
using Unity.Build.Common;
using Unity.Build.Editor;
using UnityEditor;

namespace Unity.Build.macOS.Classic
{
    static class MacOSMenuItem
    {
        const string k_CreateBuildConfigurationAssetClassic = BuildConfigurationMenuItem.k_BuildConfigurationMenu + "macOS Classic Build Configuration";

        [MenuItem(k_CreateBuildConfigurationAssetClassic, true)]
        static bool CreateBuildConfigurationAssetClassicValidation()
        {
            return Directory.Exists(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem(k_CreateBuildConfigurationAssetClassic)]
        static void CreateBuildConfigurationAsset()
        {
            Selection.activeObject = BuildConfigurationMenuItem.CreateAssetInActiveDirectory(
                "macOSClassic",
                new GeneralSettings(),
                new SceneList(),
                new ClassicBuildProfile
                {
                    Platform = new MacOSXPlatform()
                });
        }
    }
}
