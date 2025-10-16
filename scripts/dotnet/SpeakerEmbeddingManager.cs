/// Copyright (c)  2024.5 by 东风破
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SherpaOnnx
{
    public class SpeakerEmbeddingManager : IDisposable
    {
        public SpeakerEmbeddingManager(int dim)
        {
            IntPtr pointer = SherpaOnnxCreateSpeakerEmbeddingManager(dim);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateSpeakerEmbeddingManager returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroySpeakerEmbeddingManager);
            this._dim = dim;
        }

        public bool Add(string name, float[] v)
        {
            byte[] utf8Name = Encoding.UTF8.GetBytes(name);
            byte[] utf8NameWithNull = new byte[utf8Name.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Name, utf8NameWithNull, utf8Name.Length);
            utf8NameWithNull[utf8Name.Length] = 0; // Null terminator
            return SherpaOnnxSpeakerEmbeddingManagerAdd(Handle, utf8NameWithNull, v) == 1;
        }

        public bool Add(string name, ICollection<float[]> v_list)
        {
            int n = v_list.Count;
            float[] v = new float[n * _dim];
            int i = 0;
            foreach (var item in v_list)
            {
                item.CopyTo(v, i);
                i += _dim;
            }

            byte[] utf8Name = Encoding.UTF8.GetBytes(name);
            byte[] utf8NameWithNull = new byte[utf8Name.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Name, utf8NameWithNull, utf8Name.Length);
            utf8NameWithNull[utf8Name.Length] = 0; // Null terminator
            return SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(Handle, utf8NameWithNull, v, n) == 1;
        }

        public bool Remove(string name)
        {
            byte[] utf8Name = Encoding.UTF8.GetBytes(name);
            byte[] utf8NameWithNull = new byte[utf8Name.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Name, utf8NameWithNull, utf8Name.Length);
            utf8NameWithNull[utf8Name.Length] = 0; // Null terminator
            return SherpaOnnxSpeakerEmbeddingManagerRemove(Handle, utf8NameWithNull) == 1;
        }

        public string Search(float[] v, float threshold)
        {
            IntPtr p = SherpaOnnxSpeakerEmbeddingManagerSearch(Handle, v, threshold);

            string s = "";
            int length = 0;

            unsafe
            {
                byte* b = (byte*)p;
                if (b != null)
                {
                    while (*b != 0)
                    {
                        ++b;
                        length += 1;
                    }
                }
            }

            if (length > 0)
            {
                byte[] stringBuffer = new byte[length];
                Marshal.Copy(p, stringBuffer, 0, length);
                s = Encoding.UTF8.GetString(stringBuffer);
            }

            SherpaOnnxSpeakerEmbeddingManagerFreeSearch(p);

            return s;
        }

        public bool Verify(string name, float[] v, float threshold)
        {
            byte[] utf8Name = Encoding.UTF8.GetBytes(name);
            byte[] utf8NameWithNull = new byte[utf8Name.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Name, utf8NameWithNull, utf8Name.Length);
            utf8NameWithNull[utf8Name.Length] = 0; // Null terminator
            return SherpaOnnxSpeakerEmbeddingManagerVerify(Handle, utf8NameWithNull, v, threshold) == 1;
        }

        public bool Contains(string name)
        {
            byte[] utf8Name = Encoding.UTF8.GetBytes(name);
            byte[] utf8NameWithNull = new byte[utf8Name.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Name, utf8NameWithNull, utf8Name.Length);
            utf8NameWithNull[utf8Name.Length] = 0; // Null terminator
            return SherpaOnnxSpeakerEmbeddingManagerContains(Handle, utf8NameWithNull) == 1;
        }

        public string[] GetAllSpeakers()
        {
            if (NumSpeakers == 0)
            {
                return new string[] { };
            }

            IntPtr names = SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(Handle);

            string[] ans = new string[NumSpeakers];

            unsafe
            {
                byte** p = (byte**)names;
                for (int i = 0; i != NumSpeakers; i++)
                {
                    int length = 0;
                    byte* s = p[i];
                    while (*s != 0)
                    {
                        ++s;
                        length += 1;
                    }
                    byte[] stringBuffer = new byte[length];
                    Marshal.Copy((IntPtr)p[i], stringBuffer, 0, length);
                    ans[i] = Encoding.UTF8.GetString(stringBuffer);
                }
            }

            SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(names);

            return ans;
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~SpeakerEmbeddingManager()
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

        public int NumSpeakers
        {
            get
            {
                return SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(Handle);
            }
        }

        private IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }

        private NativeResourceHandle _handle;
        private int _dim;

        #region P/Invoke

        private static IntPtr SherpaOnnxCreateSpeakerEmbeddingManager(int dim)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxCreateSpeakerEmbeddingManager(dim),
                () => NativeExternal.SherpaOnnxCreateSpeakerEmbeddingManager(dim));
        }

        private static void SherpaOnnxDestroySpeakerEmbeddingManager(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroySpeakerEmbeddingManager(handle),
                () => NativeExternal.SherpaOnnxDestroySpeakerEmbeddingManager(handle));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerAdd(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerAdd(handle, utf8Name, v),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerAdd(handle, utf8Name, v));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, int n)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(handle, utf8Name, v, n),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(handle, utf8Name, v, n));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerRemove(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerRemove(handle, utf8Name),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerRemove(handle, utf8Name));
        }

        private static IntPtr SherpaOnnxSpeakerEmbeddingManagerSearch(IntPtr handle, float[] v, float threshold)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerSearch(handle, v, threshold),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerSearch(handle, v, threshold));
        }

        private static void SherpaOnnxSpeakerEmbeddingManagerFreeSearch(IntPtr p)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerFreeSearch(p),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerFreeSearch(p));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerVerify(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, float threshold)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerVerify(handle, utf8Name, v, threshold),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerVerify(handle, utf8Name, v, threshold));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerContains(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerContains(handle, utf8Name),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerContains(handle, utf8Name));
        }

        private static int SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(handle),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(handle));
        }

        private static IntPtr SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(handle),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(handle));
        }

        private static void SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(IntPtr names)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(names),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(names));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateSpeakerEmbeddingManager(int dim);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroySpeakerEmbeddingManager(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerAdd(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, int n);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerRemove(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingManagerSearch(IntPtr handle, float[] v, float threshold);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingManagerFreeSearch(IntPtr p);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerVerify(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, float threshold);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerContains(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(IntPtr names);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateSpeakerEmbeddingManager(int dim);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroySpeakerEmbeddingManager(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerAdd(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerAddListFlattened(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerRemove(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingManagerSearch(IntPtr handle, float[] v, float threshold);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingManagerFreeSearch(IntPtr p);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerVerify(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name, float[] v, float threshold);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerContains(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Name);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingManagerNumSpeakers(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingManagerGetAllSpeakers(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingManagerFreeAllSpeakers(IntPtr names);
        }

        #endregion
    }
}
