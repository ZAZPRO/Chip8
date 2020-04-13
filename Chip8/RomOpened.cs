using System;

namespace Chip8
{
    // Class to handle loading Rom and Gui button
    class RomOpened
    {
        public event EventHandler OnRomFileChanged;
        public Chip8 chip8;

        private string romfile;

        // Register event handler
        public RomOpened()
        {
            OnRomFileChanged += new EventHandler(l_OnRomFileChanged);
        }

        // Get Rom file
        public String RomFile
        {
            get => romfile;
            set
            {
                romfile = value;
                if (romfile != "openFileDialog1")
                    OnRomFileChanged(this, new EventArgs());
            }
        }

        // Create Chip8 on loading ROM
        void l_OnRomFileChanged(object sender, EventArgs e)
        {
            chip8 = new Chip8(RomFile);
        }
    }
}
