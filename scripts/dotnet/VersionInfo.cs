/// Copyright (c)  2025  Xiaomi Corporation (authors: Fangjun Kuang)
using System;
using System.Runtime.InteropServices;
using System.Text;


namespace SherpaOnnx
{
    public class VersionInfo
    {
        public static String Version
        {
          get
          {
            IntPtr p = SherpaOnnxGetVersionStr();

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

            return s;
          }
        }

        public static String GitSha1
        {
          get
          {
            IntPtr p = SherpaOnnxGetGitSha1();

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

            return s;
          }
        }

        public static String GitDate
        {
          get
          {
            IntPtr p = SherpaOnnxGetGitDate();

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

            return s;
          }
        }


        #region P/Invoke

        private static IntPtr SherpaOnnxGetVersionStr()
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxGetVersionStr(),
                () => NativeExternal.SherpaOnnxGetVersionStr());
        }

        private static IntPtr SherpaOnnxGetGitSha1()
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxGetGitSha1(),
                () => NativeExternal.SherpaOnnxGetGitSha1());
        }

        private static IntPtr SherpaOnnxGetGitDate()
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxGetGitDate(),
                () => NativeExternal.SherpaOnnxGetGitDate());
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxGetVersionStr();

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxGetGitSha1();

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxGetGitDate();
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxGetVersionStr();

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxGetGitSha1();

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxGetGitDate();
        }

        #endregion
    }
}
