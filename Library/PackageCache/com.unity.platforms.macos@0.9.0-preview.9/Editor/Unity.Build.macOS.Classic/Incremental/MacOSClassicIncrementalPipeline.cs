#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Diagnostics;
using Bee.Core;
using Bee.Toolchain.MacOS;
using Unity.Build;
using Unity.Build.Classic;
using Unity.Build.Classic.Private;
using Unity.Build.Classic.Private.IncrementalClassicPipeline;
using Unity.Build.Common;
using Bee.Tools;
using UnityEditor;

namespace Unity.Build.macOS.Classic
{
    class MacOSClassicIncrementalPipeline : ClassicIncrementalPipelineBase
    {
        public override Platform Platform => new MacOSXPlatform();
        protected override BuildTarget BuildTarget => BuildTarget.StandaloneOSX;

        public override BuildStepCollection BuildSteps { get; } = new[]
        {
            typeof(SetupCopiesFromSlimPlayerBuild),
            typeof(GraphCopyDefaultResources),
            typeof(GraphSetupCodeGenerationStep),
            typeof(GraphSetupIl2Cpp),
            typeof(GraphSetupNativePlugins),
            typeof(GraphSetupPlayerFiles),
            typeof(SetupAdditionallyProvidedFiles)
        };

        protected override void PrepareContext(BuildContext context)
        {
            base.PrepareContext(context);

            var classicContext = context.GetValue<IncrementalClassicSharedData>();
            var appBundleContentsDirectory = $"{context.GetOutputBuildDirectory()}/{context.GetComponentOrDefault<GeneralSettings>().ProductName}.app/Contents";
            var appBundleResourcesDirectory = $"{appBundleContentsDirectory}/Resources";

            var appBundlePluginsDirectory = $"{appBundleContentsDirectory}/Plugins";
            classicContext.DataDeployDirectory = $"{appBundleResourcesDirectory}/Data";

            var appBundleFrameworksDirectory = $"{appBundleContentsDirectory}/Frameworks";
            classicContext.DataDeployDirectory = $"{appBundleResourcesDirectory}/Data";

            var classicData = context.GetValue<ClassicSharedData>();
            classicData.StreamingAssetsDirectory = $"{classicContext.DataDeployDirectory}/StreamingAssets";

            var scriptingSettings = context.GetComponentOrDefault<ClassicScriptingSettings>();
            var scriptingTag = scriptingSettings.ScriptingBackend == ScriptingImplementation.Mono2x ? "mono" : "il2cpp";

            classicContext.VariationDirectory = classicContext.PlayerPackageDirectory.Combine("Variations", classicData.DevelopmentPlayer ? $"macosx64_development_{scriptingTag}" : $"macosx64_nondevelopment_{scriptingTag}").MakeAbsolute();
            classicContext.UnityEngineAssembliesDirectory = classicContext.VariationDirectory.Combine("Data", "Managed");
            classicContext.IL2CPPDataDirectory = classicContext.DataDeployDirectory.Combine("il2cpp_data");
            classicContext.LibraryDeployDirectory = appBundleFrameworksDirectory;

            var toolchain = new MacToolchain(MacSdk.Locatorx64.UserDefaultOrLatest);
            classicContext.Architectures.Add(
                Architecture.x64,
                new ClassicBuildArchitectureData
                {
                    DynamicLibraryDeployDirectory = appBundlePluginsDirectory,
                    IL2CPPLibraryDirectory = appBundleFrameworksDirectory,
                    BurstTarget = "x64_SSE4",
                    ToolChain = toolchain,
                    NativeProgramFormat = toolchain.DynamicLibraryFormat
                }
            );
        }

        protected override CleanResult OnClean(CleanContext context)
        {
            var buildType = context.GetComponentOrDefault<ClassicBuildProfile>().Configuration;
            bool isDevelopment = buildType == BuildType.Debug || buildType == BuildType.Develop;
            var playbackEngineDirectory = new NPath(UnityEditor.BuildPipeline.GetPlaybackEngineDirectory(BuildTarget, isDevelopment ? BuildOptions.Development : BuildOptions.None));

            if (context.HasComponent<InstallInBuildFolder>())
            {
                NPath sourceBuildDirectory = $"{playbackEngineDirectory}/SourceBuild/{context.BuildConfigurationName}";

                if (sourceBuildDirectory.DirectoryExists())
                    sourceBuildDirectory.Delete();
            }
            return base.OnClean(context);
        }

        protected override BoolResult OnCanRun(RunContext context)
        {
#if UNITY_STANDALONE_OSX
            return BoolResult.True();
#else
            return BoolResult.False("Active Editor platform has to be set to macOS Standalone.");
#endif
        }
        protected override RunResult OnRun(RunContext context)
        {
            var artifact = context.GetLastBuildArtifact<MacOSArtifact>();
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "open",
                    Arguments = artifact.OutputTargetFile.FullName.InQuotes(),
                    WorkingDirectory = artifact.OutputTargetFile.Directory?.FullName ?? string.Empty,
                    CreateNoWindow = true,
                    UseShellExecute = true
                }
            };

            return !process.Start() ? context.Failure($"Failed to start process at '{process.StartInfo.FileName}'.") : context.Success(new MacOSRunInstance(process));
        }
    }
}
#endif
