using AppBuilder;
using AppBuilder.UI;
using AppBuilderSample;
using UnityEngine;

public static partial class Builds
{
    [Build("Export configs to runtime")]
    [AppSettings("{sample}/Export configs to runtime")]
    [Variant("Production", "Development")]
    public static void ExportConfigs()
    {
        BuildPlayer.Build((args) => { args.Add("sample", PackageInfo.SamplesPath); }, (ctx, builder) =>
        {
            builder.ConfigureCurrentSettings();

            var config = ctx.GetConfiguration<Config>();
            config.WriteScriptable("AppBuilderSample/ExportConfigs/Config");
 
            builder.UseVariantProductName(ctx);

            builder.ConfigureAndroid(android =>
            {
                android.PackageName(ctx.GetSection<string>("PackageName"));
                android.UseDebugKeystore();
            });
            using (builder.Display(out var add))
            {
                add("Host", config.Value.Host);
                add("AppId", config.Value.AppId);
            }
        });
    }
}
