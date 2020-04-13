using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chip8
{
    // GUI
    public partial class Form1 : Form
    {
        // Scale of graphics
        int mul = 10;

        // Bitmap and Rect for drawing graphics
        Bitmap bitmap;
        Rectangle rect;

        // Chip8 instance
        Chip8 chip8;

        // Timers
        readonly Stopwatch stopWatch = Stopwatch.StartNew();
        readonly TimeSpan span60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan span1000Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);

        // Keyboard layout
        Dictionary<Keys, byte> keyMap = new Dictionary<Keys, byte>
        {
            { Keys.D1, 0x1 },
            { Keys.D2, 0x2 },
            { Keys.D3, 0x3 },
            { Keys.D4, 0xC },
            { Keys.Q, 0x4 },
            { Keys.W, 0x5 },
            { Keys.E, 0x6 },
            { Keys.R, 0xD },
            { Keys.A, 0x7 },
            { Keys.S, 0x8 },
            { Keys.D, 0x9 },
            { Keys.F, 0xE },
            { Keys.Z, 0xA },
            { Keys.X, 0x0 },
            { Keys.C, 0xB },
            { Keys.V, 0xF },
        };

        // Constructor
        public Form1()
        {
            InitializeComponent();
        }

        // Render Image
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        // Handle Rom opening
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
            var romOpened = new RomOpened
            {
                RomFile = openFileDialog1.FileName
            };
            this.chip8 = romOpened.chip8;
        }

        // On program start initialize everything
        private void Form1_Load(object sender, EventArgs e)
        {
            bitmap = new Bitmap(Chip8.ScreenWidth * mul, Chip8.ScreenHeight * mul);
            rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            // Simulate 2 hardware chips. Timer and CPU
            Task.Run(() => Loop(span60Hz, (Action)Tick60Hz));
            Task.Run(() => Loop(span1000Hz, (Action)Tick1000Hz));
        }

        // Call required method with required frequency
        void Loop(TimeSpan frequency, Action fToTrigger)
        {
            TimeSpan lastTime = stopWatch.Elapsed;
            while (true)
            {
                var currentTime = stopWatch.Elapsed;
                var elapsedTime = currentTime - lastTime;
                while (elapsedTime >= frequency)
                {
                    this.Invoke(fToTrigger);
                    elapsedTime -= frequency;
                    lastTime += frequency;
                }
                Thread.Sleep(elapsedTime);
            }
        }

        // CPU Tick
        void Tick1000Hz()
        {
            chip8?.Loop();
        }

        // Timer Tick
        void Tick60Hz()
        {
            if (chip8 == null) return;

            chip8.Loop60Hz();
            if (chip8.drawFlag)
                Render();
        }

        // Render image. Unsafe for pointers that bring performance.
        unsafe void Render()
        {
            // Bitmap layout is 1 bit per pixel indexed
            // That requires some alignment of data
            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            // Get beginning of data layout
            byte* ptr = (byte*) bmpData.Scan0.ToPointer();
            
            // For each image pixel
            for (int y = 0; y < bmpData.Height; y++)
            {
                for (int x = 0; x < bmpData.Width; x++)
                {
                    int index = y * bmpData.Stride + (x >> 3); // Get index
                    byte mask = (byte) (0x80 >> (x & 0x7)); // Mask to set
                    if (this.chip8.Screen[x / mul, y / mul]) // Handle image scaling
                        ptr[index] |= mask;
                    else
                        ptr[index] &= (byte) (mask ^ 0xff);
                }
            }

            // Finish writing to bitmap
            bitmap.UnlockBits(bmpData);
            // Refresh Form
            this.Refresh();
            // Reset Chip8 draw flag
            this.chip8.drawFlag = false;
        }

        // Handle keyboard input
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (chip8 == null) return;
            if (keyMap.ContainsKey(e.KeyCode))
                chip8.keyboard.Remove(keyMap[e.KeyCode]);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (chip8 == null) return;
            if (keyMap.ContainsKey(e.KeyCode))
                chip8.keyboard.Add(keyMap[e.KeyCode]);
        }
    }
}
