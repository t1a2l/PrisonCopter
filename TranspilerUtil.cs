using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace PrisonCopter {


    public static class TranspilerUtil {
        /// <summary>
        /// Gets parameter types from delegate
        /// </summary>
        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <param name="instance">skip first parameter. Default value is false.</param>
        /// <returns>Type[] representing arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>(bool instance = false) where TDelegate : Delegate {
            IEnumerable<ParameterInfo> parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            if (instance) {
                parameters = parameters.Skip(1);
            }

            return parameters.Select(p => p.ParameterType).ToArray();
        }
	/// <summary>
        /// Gets directly declared method.
        /// </summary>
        /// <typeparam name="TDelegate">delegate that has the same argument types as the intented overloaded method</typeparam>
        /// <param name="type">the class/type where the method is delcared</param>
        /// <param name="name">the name of the method</param>
        /// <param name="instance">is instance delegate (require skip if the first param)</param>
        /// <returns>a method or null when type is null or when a method is not found</returns>
        internal static MethodInfo DeclaredMethod<TDelegate>(Type type, string name, bool instance = false)
            where TDelegate : Delegate {
            var args = GetParameterTypes<TDelegate>(instance);
            var ret = AccessTools.DeclaredMethod(type, name, args);
            if (ret == null)
                LogHelper.Error($"failed to retrieve method {type}.{name}({args.ToArray()})");
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
                LogHelper.Error($"failed to retrieve method {type}.{name}({types.ToArray()})");

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), ret);
        }
    }
}