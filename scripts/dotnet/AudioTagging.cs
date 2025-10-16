/// Copyright (c)  2025  Xiaomi Corporation (authors: Fangjun Kuang)
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace SherpaOnnx
{
    public class AudioTagging : IDisposable
    {
        public AudioTagging(AudioTaggingConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateAudioTagging(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateAudioTagging returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyAudioTagging);
        }

        public OfflineStream CreateStream()
        {
            IntPtr p = SherpaOnnxAudioTaggingCreateOfflineStream(Handle);
            return new OfflineStream(p);
        }

        // if topK <= 0, then config.TopK is used
        // if topK > 0, then config.TopK is ignored
        public AudioEvent[] Compute(OfflineStream stream, int topK = -1)
        {
            IntPtr p = SherpaOnnxAudioTaggingCompute(Handle, stream.Handle, topK);

            var result = new List<AudioEvent>();

            if (p == IntPtr.Zero)
            {
              return result.ToArray();
            }

            int index = 0;
            while (true)
            {
              IntPtr e = Marshal.ReadIntPtr(p, index * IntPtr.Size);
              if (e == IntPtr.Zero)
              {
                break;
              }

              AudioEvent ae = new AudioEvent(e);
              result.Add(ae);

              ++index;
            }

            SherpaOnnxAudioTaggingFreeResults(p);

            return result.ToArray();
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~AudioTagging()
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

        private static IntPtr SherpaOnnxCreateAudioTagging(ref AudioTaggingConfig config)
        {
            AudioTaggingConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateAudioTagging(ref configCopy), out var result))
            {
                return result;
            }

            return NativeExternal.SherpaOnnxCreateAudioTagging(ref config);
        }

        private static void SherpaOnnxDestroyAudioTagging(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyAudioTagging(handle),
                () => NativeExternal.SherpaOnnxDestroyAudioTagging(handle));
        }

        private static IntPtr SherpaOnnxAudioTaggingCreateOfflineStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxAudioTaggingCreateOfflineStream(handle),
                () => NativeExternal.SherpaOnnxAudioTaggingCreateOfflineStream(handle));
        }

        private static IntPtr SherpaOnnxAudioTaggingCompute(IntPtr handle, IntPtr stream, int topK)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxAudioTaggingCompute(handle, stream, topK),
                () => NativeExternal.SherpaOnnxAudioTaggingCompute(handle, stream, topK));
        }

        private static void SherpaOnnxAudioTaggingFreeResults(IntPtr p)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxAudioTaggingFreeResults(p),
                () => NativeExternal.SherpaOnnxAudioTaggingFreeResults(p));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateAudioTagging(ref AudioTaggingConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyAudioTagging(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxAudioTaggingCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxAudioTaggingCompute(IntPtr handle, IntPtr stream, int topK);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxAudioTaggingFreeResults(IntPtr p);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateAudioTagging(ref AudioTaggingConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyAudioTagging(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxAudioTaggingCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxAudioTaggingCompute(IntPtr handle, IntPtr stream, int topK);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxAudioTaggingFreeResults(IntPtr p);
        }

        #endregion
    }
}
