using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Chip8
{
    static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNNN(ushort opcode)
        {
            return opcode & 0x0FFF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKK(ushort opcode)
        {
            return opcode & 0x00FF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetX(ushort opcode)
        {
            return (opcode & 0x0F00) >> 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetY(ushort opcode)
        {
            return (opcode & 0x00F0) >> 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetN(ushort opcode)
        {
            return opcode & 0x000F;
        }

        public static void WriteHex(ushort value)
        {
            Console.WriteLine(value.ToString("X"));
        }

        public static string GetHex(ushort value)
        {
            return value.ToString("X");
        }
    }
}
