/// Copyright (c)  2023  Xiaomi Corporation (authors: Fangjun Kuang)
/// Copyright (c)  2023 by manyeyes
/// Copyright (c)  2024.5 by 东风破
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    // please see
    // https://www.mono-project.com/docs/advanced/pinvoke/#gc-safe-pinvoke-code
    // https://www.mono-project.com/docs/advanced/pinvoke/#properly-disposing-of-resources
    public class OnlineRecognizer : IDisposable
    {
        public OnlineRecognizer(OnlineRecognizerConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateOnlineRecognizer(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateOnlineRecognizer returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyOnlineRecognizer);
        }

        public OnlineStream CreateStream()
        {
            IntPtr p = SherpaOnnxCreateOnlineStream(Handle);
            return new OnlineStream(p);
        }

        /// Return true if the passed stream is ready for decoding.
        public bool IsReady(OnlineStream stream)
        {
            return IsReady(Handle, stream.Handle) != 0;
        }

        /// Return true if an endpoint is detected for this stream.
        /// You probably need to invoke Reset(stream) when this method returns
        /// true.
        public bool IsEndpoint(OnlineStream stream)
        {
            return SherpaOnnxOnlineStreamIsEndpoint(Handle, stream.Handle) != 0;
        }

        /// You have to ensure that IsReady(stream) returns true before
        /// you call this method
        public void Decode(OnlineStream stream)
        {
            Decode(Handle, stream.Handle);
        }

        // The caller should ensure all passed streams are ready for decoding.
        public void Decode(IEnumerable<OnlineStream> streams)
        {
            // TargetFramework=net20 does not support System.Linq
            // IntPtr[] ptrs = streams.Select(s => s.Handle).ToArray();
            List<IntPtr> list = new List<IntPtr>();
            foreach (OnlineStream s in streams)
            {
                list.Add(s.Handle);
            }

            IntPtr[] ptrs = list.ToArray();
            Decode(Handle, ptrs, ptrs.Length);
        }

        public OnlineRecognizerResult GetResult(OnlineStream stream)
        {
            IntPtr h = GetResult(Handle, stream.Handle);
            OnlineRecognizerResult result = new OnlineRecognizerResult(h);
            DestroyResult(h);
            return result;
        }

        /// When this method returns, IsEndpoint(stream) will return false.
        public void Reset(OnlineStream stream)
        {
            SherpaOnnxOnlineStreamReset(Handle, stream.Handle);
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~OnlineRecognizer()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_handle != null)
            {
                _handle.Dispose();
                _handle = null;
            }
        }

        private IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }

        private NativeResourceHandle _handle;
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateOnlineRecognizer(ref OnlineRecognizerConfig config)
        {
            OnlineRecognizerConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateOnlineRecognizer(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateOnlineRecognizer(ref config);
        }

        private static void SherpaOnnxDestroyOnlineRecognizer(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOnlineRecognizer(handle),
                () => NativeExternal.SherpaOnnxDestroyOnlineRecognizer(handle));
        }

        private static IntPtr SherpaOnnxCreateOnlineStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateOnlineStream(handle),
                () => NativeExternal.SherpaOnnxCreateOnlineStream(handle));
        }

        private static int IsReady(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.IsReady(handle, stream),
                () => NativeExternal.IsReady(handle, stream));
        }

        private static void Decode(IntPtr handle, IntPtr stream)
        {
            Dll.Invoke(
                () => NativeInternal.Decode(handle, stream),
                () => NativeExternal.Decode(handle, stream));
        }

        private static void Decode(IntPtr handle, IntPtr[] streams, int n)
        {
            Dll.Invoke(
                () => NativeInternal.Decode(handle, streams, n),
                () => NativeExternal.Decode(handle, streams, n));
        }

        private static IntPtr GetResult(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.GetResult(handle, stream),
                () => NativeExternal.GetResult(handle, stream));
        }

        private static void DestroyResult(IntPtr result)
        {
            Dll.Invoke(
                () => NativeInternal.DestroyResult(result),
                () => NativeExternal.DestroyResult(result));
        }

        private static void SherpaOnnxOnlineStreamReset(IntPtr handle, IntPtr stream)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxOnlineStreamReset(handle, stream),
                () => NativeExternal.SherpaOnnxOnlineStreamReset(handle, stream));
        }

        private static int SherpaOnnxOnlineStreamIsEndpoint(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOnlineStreamIsEndpoint(handle, stream),
                () => NativeExternal.SherpaOnnxOnlineStreamIsEndpoint(handle, stream));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOnlineRecognizer(ref OnlineRecognizerConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOnlineRecognizer(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOnlineStream(IntPtr handle);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxIsOnlineStreamReady")]
            internal static extern int IsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeOnlineStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeMultipleOnlineStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxGetOnlineStreamResult")]
            internal static extern IntPtr GetResult(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDestroyOnlineRecognizerResult")]
            internal static extern void DestroyResult(IntPtr result);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOnlineStreamReset(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxOnlineStreamIsEndpoint(IntPtr handle, IntPtr stream);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOnlineRecognizer(ref OnlineRecognizerConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOnlineRecognizer(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOnlineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxIsOnlineStreamReady")]
            internal static extern int IsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeOnlineStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeMultipleOnlineStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxGetOnlineStreamResult")]
            internal static extern IntPtr GetResult(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDestroyOnlineRecognizerResult")]
            internal static extern void DestroyResult(IntPtr result);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOnlineStreamReset(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxOnlineStreamIsEndpoint(IntPtr handle, IntPtr stream);
        }

        #endregion
    }
}
