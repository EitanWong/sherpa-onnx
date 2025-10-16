/// Copyright (c)  2024.5 by 东风破

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class OfflineRecognizer : IDisposable
    {
        public OfflineRecognizer(OfflineRecognizerConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateOfflineRecognizer(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateOfflineRecognizer returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyOfflineRecognizer);
        }

        public void SetConfig(OfflineRecognizerConfig config)
        {
            SherpaOnnxOfflineRecognizerSetConfig(Handle, ref config);
        }

        public OfflineStream CreateStream()
        {
            IntPtr p = SherpaOnnxCreateOfflineStream(Handle);
            return new OfflineStream(p);
        }

        public void Decode(OfflineStream stream)
        {
            Decode(Handle, stream.Handle);
        }

        // The caller should ensure all passed streams are ready for decoding.
        public void Decode(IEnumerable<OfflineStream> streams)
        {
            // TargetFramework=net20 does not support System.Linq
            // IntPtr[] ptrs = streams.Select(s => s.Handle).ToArray();
            List<IntPtr> list = new List<IntPtr>();
            foreach (OfflineStream s in streams)
            {
                list.Add(s.Handle);
            }
            IntPtr[] ptrs = list.ToArray();
            Decode(Handle, ptrs, ptrs.Length);
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~OfflineRecognizer()
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

        private static IntPtr SherpaOnnxCreateOfflineRecognizer(ref OfflineRecognizerConfig config)
        {
            OfflineRecognizerConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateOfflineRecognizer(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateOfflineRecognizer(ref config);
        }

        private static void SherpaOnnxOfflineRecognizerSetConfig(IntPtr handle, ref OfflineRecognizerConfig config)
        {
            OfflineRecognizerConfig configCopy = config;
            if (Dll.InvokeInternal(() => NativeInternal.SherpaOnnxOfflineRecognizerSetConfig(handle, ref configCopy)))
            {
                return;
            }
            NativeExternal.SherpaOnnxOfflineRecognizerSetConfig(handle, ref config);
        }

        private static void SherpaOnnxDestroyOfflineRecognizer(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflineRecognizer(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflineRecognizer(handle));
        }

        private static IntPtr SherpaOnnxCreateOfflineStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateOfflineStream(handle),
                () => NativeExternal.SherpaOnnxCreateOfflineStream(handle));
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

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineRecognizer(ref OfflineRecognizerConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOfflineRecognizerSetConfig(IntPtr handle, ref OfflineRecognizerConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflineRecognizer(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeOfflineStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeMultipleOfflineStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineRecognizer(ref OfflineRecognizerConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOfflineRecognizerSetConfig(IntPtr handle, ref OfflineRecognizerConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflineRecognizer(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeOfflineStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeMultipleOfflineStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);
        }

        #endregion
    }

}
