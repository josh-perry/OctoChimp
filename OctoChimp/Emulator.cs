using System;
using System.IO;

namespace OctoChimp
{
    public class Emulator
    {
        /*
            Memory map:

            0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
            0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
            0x200-0xFFF - Program ROM and work RAM
        */

        /// <summary>
        /// The opcode about to be run. 2 bytes.
        /// </summary>
        public int CurrentOpcode;

        /// <summary>
        /// 4k of memory.
        /// </summary>
        public byte[] Memory;

        /// <summary>
        /// 15 8-bit general registers (V0 through VE) + 1 carry flag.
        /// </summary>
        public byte[] VRegisters;

        /// <summary>
        /// 
        /// </summary>
        public int IndexRegister;

        /// <summary>
        /// 
        /// </summary>
        public short ProgramCounter;

        /// <summary>
        /// 64x32 bool array, screen pixels are true if white and false if black.
        /// </summary>
        public bool[,] Screen;

        /// <summary>
        /// 
        /// </summary>
        public byte[] Stack;

        /// <summary>
        /// 
        /// </summary>
        public byte StackPointer;

        /// <summary>
        /// 
        /// </summary>
        public bool[] Keys;

        /// <summary>
        /// Is emulator still running?
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Should we update the screen this frame?
        /// </summary>
        public bool DrawFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Emulator()
        {
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            Memory = new byte[4096];
            VRegisters = new byte[16];
            IndexRegister = new short();
            ProgramCounter = new short();
            Screen = new bool[64, 32];
            Stack = new byte[16];
            Keys = new bool[16];

            ProgramCounter = 512;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Running = true;

            while (Running)
            {
                EmulateCycle();

                if (DrawFlag)
                {
                    DrawGraphics();
                }

                SetKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetKeys()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void DrawGraphics()
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        private void EmulateCycle()
        {
            // Fetch
            CurrentOpcode = Memory[ProgramCounter] << 8 | Memory[ProgramCounter + 1];

            // Decode
            switch (CurrentOpcode & 0xF000)
            {               
                // ANNN: Sets I to the address NNN.
                case 0xA000:
                    IndexRegister = CurrentOpcode & 0x0FFF;
                    ProgramCounter += 2;
                    break;

                default:
                    Console.WriteLine($"Unknown opcode: 0x{CurrentOpcode}");
                    break;
            }

            // Execute

            // Update timers
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadGame(FileInfo file)
        {
            Console.WriteLine($"Loading '{file.Name}'");

            var bytes = File.ReadAllBytes(file.FullName);

            for (var index = 0; index < bytes.Length; index++)
            {
                Memory[index + 512] = bytes[index];
            }
        }
    }
}