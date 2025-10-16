/// Copyright (c)  2024  Xiaomi Corporation (authors: Fangjun Kuang)

using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class CircularBuffer : IDisposable
    {
        public CircularBuffer(int capacity)
        {
            IntPtr pointer = SherpaOnnxCreateCircularBuffer(capacity);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateCircularBuffer returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyCircularBuffer);
        }

        public void Push(float[] data)
        {
            SherpaOnnxCircularBufferPush(Handle, data, data.Length);
        }

        public float[] Get(int startIndex, int n)
        {
            IntPtr p = SherpaOnnxCircularBufferGet(Handle, startIndex, n);

            float[] ans = new float[n];
            Marshal.Copy(p, ans, 0, n);

            SherpaOnnxCircularBufferFree(p);

            return ans;
        }

        public void Pop(int n)
        {
            SherpaOnnxCircularBufferPop(Handle, n);
        }

        public int Size
        {
            get
            {
                return SherpaOnnxCircularBufferSize(Handle);
            }
        }

        public int Head
        {
            get
            {
                return SherpaOnnxCircularBufferHead(Handle);
            }
        }

        public void Reset()
        {
            SherpaOnnxCircularBufferReset(Handle);
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~CircularBuffer()
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

        private static IntPtr SherpaOnnxCreateCircularBuffer(int capacity)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateCircularBuffer(capacity),
                () => NativeExternal.SherpaOnnxCreateCircularBuffer(capacity));
        }

        private static void SherpaOnnxDestroyCircularBuffer(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyCircularBuffer(handle),
                () => NativeExternal.SherpaOnnxDestroyCircularBuffer(handle));
        }

        private static void SherpaOnnxCircularBufferPush(IntPtr handle, float[] p, int n)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferPush(handle, p, n),
                () => NativeExternal.SherpaOnnxCircularBufferPush(handle, p, n));
        }

        private static IntPtr SherpaOnnxCircularBufferGet(IntPtr handle, int startIndex, int n)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferGet(handle, startIndex, n),
                () => NativeExternal.SherpaOnnxCircularBufferGet(handle, startIndex, n));
        }

        private static void SherpaOnnxCircularBufferFree(IntPtr p)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferFree(p),
                () => NativeExternal.SherpaOnnxCircularBufferFree(p));
        }

        private static void SherpaOnnxCircularBufferPop(IntPtr handle, int n)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferPop(handle, n),
                () => NativeExternal.SherpaOnnxCircularBufferPop(handle, n));
        }

        private static int SherpaOnnxCircularBufferSize(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferSize(handle),
                () => NativeExternal.SherpaOnnxCircularBufferSize(handle));
        }

        private static int SherpaOnnxCircularBufferHead(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferHead(handle),
                () => NativeExternal.SherpaOnnxCircularBufferHead(handle));
        }

        private static void SherpaOnnxCircularBufferReset(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxCircularBufferReset(handle),
                () => NativeExternal.SherpaOnnxCircularBufferReset(handle));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateCircularBuffer(int capacity);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyCircularBuffer(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxCircularBufferPush(IntPtr handle, float[] p, int n);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCircularBufferGet(IntPtr handle, int startIndex, int n);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxCircularBufferFree(IntPtr p);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxCircularBufferPop(IntPtr handle, int n);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxCircularBufferSize(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxCircularBufferHead(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxCircularBufferReset(IntPtr handle);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateCircularBuffer(int capacity);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyCircularBuffer(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxCircularBufferPush(IntPtr handle, float[] p, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCircularBufferGet(IntPtr handle, int startIndex, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxCircularBufferFree(IntPtr p);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxCircularBufferPop(IntPtr handle, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxCircularBufferSize(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxCircularBufferHead(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxCircularBufferReset(IntPtr handle);
        }

        #endregion

    }
}
