using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        private const int StateInternalPreferred = (int)BindingMode.InternalPreferred;
        private const int StateInternalOnly = (int)BindingMode.InternalOnly;
        private const int StateExternalOnly = (int)BindingMode.ExternalOnly;
        private const BindingFlags StaticPublicBindingFlags = BindingFlags.Public | BindingFlags.Static;

        private static readonly OSPlatform AppleIosPlatform = OSPlatform.Create("IOS");
        private static readonly OSPlatform AppleTvOsPlatform = OSPlatform.Create("TVOS");

        private static readonly bool IsAppleMobile = DetectAppleMobilePlatform();
        private static readonly Type UnityScriptingUtilityType = GetTypeOrDefault(
            "UnityEngine.ScriptingUtility, UnityEngine.CoreModule",
            "UnityEngine.ScriptingUtility, UnityEngine");
        private static readonly PropertyInfo UnityScriptingUtilityIsIl2CppProperty =
            UnityScriptingUtilityType?.GetProperty("isIL2CPP", StaticPublicBindingFlags);
        private static readonly MethodInfo UnityScriptingUtilityIsIl2CppMethod =
            UnityScriptingUtilityType?.GetMethod("IsIL2CPP", StaticPublicBindingFlags, null, Type.EmptyTypes, null);
        private static readonly Type UnityApplicationType = GetTypeOrDefault(
            "UnityEngine.Application, UnityEngine.CoreModule",
            "UnityEngine.Application, UnityEngine");
        private static readonly PropertyInfo UnityApplicationPlatformProperty =
            UnityApplicationType?.GetProperty("platform", StaticPublicBindingFlags);
        private static readonly bool IsUnityIl2CppRuntime = DetectUnityIl2Cpp();

        private static volatile int _bindingState = StateInternalPreferred;

        static Dll()
        {
            var forced = GetEnvironmentFlag();
            if (forced.HasValue)
            {
                _bindingState = forced.Value;
                return;
            }

            if (IsAppleMobile || IsUnityIl2CppRuntime)
            {
                _bindingState = StateInternalOnly;
                return;
            }

            _bindingState = ProbeInternalBinding() ? StateInternalOnly : StateExternalOnly;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TResult Invoke<TResult>(ResultFactory<TResult> internalCall, ResultFactory<TResult> externalCall)
        {
            if (TryInvokeInternalCore(internalCall, out var result))
            {
                return result;
            }

            return InvokeExternal(externalCall);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Invoke(VoidAction internalCall, VoidAction externalCall)
        {
            if (TryInvokeInternalCore(internalCall))
            {
                return;
            }

            InvokeExternal(externalCall);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryInvokeInternal<TResult>(ResultFactory<TResult> internalCall, out TResult result)
        {
            return TryInvokeInternalCore(internalCall, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool InvokeInternal(VoidAction internalCall)
        {
            return TryInvokeInternalCore(internalCall);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryInvokeInternalCore<TResult>(ResultFactory<TResult> internalCall, out TResult result)
        {
            result = default;
            if (_bindingState == StateExternalOnly)
            {
                return false;
            }

            try
            {
                result = internalCall();
                _bindingState = StateInternalOnly;
                return true;
            }
            catch (DllNotFoundException)
            {
                _bindingState = StateExternalOnly;
            }
            catch (EntryPointNotFoundException)
            {
                _bindingState = StateExternalOnly;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryInvokeInternalCore(VoidAction internalCall)
        {
            if (_bindingState == StateExternalOnly)
            {
                return false;
            }

            try
            {
                internalCall();
                _bindingState = StateInternalOnly;
                return true;
            }
            catch (DllNotFoundException)
            {
                _bindingState = StateExternalOnly;
            }
            catch (EntryPointNotFoundException)
            {
                _bindingState = StateExternalOnly;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TResult InvokeExternal<TResult>(ResultFactory<TResult> externalCall)
        {
            var value = externalCall();
            _bindingState = StateExternalOnly;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InvokeExternal(VoidAction externalCall)
        {
            externalCall();
            _bindingState = StateExternalOnly;
        }

        private static int? GetEnvironmentFlag()
        {
            if (TryGetBindingMode(Environment.GetEnvironmentVariable(BindingEnvVariable), out var mode))
            {
                return mode;
            }

            if (IsTrue(Environment.GetEnvironmentVariable(ForceInternalEnvVariable)))
            {
                return StateInternalOnly;
            }

            if (IsTrue(Environment.GetEnvironmentVariable(ForceExternalEnvVariable)))
            {
                return StateExternalOnly;
            }

            return null;
        }

        private static bool TryGetBindingMode(string value, out int mode)
        {
            mode = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            var comparison = StringComparison.OrdinalIgnoreCase;

            if (IsTrue(trimmed) ||
                string.Equals(trimmed, "internal", comparison) ||
                string.Equals(trimmed, InternalFilename, comparison))
            {
                mode = StateInternalOnly;
                return true;
            }

            if (string.Equals(trimmed, "external", comparison) ||
                string.Equals(trimmed, "dll", comparison))
            {
                mode = StateExternalOnly;
                return true;
            }

            return false;
        }

        private static bool IsTrue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            return string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, "yes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static bool DetectAppleMobilePlatform()
        {
            try
            {
                return RuntimeInformation.IsOSPlatform(AppleIosPlatform) ||
                       RuntimeInformation.IsOSPlatform(AppleTvOsPlatform);
            }
            catch
            {
                return false;
            }
        }

        private static bool DetectUnityIl2Cpp()
        {
            try
            {
                if (UnityScriptingUtilityIsIl2CppProperty != null &&
                    UnityScriptingUtilityIsIl2CppProperty.PropertyType == typeof(bool) &&
                    UnityScriptingUtilityIsIl2CppProperty.GetIndexParameters().Length == 0)
                {
                    var propertyValue = UnityScriptingUtilityIsIl2CppProperty.GetValue(null, null);
                    if (propertyValue is bool boolPropertyValue)
                    {
                        return boolPropertyValue;
                    }
                }

                if (UnityScriptingUtilityIsIl2CppMethod != null &&
                    UnityScriptingUtilityIsIl2CppMethod.ReturnType == typeof(bool))
                {
                    var methodValue = UnityScriptingUtilityIsIl2CppMethod.Invoke(null, null);
                    if (methodValue is bool boolMethodValue)
                    {
                        return boolMethodValue;
                    }
                }

                var platformValue = UnityApplicationPlatformProperty?.GetValue(null, null)?.ToString();
                return !string.IsNullOrEmpty(platformValue) &&
                       platformValue.IndexOf("IPhonePlayer", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool ProbeInternalBinding()
        {
            try
            {
                BindingProbe.Touch();
                return true;
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            return false;
        }

        private static Type GetTypeOrDefault(params string[] typeNames)
        {
            if (typeNames == null)
            {
                return null;
            }

            for (int i = 0; i < typeNames.Length; ++i)
            {
                var typeName = typeNames[i];
                if (typeName == null)
                {
                    continue;
                }

                var type = Type.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static class BindingProbe
        {
            [DllImport(InternalFilename, EntryPoint = "SherpaOnnxGetVersionStr")]
            private static extern IntPtr SherpaOnnxGetVersionStr();

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void Touch()
            {
                // We only care that the call succeeds without throwing.
                SherpaOnnxGetVersionStr();
            }
        }
    }
}
