#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using NiceIO;
using System.IO;
using System.Linq;
using Unity.Build;
using Unity.Build.Classic.Private.IncrementalClassicPipeline;
using UnityEditor;

namespace Unity.Build.macOS.Classic
{
    class GraphSetupNativePlugins : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var classicContext = context.GetValue<IncrementalClassicSharedData>();
            var buildTarget = classicContext.BuildTarget;

            var nativePlugins = PluginImporter.GetImporters(buildTarget).Where(m => m.isNativePlugin);
            foreach (var p in nativePlugins)
            {
                string cpu = p.GetPlatformData(buildTarget, "CPU");
                switch (cpu)
                {
                    case "x86_64":
                        CopyTool.Instance().Setup(classicContext.Architectures[Architecture.x64].DynamicLibraryDeployDirectory.Combine(cpu, Path.GetFileName(p.assetPath)), new NPath(p.assetPath).MakeAbsolute());
                        break;
                    // This is a special case for CPU targets, means no valid CPU is selected
                    case "None":
                        continue;
                }
            }
            return context.Success();
        }
    }
}
#endif
