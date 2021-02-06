#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using NiceIO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Build;
using Unity.Build.Classic.Private.IncrementalClassicPipeline;
using Unity.Build.Common;
using UnityEditor;
using UnityEditorInternal;

namespace Unity.Build.macOS.Classic
{
    class GraphSetupPlayerFiles : BuildStepBase
    {
        public override Type[] UsedComponents { get; } = { typeof(GeneralSettings) };

        public override BuildResult Run(BuildContext context)
        {
            var classicContext = context.GetValue<IncrementalClassicSharedData>();
            var playerDirectory = classicContext.VariationDirectory;
            var generalSettings = context.GetComponentOrDefault<GeneralSettings>();
            var productName = generalSettings.ProductName;
            var appName = new NPath(productName + ".app");
            var appData = appName.Combine("Contents", "Resources", "Data");

            NPath outputBuildDirectory = new NPath(context.GetOutputBuildDirectory()).MakeAbsolute();
            foreach (var file in playerDirectory.Files(true))
            {
                if (file.Parent.FileName == "Managed")
                    continue;

                if (file.FileName == ".DS_Store")
                    continue;

                var targetRelativePath = file.RelativeTo(playerDirectory);
                if (targetRelativePath.ToString().StartsWith("Data/"))
                    targetRelativePath = appData;

                // Replace UnityPlayer.app (from the player package) to the output app's name
                var modifiedTargetRelativePath = targetRelativePath.ToString()
                    .Replace("UnityPlayer.app", appName.ToString());

                // Also rename the binary
                if (file.FileName == "UnityPlayer")
                {
                    modifiedTargetRelativePath = modifiedTargetRelativePath.Replace("UnityPlayer", productName);
                }

                CopyTool.Instance().Setup(outputBuildDirectory.Combine(modifiedTargetRelativePath),
                    file.MakeAbsolute());
            }

            // Copy and modify Info.plist
            var plistPath = classicContext.PlayerPackageDirectory.Combine("Source", "Player", "MacPlayer",
                "MacPlayerEntryPoint", "Info.plist");
            var plistOutputPath = outputBuildDirectory.Combine(appName, "Contents", "Info.plist");

            outputBuildDirectory.Combine(appName, "Contents").CreateDirectory();
            UpdateInfoPlist(plistPath.ToString(), plistOutputPath.ToString(), productName);

            // Bring over any Mono support dylibs
            var frameworksDirectory = classicContext.PlayerPackageDirectory.Combine("Frameworks");
            foreach (var file in frameworksDirectory.Files(true))
            {
                CopyTool.Instance().Setup(outputBuildDirectory.Combine(appName, "Contents", "Frameworks", file.FileName),
                    file.MakeAbsolute());
            }
            
            var appInfo = outputBuildDirectory.Combine(appData, "app.info");
            Backend.Current.AddWriteTextAction(appInfo,
                string.Join("\n", new[]
                {
                    generalSettings.CompanyName,
                    generalSettings.ProductName
                }));

            var artifact = context.GetOrCreateValue<MacOSArtifact>();
            artifact.OutputTargetFile =
                new FileInfo(outputBuildDirectory.Combine(appName).ToString());

            return context.Success();
        }

        private void UpdateInfoPlist(string sourcePath, string outputPath, string executableName)
        {
            string text = File.ReadAllText(sourcePath);
            try
            {
                var doc = new PlistDocument();
                doc.ReadFromString(text);

                var root = doc.root;
                root.SetString("CFBundleGetInfoString",
                    string.Format("Unity Player version {0}. {1}", InternalEditorUtility.GetFullUnityVersion(),
                        InternalEditorUtility.GetUnityCopyright())
                );
                root.SetString("CFBundleExecutable", executableName);
                root.SetString("CFBundleIdentifier",
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone));
                root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
                root.SetString("CFBundleVersion", PlayerSettings.macOS.buildNumber);
                root.SetString("CFBundleName", executableName);
                root.SetString("UnityBuildNumber",
                    Regex.Match(InternalEditorUtility.GetFullUnityVersion(), @"\((.+)\)").Groups[1].Value);

                text = doc.WriteToString();
            }
            catch (Exception)
            {
                // Incorrect Info.Plist is not a fatal error. Ignore
                return;
            }

            File.WriteAllText(outputPath, text);
        }
    }
}
#endif
