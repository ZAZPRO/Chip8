using System;
using System.Collections.Generic;

namespace Chip8
{
    static class Opcodes
    {
        // Initialize array and dicts of all Chip8 instructions
        public delegate void execOpcode(Chip8 chip8);

        public static execOpcode[] chip8Opcodes = { Zeros, Jmp_NNN, Jsr_NNN, Skeq_VXKK, Skne_VXKK, Skeq_VXVY, Mov_VXKK, Add_VXKK,
         Arithmetics, Skne_VXVY, Mvi_NNN, Jmi_NNN, Rand_VXKK, Sprite_VXVYN, Keyboard, Additional };

        public static execOpcode[] arithmetics = { Mov_VXVY, Or_VXVY, And_VXVY, Xor_VXVY, Add_VXVY, Sub_VXVY, Shr_VX, Rsb_VXVY, Chip8_NULL,
            Chip8_NULL, Chip8_NULL, Chip8_NULL, Chip8_NULL, Chip8_NULL, Shl_VX };
        public static Dictionary<int, execOpcode> additional = new Dictionary<int, execOpcode>() { { 0x07, Gdelay_VX }, { 0x15, Sdelay_VX },
            { 0x18, Ssound_VX }, { 0x1E, Adi_VX }, { 0x29, Font_VX}, { 0x33, Bcd_VX }, { 0x55, Str_V0VX }, { 0x65, Ldr_V0VX } };
        public static Dictionary<int, execOpcode> zeros = new Dictionary<int, execOpcode>() { { 0xE0, Cls }, { 0xEE, Rts } };
        public static Dictionary<int, execOpcode> keys = new Dictionary<int, execOpcode>() { { 0x9E, Skpr_X }, { 0xA1, Skup_X } };

        // 0    
        public static void Zeros(Chip8 chip8)
        {
            var kk = Utils.GetKK(chip8.Opcode);
            zeros[kk](chip8);
        }

        // 00E0
        public static void Cls(Chip8 chip8)
        {
            var boundX = chip8.Screen.GetUpperBound(0);
            var boundY = chip8.Screen.GetUpperBound(1);

            for (int x = 0; x < boundX; x++)
            {
                for (int y = 0; y < boundY; y++)
                {
                    chip8.Screen[x, y] = false;
                }
            }
        }

        // 00EE
        public static void Rts(Chip8 chip8)
        {
            chip8.PC = chip8.Stack[--chip8.SP];
        }

        // 1NNN
        public static void Jmp_NNN(Chip8 chip8)
        {
            var address = Utils.GetNNN(chip8.Opcode);
            chip8.PC = (ushort)address;
        }

        // 2NNN
        public static void Jsr_NNN(Chip8 chip8)
        {
            var nnn = Utils.GetNNN(chip8.Opcode);

            chip8.Stack[chip8.SP++] = chip8.PC;
            chip8.PC = (ushort)nnn;
        }

        // 3XKK
        public static void Skeq_VXKK(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var kk = Utils.GetKK(chip8.Opcode);
            if (chip8.V[x] == kk)
                chip8.PC += 2;
        }

        // 4XKK
        public static void Skne_VXKK(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var kk = Utils.GetKK(chip8.Opcode);
            if (chip8.V[x] != kk)
                chip8.PC += 2;
        }

        // 5XY0
        public static void Skeq_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            if (chip8.V[x] == chip8.V[y])
                chip8.PC += 2;
        }

        // 6XKK
        public static void Mov_VXKK(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var kk = Utils.GetKK(chip8.Opcode);
            chip8.V[x] = (byte)kk;
        }

        // 7XKK
        public static void Add_VXKK(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var kk = Utils.GetKK(chip8.Opcode);
            chip8.V[x] += (byte)kk;
        }

        // 8
        public static void Arithmetics(Chip8 chip8)
        {
            arithmetics[chip8.Opcode & 0x000F](chip8);
        }

