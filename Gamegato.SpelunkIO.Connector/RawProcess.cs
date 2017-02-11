using System;
using System.Diagnostics;

namespace Gamegato.SpelunkIO.Connector
{
    public class RawProcess : IDisposable
    {
        public string ProcessName { get; private set; }
        private IntPtr ProcessHandle;
        private Process Process;
        public int BaseAddress { get; private set; }
        public bool HasExited => Process.HasExited;
        public string FilePath => Process.MainModule.FileName;

        public RawProcess(string processName)
        {
            ProcessName = processName;

            var processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception("Failed to find process by name: " + processName);

            var process = processes[0];
            Process = process;
            BaseAddress = process.MainModule.BaseAddress.ToInt32();
            ProcessHandle = Kernel32.OpenProcess(Kernel32.ProcessPermissionsEnum.PROCESS_ALL_ACCESS, false, process.Id);
        }

        // returns an offset containing the signature match (if it exists)
        int? FindSignatureMatch(byte[] buf, int bufUsed, byte?[] signature)
        {
            var end = bufUsed - signature.Length + 1;
            for (var offs = 0; offs < end; ++offs)
            {
                var matches = 0;
                for (var i = 0; i < signature.Length; ++i)
                {
                    byte? s = signature[i];
                    if (s.HasValue && s.Value != buf[i + offs]) { goto NoMatch; }
                    ++matches;
                }
                return offs;
                NoMatch: { };
            }
            return null;
        }

        const int BUF_SCAN_SIZE = 4096;
        public int? FindBytes(byte?[] signature, int startAddr = 0, int endAddr = 0x3000000)
        {
            int addr = startAddr;
            var mbi = new Kernel32.MEMORY_BASIC_INFORMATION();
            byte[] buf = new byte[4096];
            var pagesConsidered = 0;

            while (Kernel32.VirtualQueryEx((int)ProcessHandle, (IntPtr)addr, ref mbi, Kernel32.MBI_LENGTH) > 0)
            {
                int end = (int)mbi.BaseAddress + (int)mbi.RegionSize;
                if (mbi.State != Kernel32.StateEnum.MEM_COMMIT) { goto NextPage; }
                if ((int)mbi.BaseAddress > addr) { addr = (int)mbi.BaseAddress; }
                if (addr >= endAddr) { break; }

                while (addr < end)
                {
                    var bufUsed = Math.Min(buf.Length, end - addr);
                    int szRead = ReadBytes(addr, ref buf);
                    if (szRead == 0) { break; }
                    var maybeOffs = FindSignatureMatch(buf, bufUsed, signature);
                    if (maybeOffs.HasValue) { return addr + maybeOffs.Value; }
                    addr += bufUsed;
                }

                NextPage: { }
                addr = end;
                ++pagesConsidered;
            }

            return null;
        }

        public int WriteInt32(int address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public int WriteBool(int address, bool value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public int WriteDouble(int address, double value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public int WriteSingle(int address, float value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public void SetMemoryWritable(int address, int numBytes)
        {
            var prevPermissions = new Kernel32.PagePermissionsEnum();
            if (!Kernel32.VirtualProtectEx((int)ProcessHandle, (IntPtr)address, numBytes, Kernel32.PagePermissionsEnum.PAGE_EXECUTE_READWRITE, ref prevPermissions))
                throw new Exception("Failed to set memory writeable: " + Kernel32.GetLastError());
        }

        public int WriteBytes(int address, byte[] bytes)
        {
            AssertValid();
            int bytesWritten = 0;
            Kernel32.WriteProcessMemory((int)ProcessHandle, address, bytes, bytes.Length, ref bytesWritten);
            return bytesWritten;
        }

        public int ReadInt32(int address)
        {
            return BitConverter.ToInt32(ReadBytes(address, sizeof(int)), 0);
        }

        public bool ReadBool(int address)
        {
            return BitConverter.ToBoolean(ReadBytes(address, sizeof(bool)), 0);
        }

        public float ReadSingle(int address)
        {
            return BitConverter.ToSingle(ReadBytes(address, sizeof(float)), 0);
        }

        public double ReadDouble(int address)
        {
            return BitConverter.ToDouble(ReadBytes(address, sizeof(double)), 0);
        }

        public byte[] ReadBytes(int address, int count)
        {
            byte[] bytes = new byte[count];
            ReadBytes(address, ref bytes);
            return bytes;
        }

        public int ReadBytes(int address, ref byte[] bytes)
        {
            AssertValid();
            int bytesRead = 0;
            Kernel32.ReadProcessMemory((int)ProcessHandle, address, bytes, bytes.Length, ref bytesRead);
            return bytesRead;
        }

        public void AssertValid()
        {
            if (ProcessHandle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(RawProcess));
            else if (Process.HasExited)
                throw new Exception("Target process has exited");
        }

        public void Dispose()
        {
            if (ProcessHandle == IntPtr.Zero)
                return;
            Kernel32.CloseHandle((int)ProcessHandle);
            ProcessHandle = IntPtr.Zero;
        }
    }
}
