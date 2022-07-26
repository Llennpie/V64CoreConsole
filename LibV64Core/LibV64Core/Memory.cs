using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibV64Core
{
    public class Memory
    {
        #region DllImports
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, long nSize, ref long lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, long nSize, ref long lpNumberOfBytesWritten);
        #endregion

        public static Process? emulatorProcess;
        private static IntPtr emulatorProcessHandle;
        const int PROCESS_ALL_ACCESS = 0x01F0FF;

        public static long BaseAddress;

        public static bool IsEmulatorOpen
        {
            get
            {
                try
                {
                    return (emulatorProcess != null && !emulatorProcess.HasExited);
                }
                catch
                {
                    return false;
                }
            }
        }

        #region Processes
        /// <summary>
        /// Returns an array of emulator processes by name. Defaults to "Project64".
        /// </summary>
        /// <param name="processName"></param>
        public static Process[] GetEmulatorProcesses(string processName = "Project64")
        {
            // To account for multiple emulator processes, we store them in an array.
            Process[] emulators = Process.GetProcessesByName(processName);

            if (emulators == null || emulators.Length == 0)
            {
                throw new InvalidOperationException("ERROR: Could not find active Project64 process");
            }
            else
            {
                return emulators;
            }
        }

        /// <summary>
        /// Hooks a single emulator process.
        /// </summary>
        /// <param name="process"></param>
        public static void HookEmulatorProcess(Process? process)
        {
            if (process != null)
            {
                emulatorProcess = process;
                emulatorProcessHandle = process.Handle;
            }
        }

        /// <summary>
        /// Clears the hooked emulator process.
        /// </summary>
        public static void ClearEmulatorProcess()
        {
            emulatorProcess = null;
            emulatorProcessHandle = IntPtr.Zero;
        }
        #endregion

        #region Reading/Writing
        /// <summary>
        /// Returns a byte array at a given address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(long address, long size)
        {
            IntPtr ptr = new IntPtr(address);
            byte[] buffer = new byte[size];
            long bytesRead = 0;

            ReadProcessMemory(emulatorProcessHandle, ptr, buffer, size, ref bytesRead);
            return buffer;
        }
        /// <summary>
        /// Writes a byte array at a given address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="swap"></param>
        public static void WriteBytes(long address, byte[] data, bool swap = false)
        {
            IntPtr ptr = new IntPtr(address);
            long size = data.LongLength;
            long bytesWritten = 0;

            // In some write cases, the endianness may need to be swapped.
            if (swap)
            {
                data = SwapEndian(data, 4);
            }

            WriteProcessMemory(emulatorProcessHandle, ptr, data, size, ref bytesWritten);
        }

        /// <summary>
        /// Swaps endianness of the given byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="wordSize"></param>
        /// <returns></returns>
        public static byte[] SwapEndian(byte[] array, int wordSize)
        {
            byte[] byteArray = new byte[array.Length];
            array.CopyTo(byteArray, 0);
            for (int x = 0; x < byteArray.Length / wordSize; x++)
            {
                Array.Reverse(byteArray, x * wordSize, wordSize);
            }

            return byteArray;
        }
        #endregion

        /// <summary>
        /// Finds the in-game base address and sets it globally (BaseAddress).
        /// </summary>
        /// <returns></returns>
        public static void FindBaseAddress()
        {
            long start = 0x20000000;
            long stop = 0xF0000000;
            long step = 0x10000;

            uint value;

            // Checks if our current BaseAddress is still valid.
            if (BaseAddress > 0)
            {
                value = BitConverter.ToUInt32(ReadBytes(BaseAddress, sizeof(uint)), 0);
                if (value == 0x3C1A8032)
                    return;
            }

            // We begin searching for the BaseAddress. This may cause CPU issues on lower-end devices.
            for (long scanAddress = start; scanAddress < stop - step; scanAddress += step)
            {
                value = BitConverter.ToUInt32(ReadBytes(scanAddress, sizeof(uint)), 0);
                if (value == 0x3C1A8032)
                {
                    BaseAddress = scanAddress;
                    return;
                }
            }

            // If nothing is found, set the BaseAddress to 0.
            BaseAddress = 0;
        }
    }
}