        // 8XY0
        public static void Mov_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            chip8.V[x] = chip8.V[y];
        }

        // 8XY1
        public static void Or_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            chip8.V[x] = (byte)(chip8.V[x] | chip8.V[y]);
        }

        // 8XY2
        public static void And_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            chip8.V[x] = (byte)(chip8.V[x] & chip8.V[y]);
        }

        // 8XY3
        public static void Xor_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            chip8.V[x] = (byte)(chip8.V[x] ^ chip8.V[y]);
        }

        // 8XY4
        public static void Add_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);

            if ((chip8.V[x] + chip8.V[y]) > Byte.MaxValue)
                chip8.V[0xF] = 1;
            else
                chip8.V[0xF] = 0;
            chip8.V[x] += chip8.V[y];
        }

        // 8XY5
        public static void Sub_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);

            if (chip8.V[x] > chip8.V[y])
                chip8.V[0xF] = 1;
            else
                chip8.V[0xF] = 0;
            chip8.V[x] -= chip8.V[y];
        }

        // 8X06
        public static void Shr_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var b = (byte) (chip8.V[x] & 0b00000001);

            chip8.V[0xF] = b;
            chip8.V[x] >>= 1;
        }

        // 8XY7
        public static void Rsb_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);

            if (chip8.V[y] > chip8.V[x])
                chip8.V[0xF] = 1;
            else
                chip8.V[0xF] = 0;
            chip8.V[x] = (byte) (chip8.V[y] - chip8.V[x]);
        }

        // 8X0E
        public static void Shl_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var b = (byte)(chip8.V[x] & 0b10000000);

            chip8.V[0xF] = b;
            chip8.V[x] <<= 1;
        }

        // 9XY0
        public static void Skne_VXVY(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var y = Utils.GetY(chip8.Opcode);
            if (chip8.V[x] != chip8.V[y])
                chip8.PC += 2;
        }

        // ANNN
        public static void Mvi_NNN(Chip8 chip8)
        {
            var nnn = Utils.GetNNN(chip8.Opcode);
            chip8.I = (ushort)nnn;
        }

        // BNNN
        public static void Jmi_NNN(Chip8 chip8)
        {
            var nnn = Utils.GetNNN(chip8.Opcode);
            chip8.PC = (ushort)(chip8.V[0] + nnn);
        }

        // CXKK
        public static void Rand_VXKK(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            var kk = Utils.GetKK(chip8.Opcode);
            var random = new Random();
            var number = random.Next(256);

            chip8.V[x] = (byte)(number & kk);
        }

        // DXYN
        public static void Sprite_VXVYN(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            x = chip8.V[x];
            var y = Utils.GetY(chip8.Opcode);
            y = chip8.V[y];
            y = y % Chip8.ScreenHeight;
            var n = Utils.GetN(chip8.Opcode);
            var actualX = 0;
            var actualY = 0;

            chip8.V[0xF] = 0;
            ushort p;

            for (int yy = 0; yy < n; yy++)
            {
                p = chip8.RAM[chip8.I + yy];
                for (int xx = 0; xx < 8; xx++)
                {
                    if ((p & (0x80 >> xx)) != 0)
                    {
                        actualX = (x + xx) % Chip8.ScreenWidth;
                        actualY = (y + yy) % Chip8.ScreenHeight;
                        if (chip8.Screen[actualX, actualY])
                            chip8.V[0xF] = 1;
                        chip8.Screen[actualX, actualY] ^= true;
                    }
                }
            }

            chip8.drawFlag = true;
        }

        // E
        public static void Keyboard(Chip8 chip8)
        {
            var kk = Utils.GetKK(chip8.Opcode);
            keys[kk](chip8);
        }

        // EX9E
        public static void Skpr_X(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);

            if (chip8.keyboard.Contains(chip8.V[x]))
                chip8.PC += 2;
        }

        // EX9E
        public static void Skup_X(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);

            if (!chip8.keyboard.Contains(chip8.V[x]))
                chip8.PC += 2;
        }

        // F
        public static void Additional(Chip8 chip8)
        {
            var kk = Utils.GetKK(chip8.Opcode);
            additional[kk](chip8);
        }

        // FX07
        public static void Gdelay_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            chip8.V[x] = chip8.delayTimer;
        }

        // FX15
        public static void Sdelay_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            chip8.delayTimer = chip8.V[x];
        }

        // FX18
        public static void Ssound_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            chip8.soundTimer = chip8.V[x];
        }

        // FX1E
        public static void Adi_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            chip8.I += chip8.V[x];
        }

        // FX29
        public static void Font_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            chip8.I = (ushort)(chip8.V[x] * 5);
        }

        // FX33
        public static void Bcd_VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);

            chip8.RAM[chip8.I] = (byte)(chip8.V[x] / 100);
            chip8.RAM[chip8.I + 1] = (byte)((chip8.V[x] / 10) % 10);
            chip8.RAM[chip8.I + 2] = (byte)((chip8.V[x] % 100) % 10);
        }

        // FX55
        public static void Str_V0VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            for (int i = 0; i <= x; i++)
                chip8.RAM[chip8.I + i] = chip8.V[i];
        }

        // FX65
        public static void Ldr_V0VX(Chip8 chip8)
        {
            var x = Utils.GetX(chip8.Opcode);
            for (int i = 0; i <= x; i++)
                chip8.V[i] = chip8.RAM[chip8.I + i];
        }

        // Simple NULL pattern
        public static void Chip8_NULL(Chip8 chip8)
        {
            Console.WriteLine("Not implemented");
        }
    }
}
