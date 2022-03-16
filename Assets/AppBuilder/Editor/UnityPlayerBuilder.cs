using System;
using System.IO;
using System.Linq;
using Builds;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AppBuilder
{
    public partial class UnityPlayerBuilder : IUnityPlayerBuilder
    {
        private BuildPlayerOptions _buildOptions;
        public BuildConfigureRecorder Recorder { get; } = new();

        public string[] Scenes
        {
            set => _buildOptions.scenes = value;
        }

        private string _outputDirectory;

        public string OutputPath
        {
            set => _outputDirectory = value.Replace("\\", "/");
            get { return _outputDirectory; }
        }

        public override string ToString()
        {
            return Recorder.ToString();
        }

        public BuildExecutor Build()
        {
            if (!Path.HasExtension(_outputDirectory))
            {
                _buildOptions.locationPathName = Path.ChangeExtension(
                    _outputDirectory,
                    _buildOptions.target switch
                    {
                        BuildTarget.Android => ".apk",
                        _ => _outputDirectory
                    });
            }
            else
            {
                _buildOptions.locationPathName = _outputDirectory;
            }

            if (_buildOptions.scenes == null || !_buildOptions.scenes.Any())
            {
                Recorder.Write(new BuildProperty("Scenes", string.Empty));
            }
            else
            {
                Recorder.Write(new BuildProperty("Scenes", _buildOptions.scenes[0]));

                for (int i = 1; i < _buildOptions.scenes.Length; i++)
                {
                    Recorder.Write(new BuildProperty(string.Empty, _buildOptions.scenes[i]));
                }
            }

            Recorder.Write("BuildTarget", _buildOptions.target.ToString());
            Recorder.Write("BuildTargetGroup", _buildOptions.targetGroup.ToString());
            Recorder.Write("OutputPath", _buildOptions.locationPathName);

            return new BuildExecutor(_buildOptions, Recorder.Export());
        }
    }

    public class BuildExecutor
    {
        private readonly Action[] _configurations;
        public BuildPlayerOptions Options { get; }


        public BuildExecutor(BuildPlayerOptions options, Action[] configurations)
        {
            _configurations = configurations;
            Options = options;
        }

        public void Validate()
        {
            Options.Validate();
        }

        public void Configure()
        {
            if (!Application.isBatchMode)
            {
                switch (Options.target)
                {
                    case BuildTarget.Android:
                        var isSuccess =
                            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                                BuildTarget.Android);
                        if (!isSuccess)
                        {
                            throw new Exception("[AppBuilder] SwitchPlatform Failed!");
                        }

                        break;
                }
            }
            
            foreach (var configuration in _configurations)
            {
                configuration.Invoke();
            }
        }
        
        public BuildReport Execute()
        {
            Configure();
         
            Debug.Log($"[AppBuilder] {JsonConvert.SerializeObject(Options)}");
            return BuildPipeline.BuildPlayer(Options);
        }
    }

    public partial class UnityPlayerBuilder
    {
        public void ConfigureAndroid(Action<AndroidSettingsBuilder> configuration)
        {
            _buildOptions.target = BuildTarget.Android;
            _buildOptions.targetGroup = BuildTargetGroup.Android;
            var builder = new AndroidSettingsBuilder(Recorder);
            configuration(builder);
        }
    }

    public static class OptionsExtensions
    {
        public static void WriteScriptable<TConfig>(this IOptions<TConfig> source, string path) where TConfig : class
        {
            var output = Resources.Load<OptionsScriptableObject<TConfig>>(path);
            output.Value = source.Value;
        }
    }
}