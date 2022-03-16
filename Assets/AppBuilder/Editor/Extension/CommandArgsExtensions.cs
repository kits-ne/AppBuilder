using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AppBuilder
{
    public static class CommandArgsExtensions
    {
        public static string ToArgsString(this Dictionary<string, string> args, StringBuilder builder = null)
        {
            builder ??= new StringBuilder();

            builder.AppendLine("[CommandArgs]");
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    builder.AppendLine($"-{arg.Key}");
                }
                else
                {
                    builder.AppendLine($"-{arg.Key} {arg.Value}");
                }
            }

            return builder.ToString();
        }

        private static void Reserve(this Dictionary<string, string> args, string key, string value)
        {
            if (args.ContainsKey(key))
            {
                throw new ArgumentException("Contains Reserve Args");
            }

            args.Add(key, value);
        }

        public static Dictionary<string, string> AddReserveArguments(this Dictionary<string, string> args)
        {
            args.Reserve("productName", Application.productName);
            return args;
        }

        public static void Merge(this Dictionary<string, string> args, Dictionary<string, string> other)
        {
            if (other == null) throw new ArgumentNullException();
            if (other.Count == 0) return;

            foreach (var arg in other)
            {
                args[arg.Key] = arg.Value;
            }
        }

        private static HashSet<string> _unityArgs = new()
        {
            "useHub",
            "hubIPC",
            "cloudEnvironment",
            "licensingIpc",
            "hubSessionId",
            "accessToken",
        };

        public static IEnumerable<KeyValuePair<string, string>> IgnoreUnityArgs(this Dictionary<string, string> args)
        {
            return args.Where(pair => !_unityArgs.Contains(pair.Key));
        }

        public static void EnableArg(this Dictionary<string, string> args, string key, bool enable)
        {
            if (enable)
            {
                args.Add(key, string.Empty);
            }
            else
            {
                args.Remove(key);
            }
        }
    }
}