using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SherpaOnnx
{
    internal static class Dll
    {
        public const string InternalFilename = "__Internal";
        public const string Filename = "sherpa-onnx-c-api";

        private const string BindingEnvVariable = "SHERPA_ONNX_BINDING";
        private const string ForceInternalEnvVariable = "SHERPA_ONNX_FORCE_INTERNAL";
        private const string ForceExternalEnvVariable = "SHERPA_ONNX_FORCE_EXTERNAL";

        private enum BindingMode : int
        {
            InternalPreferred = 0,
            InternalOnly = 1,
            ExternalOnly = 2,
        }

        internal delegate TResult ResultFactory<TResult>();
        internal delegate void VoidAction();

        private static int _bindingState = (int)BindingMode.InternalPreferred;

        static Dll()
        {
            var forced = GetEnvironmentFlag();
            if (forced.HasValue)
            {
                _bindingState = forced.Value;
                return;
            }

            if (IsAppleMobilePlatform() || IsUnityIl2Cpp())
            {
                _bindingState = (int)BindingMode.InternalOnly;
            }
        }

        internal static TResult Invoke<TResult>(ResultFactory<TResult> internalCall, ResultFactory<TResult> externalCall)
        {
            if (TryInvokeInternal(internalCall, out var result))
            {
                return result;
            }

            return ExecuteExternal(externalCall);
        }

        internal static void Invoke(VoidAction internalCall, VoidAction externalCall)
        {
            if (InvokeInternal(internalCall))
            {
                return;
            }

            ExecuteExternal(externalCall);
        }

        internal static bool TryInvokeInternal<TResult>(ResultFactory<TResult> internalCall, out TResult result)
        {
            result = default(TResult);

            if (!CanUseInternal())
            {
                return false;
            }

            try
            {
                var value = internalCall();
                MarkInternalSuccess();
                result = value;
                return true;
            }
            catch (DllNotFoundException)
            {
                MarkExternalFallback();
            }
            catch (EntryPointNotFoundException)
            {
                MarkExternalFallback();
            }

            return false;
        }

        internal static bool InvokeInternal(VoidAction internalCall)
        {
            if (!CanUseInternal())
            {
                return false;
            }

            try
            {
                internalCall();
                MarkInternalSuccess();
                return true;
            }
            catch (DllNotFoundException)
            {
                MarkExternalFallback();
            }
            catch (EntryPointNotFoundException)
            {
                MarkExternalFallback();
            }

            return false;
        }

        private static bool CanUseInternal()
        {
            return Thread.VolatileRead(ref _bindingState) != (int)BindingMode.ExternalOnly;
        }

        private static TResult ExecuteExternal<TResult>(ResultFactory<TResult> externalCall)
        {
            var value = externalCall();
            MarkExternalSuccess();
            return value;
        }

        private static void ExecuteExternal(VoidAction externalCall)
        {
            externalCall();
            MarkExternalSuccess();
        }

        private static void MarkInternalSuccess()
        {
            Interlocked.Exchange(ref _bindingState, (int)BindingMode.InternalOnly);
        }

        private static void MarkExternalFallback()
        {
            Interlocked.Exchange(ref _bindingState, (int)BindingMode.ExternalOnly);
        }

        private static void MarkExternalSuccess()
        {
            Interlocked.Exchange(ref _bindingState, (int)BindingMode.ExternalOnly);
        }

        private static int? GetEnvironmentFlag()
        {
            string binding = Environment.GetEnvironmentVariable(BindingEnvVariable);
            if (!IsNullOrWhiteSpace(binding))
            {
                if (IsTrue(binding) || string.Equals(binding, "internal", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(binding, "__internal", StringComparison.OrdinalIgnoreCase))
                {
                    return (int)BindingMode.InternalOnly;
                }

                if (string.Equals(binding, "external", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(binding, "dll", StringComparison.OrdinalIgnoreCase))
                {
                    return (int)BindingMode.ExternalOnly;
                }
            }

            if (IsTrue(Environment.GetEnvironmentVariable(ForceInternalEnvVariable)))
            {
                return (int)BindingMode.InternalOnly;
            }

            if (IsTrue(Environment.GetEnvironmentVariable(ForceExternalEnvVariable)))
            {
                return (int)BindingMode.ExternalOnly;
            }

            return null;
        }

        private static bool IsTrue(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "1":
                case "true":
                case "yes":
                case "on":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsAppleMobilePlatform()
        {
            try
            {
                var runtimeInfoType = Type.GetType("System.Runtime.InteropServices.RuntimeInformation, System.Runtime.InteropServices.RuntimeInformation");
                var osPlatformType = Type.GetType("System.Runtime.InteropServices.OSPlatform, System.Runtime.InteropServices.RuntimeInformation");

                if (runtimeInfoType == null || osPlatformType == null)
                {
                    return false;
                }

                var createMethod = osPlatformType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (createMethod == null)
                {
                    return false;
                }

                var iosPlatform = createMethod.Invoke(null, new object[] { "IOS" });
                if (iosPlatform == null)
                {
                    return false;
                }

                var isOsPlatform = runtimeInfoType.GetMethod("IsOSPlatform", BindingFlags.Public | BindingFlags.Static);
                if (isOsPlatform == null)
                {
                    return false;
                }

                if ((bool)isOsPlatform.Invoke(null, new[] { iosPlatform }))
                {
                    return true;
                }

                var tvosPlatform = createMethod.Invoke(null, new object[] { "TVOS" });
                if (tvosPlatform != null && (bool)isOsPlatform.Invoke(null, new[] { tvosPlatform }))
                {
                    return true;
                }
            }
            catch
            {
                // Ignore and fallback to the default behaviour.
            }

            return false;
        }

        private static bool IsUnityIl2Cpp()
        {
            try
            {
                var applicationType = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule") ??
                                      Type.GetType("UnityEngine.Application, UnityEngine");
                if (applicationType == null)
                {
                    return false;
                }

                var platformProperty = applicationType.GetProperty("platform", BindingFlags.Public | BindingFlags.Static);
                var platformValue = platformProperty?.GetValue(null, null)?.ToString();
                if (!string.IsNullOrEmpty(platformValue) && ContainsOrdinalIgnoreCase(platformValue, "IPhonePlayer"))
                {
                    return true;
                }

                var scriptingUtilityType = Type.GetType("UnityEngine.ScriptingUtility, UnityEngine.CoreModule") ??
                                           Type.GetType("UnityEngine.ScriptingUtility, UnityEngine");
                if (scriptingUtilityType != null)
                {
                    var isIl2CppProperty = scriptingUtilityType.GetProperty("isIL2CPP", BindingFlags.Public | BindingFlags.Static);
                    if (isIl2CppProperty != null && isIl2CppProperty.PropertyType == typeof(bool))
                    {
                        return (bool)isIl2CppProperty.GetValue(null, null);
                    }

                    var method = scriptingUtilityType.GetMethod("IsIL2CPP", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (method != null && method.ReturnType == typeof(bool))
                    {
                        return (bool)method.Invoke(null, null);
                    }
                }
            }
            catch
            {
                // Ignore reflection errors; we'll fall back to default binding behaviour.
            }

            return false;
        }

        private static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; ++i)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsOrdinalIgnoreCase(string source, string value)
        {
            if (source == null || value == null)
            {
                return false;
            }

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
