using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lomont.Win
{
    /// <summary>
    /// Class for running a function in a DLL or EXE
    /// </summary>
    public static class RunDll
    {

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);


        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Call a C/C++ char * func(void) style function
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static (bool success, T result) GetTFromFuncCall<T,TDelegate>(string filename, string functionName, Func<Delegate,T> mapper)
        {
            bool success;
            T result = default;
            try
            {
                var hModule = LoadLibrary(filename);
                if (hModule == IntPtr.Zero)
                {
                    Trace.TraceError($"Cannot load library {filename}");
                    return (false, result);
                }

                var addr = GetProcAddress(hModule, functionName);
                if (addr == IntPtr.Zero)
                {
                    Trace.TraceError($"Cannot load function {functionName} in library {filename}");
                    return (false, result);
                }
                var d = Marshal.GetDelegateForFunctionPointer(addr, typeof(TDelegate));
                result = mapper(d);

                FreeLibrary(hModule); // todo - RAII this
                success = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Exception: {ex}");
                success = false;
                result = default;
            }

            return (success, result);
        }

        /// <summary>
        /// Call a C/C++ char * func(void) style function
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static (bool success, string result) GetStringFromFuncCall(string filename, string functionName)
        {
            return GetTFromFuncCall<string, StrVoid>(
                filename, functionName,
                dele => Marshal.PtrToStringAnsi(((StrVoid) dele)()));
        }
        delegate IntPtr StrVoid();


        /// <summary>
        /// Call a C/C++ int func(void) style function
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static (bool success, int result) GetIntFromFuncCall(string filename, string functionName)
        {
            return GetTFromFuncCall<int, IntVoid>(
                filename, functionName,
                dele => ((IntVoid) dele)()
                );
        }
        delegate int IntVoid();

        /// <summary>
        /// Call a C/C++ void func(int*,int*,int*) style function
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static (bool success, int a,int b, int c) GetInt3FromFuncCall(string filename, string functionName)
        {
            var res1 = GetTFromFuncCall<(int,int,int), VoidInt3>(
                filename, functionName,
                dele =>
                {
                    ((VoidInt3) dele)(out var a, out var b, out var c);
                    return (a,b,c);
                });
            return (res1.success, res1.result.Item1, res1.result.Item2, res1.result.Item3);
        }
        delegate void VoidInt3(out int a, out int b, out int c);


    }
}
