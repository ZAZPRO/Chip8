using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Chip8
{
    class Chip8
    {
        // Screen size
        public const int ScreenWidth = 64;
        public const int ScreenHeight = 32;

        // Registers declaration
        public byte[] RAM = new byte[4096];
        public byte[] V = new byte[16];
        public ushort I;
        public byte delayTimer;
        public byte soundTimer;
        public ushort PC { get; set; }
        public byte SP { get; set; }
        public ushort[] Stack = new ushort[16];

        // 1 byte graphics 2 dimensional screen array
        public bool[,] Screen = new bool[ScreenWidth, ScreenHeight];
        // Screen need update flag
        public bool drawFlag = false;
        // Is playing sound
        private bool beeping = false;

        // Current Opcode
        public ushort Opcode { get; set; }

        // Default font set that should be loaded in the memory
        private readonly byte[] fontset =
        {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        // Hash set of states of keyboard keys
        public HashSet<byte> keyboard = new HashSet<byte>();

        // Constructor
        public Chip8(string romPath)
        {
            // Loading font set
            Array.Copy(fontset, 0, RAM, 0, fontset.Length);
            // Moving PC to the begging of data space
            PC = 0x200;
            // Reading ROM bytes
            var rom = File.ReadAllBytes(romPath);
            // Loading ROM bytes into the data space
            Array.Copy(rom, 0, RAM, PC, rom.Length);
            // Clearing screen
            for (int x = 0; x < ScreenWidth; x++)
            {
                for (int y = 0; y < ScreenHeight; y++)
                {
                    Screen[x, y] = false;
                }
            }
        }

        // Main loop
        public void Loop()
        {
            // Get current instruction
            Fetch();
            // Execute it
            Opcodes.chip8Opcodes[(Opcode & 0xF000) >> 12](this);
        }

        // Timer loop
        public void Loop60Hz()
        {
            // Decrement timers
            if (this.delayTimer > 0)
                this.delayTimer--;

            if (this.soundTimer > 0)
                this.soundTimer--;

            // Reset var so we can play sound again
            if (soundTimer == 0)
                beeping = false;

            // Play sound if we should
            if (soundTimer > 0 && !beeping)
            {
                new Thread(() => Console.Beep(500, soundTimer)).Start();
                beeping = true;
            }
        }

        // Prepare instruction
        void Fetch()
        {
            // Each instruction is 2 bytes long so we need to merge them
            Opcode = (ushort)((RAM[PC] << 8) + RAM[PC + 1]);
            // Increment program counter by those 2 bytes read
            PC += 2;
        }
    }
}
