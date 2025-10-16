/// Copyright (c)  2024  Xiaomi Corporation (authors: Fangjun Kuang)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SherpaOnnx
{
    // please see
    // https://www.mono-project.com/docs/advanced/pinvoke/#gc-safe-pinvoke-code
    // https://www.mono-project.com/docs/advanced/pinvoke/#properly-disposing-of-resources
    public class KeywordSpotter : IDisposable
    {
        public KeywordSpotter(KeywordSpotterConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateKeywordSpotter(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateKeywordSpotter returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyKeywordSpotter);
        }

        public OnlineStream CreateStream()
        {
            IntPtr p = SherpaOnnxCreateKeywordStream(Handle);
            return new OnlineStream(p);
        }

        public OnlineStream CreateStream(string keywords)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(keywords);
            byte[] utf8BytesWithNull = new byte[utf8Bytes.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Bytes, utf8BytesWithNull, utf8Bytes.Length);
            utf8BytesWithNull[utf8Bytes.Length] = 0; // Null terminator
            IntPtr p = SherpaOnnxCreateKeywordStreamWithKeywords(Handle, utf8BytesWithNull);
            return new OnlineStream(p);
        }

        /// Return true if the passed stream is ready for decoding.
        public bool IsReady(OnlineStream stream)
        {
            return IsReady(Handle, stream.Handle) != 0;
        }

        /// You have to ensure that IsReady(stream) returns true before
        /// you call this method
        public void Decode(OnlineStream stream)
        {
            Decode(Handle, stream.Handle);
        }

        public void Reset(OnlineStream stream)
        {
            Reset(Handle, stream.Handle);
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

        public KeywordResult GetResult(OnlineStream stream)
        {
            IntPtr h = GetResult(Handle, stream.Handle);
            KeywordResult result = new KeywordResult(h);
            DestroyResult(h);
            return result;
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~KeywordSpotter()
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

        private NativeResourceHandle _handle;

        private IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateKeywordSpotter(ref KeywordSpotterConfig config)
        {
            KeywordSpotterConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateKeywordSpotter(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateKeywordSpotter(ref config);
        }

        private static void SherpaOnnxDestroyKeywordSpotter(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyKeywordSpotter(handle),
                () => NativeExternal.SherpaOnnxDestroyKeywordSpotter(handle));
        }

        private static IntPtr SherpaOnnxCreateKeywordStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateKeywordStream(handle),
                () => NativeExternal.SherpaOnnxCreateKeywordStream(handle));
        }

        private static IntPtr SherpaOnnxCreateKeywordStreamWithKeywords(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Keywords)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateKeywordStreamWithKeywords(handle, utf8Keywords),
                () => NativeExternal.SherpaOnnxCreateKeywordStreamWithKeywords(handle, utf8Keywords));
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

        private static void Reset(IntPtr handle, IntPtr stream)
        {
            Dll.Invoke(
                () => NativeInternal.Reset(handle, stream),
                () => NativeExternal.Reset(handle, stream));
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

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordSpotter(ref KeywordSpotterConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyKeywordSpotter(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordStream(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordStreamWithKeywords(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Keywords);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxIsKeywordStreamReady")]
            internal static extern int IsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeKeywordStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxResetKeywordStream")]
            internal static extern void Reset(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDecodeMultipleKeywordStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxGetKeywordResult")]
            internal static extern IntPtr GetResult(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDestroyKeywordResult")]
            internal static extern void DestroyResult(IntPtr result);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordSpotter(ref KeywordSpotterConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyKeywordSpotter(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordStream(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateKeywordStreamWithKeywords(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Keywords);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxIsKeywordStreamReady")]
            internal static extern int IsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeKeywordStream")]
            internal static extern void Decode(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxResetKeywordStream")]
            internal static extern void Reset(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDecodeMultipleKeywordStreams")]
            internal static extern void Decode(IntPtr handle, IntPtr[] streams, int n);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxGetKeywordResult")]
            internal static extern IntPtr GetResult(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDestroyKeywordResult")]
            internal static extern void DestroyResult(IntPtr result);
        }

        #endregion
    }
}
