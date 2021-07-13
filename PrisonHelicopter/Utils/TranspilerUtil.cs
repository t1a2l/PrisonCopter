using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace PrisonHelicopter.Utils {
    public static class TranspilerUtil {
        internal static Type[] GetParameterTypes<TDelegate>(bool instance = false) where TDelegate : Delegate {
            IEnumerable<ParameterInfo> parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            if (instance) {
                parameters = parameters.Skip(1);
            }

            return parameters.Select(p => p.ParameterType).ToArray();
        }

        internal static MethodInfo DeclaredMethod<TDelegate>(Type type, string name, bool instance = false)
            where TDelegate : Delegate {
            var args = GetParameterTypes<TDelegate>(instance);
            var ret = AccessTools.DeclaredMethod(type, name, args);
            if (ret == null)
                LogHelper.Error($"failed to retrieve method {type}.{name}({args.ToSTR()})");
            return ret;
        }

        public static TDelegate CreateDelegate<TDelegate>(Type type, string name, bool instance)
            where TDelegate : Delegate {

            var types = GetParameterTypes<TDelegate>(instance);
            var ret = type.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                types,
                new ParameterModifier[0]);
            if (ret == null)
                LogHelper.Error($"failed to retrieve method {type}.{name}({types.ToSTR()})");

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), ret);
        }

        internal static string ToSTR<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null)
                return "Null";
            string ret = "{ ";
            foreach (T item in enumerable) {
                ret += $"{item}, ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

    }
}
