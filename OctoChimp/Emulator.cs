using System;
using System.Diagnostics;
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
        public ushort CurrentOpcode;
        
        /// <summary>
        /// 4k of memory.
        /// </summary>
        public byte[] Memory;

        /// <summary>
        /// 15 8-bit general registers (V0 through VE) + 1 carry flag.
        /// </summary>
        public ushort[] VRegisters;

        /// <summary>
        /// 
        /// </summary>
        public ushort IndexRegister;

        /// <summary>
        /// 
        /// </summary>
        public ushort ProgramCounter;

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
        private static byte[] _fontSet =
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

        private Random rnd;

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
            VRegisters = new ushort[16];
            IndexRegister = new ushort();
            ProgramCounter = new ushort();
            Screen = new bool[64, 32];
            Stack = new byte[16];
            Keys = new bool[16];

            ProgramCounter = 512;

            rnd = new Random();

            LoadFont();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadFont()
        {
            Console.WriteLine($"Loading fontset");

            for (var index = 0; index < _fontSet.Length; index++)
            {
                Memory[index] = _fontSet[index];
            }
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
            CurrentOpcode = (ushort) (Memory[ProgramCounter] << 8 | Memory[ProgramCounter + 1]);

            var decodedOpcode = new DecodedOpcode(CurrentOpcode);

            // Decode + execute
            switch (CurrentOpcode & 0xF000)
            {
                case 0x1000:
                    _1NNN(decodedOpcode);
                    break;

                case 0x3000:
                    _3XNN(decodedOpcode);
                    break;

                case 0x6000:
                    _6XNN(decodedOpcode);
                    break;

                case 0x7000:
                    _7XNN(decodedOpcode);
                    break;

                case 0x8000:
                    _8XYN(decodedOpcode);
                    break;
                
                case 0xA000:
                    _ANNN(decodedOpcode);
                    break;
                
                case 0xC000:
                    _CXNN(decodedOpcode);
                    break;
                
                case 0xD000:
                    _DXYN(decodedOpcode);
                    break;
                
                default:
                    Debugger.Break();
                    break;
            }
            
            // Update timers
        }

        #region Opcodes
        /// <summary>
        /// Jumps to address NNN.   
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _1NNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;
            //ProgramCounter = decodedOpcode.NNN;
        }

        /// <summary>
        /// Skips the next instruction if VX equals NN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _3XNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            if (VRegisters[decodedOpcode.X] == decodedOpcode.NN)
            {
                ProgramCounter += 2;
            }
        }

        /// <summary>
        /// Sets VX to NN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _6XNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            VRegisters[decodedOpcode.X] = decodedOpcode.NN;
        }

        /// <summary>
        /// Adds NN to VX.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _7XNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            VRegisters[decodedOpcode.X] = (ushort)(VRegisters[decodedOpcode.X] + decodedOpcode.NN);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _8XYN(DecodedOpcode decodedOpcode)
        {
            switch (decodedOpcode.N)
            {
                // Sets VX to the value of VY.
                case 0x0:
                    VRegisters[decodedOpcode.X] = VRegisters[decodedOpcode.Y];
                    break;

                // Sets VX to VX or VY.
                case 0x1:
                    VRegisters[decodedOpcode.X] = (ushort) (VRegisters[decodedOpcode.X] | VRegisters[decodedOpcode.Y]);
                    break;

                // Sets VX to VX and VY.
                case 0x2:
                    VRegisters[decodedOpcode.X] = (ushort)(VRegisters[decodedOpcode.X] & VRegisters[decodedOpcode.Y]);
                    break;

                // Sets VX to VX xor VY.
                case 0x3:
                    VRegisters[decodedOpcode.X] = (ushort)(VRegisters[decodedOpcode.X] ^ VRegisters[decodedOpcode.Y]);
                    break;

                // Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                case 0x4:
                    // Not sure if this is correct.
                    // Source: https://github.com/Oicho/GO-Chip8/blob/master/chip8/opcodes.go#L151
                    if (VRegisters[decodedOpcode.Y] > 0xFF - decodedOpcode.X)
                    {
                        VRegisters[0xF] = 1;
                    }
                    else
                    {
                        VRegisters[0xF] = 0;
                    }

                    VRegisters[decodedOpcode.X] += VRegisters[decodedOpcode.Y];
                    break;

                // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                case 0x5:
                    if (decodedOpcode.X < decodedOpcode.Y)
                    {
                        VRegisters[0xF] = 1;
                    }
                    else
                    {
                        VRegisters[0xF] = 0;
                    }

                    VRegisters[decodedOpcode.X] -= VRegisters[decodedOpcode.Y];
                    break;

                // Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
                //case 0x6:
                //    break;

                // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                //case 0x7:
                //    break;

                // Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
                //case 0xE:
                //    break;

                default:
                    throw new Exception("Unknown opcode!");
            }

            ProgramCounter += 2;
        }

        /// <summary>
        /// Sets I to the address NNN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _ANNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;
            IndexRegister = decodedOpcode.NNN;
        }

        /// <summary>
        /// Sets VX to the result of a bitwise and operation on a random number and NN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _CXNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;
            VRegisters[decodedOpcode.X] = (ushort) (decodedOpcode.NN & rnd.Next());
        }

        /// <summary>
        /// Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
        /// Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change after the execution of this instruction.
        /// As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _DXYN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            // TODO: Finish DXYN.
        }
        #endregion

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