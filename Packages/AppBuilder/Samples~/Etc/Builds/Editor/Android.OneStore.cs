﻿using System.IO;
using System.Linq;
using AppBuilder;
using UnityEditor;
using UnityEngine;

namespace Builds
{
    public static partial class Android
    {
        // [Build]
        [Input("outputPath", InputOptions.Directory)]
        [AppSettings("{projectpath}/Assets/AppBuilder/Samples/Etc/Builds")]
        public static void OneStore()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                ctx.GetConfiguration<AppSettings>()
                    .WriteScriptable("AppSettings", ctx.GetConfiguration().ToJson());

                builder.OutputPath = ctx.GetArgument("outputPath");
                builder.ProductName = ctx.GetSection<string>("ProductName");

                var host = ctx.GetSection<string>("Host");
                var scenes = ctx.GetSections<string>("Scenes").ToArray();

                if (scenes.Any()) builder.Scenes = scenes;
                else builder.UseEnableEditorScenes();

                builder.ConfigureAndroid(_ =>
                {
                    _.PackageName(ctx.GetSection<string>("Package"));
                    _.IL2CPP();
                    _.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                    _.SupportEmulator();
                });
            });
        }
    }
}