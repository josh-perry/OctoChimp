﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;


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
        /// Program counter as of last cycle.
        /// </summary>
        public ushort PrevProgramCounter;

        /// <summary>
        /// 64x32 bool array, screen pixels are true if white and false if black.
        /// </summary>
        public bool[,] Screen;

        /// <summary>
        /// 
        /// </summary>
        public ushort[] Stack;

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
        public bool DrawFlag => true;

        public int InstructionsExecuted = 0;

        private Thread TimerThread;

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

        public byte DelayTimer;

        public ushort SoundTimer;

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
            Stack = new ushort[16];
            Keys = new bool[16];

            ProgramCounter = 512;

            TimerThread = new Thread(UpdateTimers);
            TimerThread.Start();

            rnd = new Random();

            LoadFont();
            SetKeys();

            InstructionsExecuted = 0;
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
            }
        }

        private void UpdateTimers()
        {
            while (true)
            {
                if (!Running)
                {
                    Thread.Sleep(1000 / 60);
                    continue;
                }

                // Update timers
                if (DelayTimer > 0)
                {
                    DelayTimer--;
                }

                if (SoundTimer > 0)
                {
                    if (SoundTimer == 1)
                    {
                        Console.Beep();
                    }

                    SoundTimer--;
                }

                Thread.Sleep(1000/60);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetKeys()
        {
            for (var index = 0; index < Keys.Length; index++)
            {
                Keys[index] = false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void EmulateCycle()
        {
            PrevProgramCounter = ProgramCounter;

            // Check boundaries
            if (ProgramCounter < 512 || ProgramCounter > 4096)
            {
                Console.WriteLine("ProgramCounter is out of bounds!");
                Debugger.Break();
            }

            // Fetch
            CurrentOpcode = (ushort) (Memory[ProgramCounter] << 8 | Memory[ProgramCounter + 1]);
            
            var op = new DecodedOpcode(CurrentOpcode);
            
            // Decode + execute
            switch (CurrentOpcode & 0xF000)
            {
                case 0x0000:
                    _miscOpcodes(op);
                    break;

                case 0x1000:
                    //op.Description = $"Jump to 0x{op.NNN.ToString("X")}";
                    _1NNN(op);
                    break;

                case 0x2000:
                    //op.Description = $"Call subroutine at 0x{op.NNN.ToString("X")}";
                    _2NNN(op);
                    break;

                case 0x3000:
                    //op.Description = $"Skip if V{op.X} == {op.NN.ToString("X")}";
                    _3XNN(op);
                    break;

                case 0x4000:
                    //op.Description = $"Skip if V{op.X} != {op.NN.ToString("X")}";
                    _4XNN(op);
                    break;

                case 0x5000:
                    //op.Description = $"Skip if V{op.X} == V{op.Y}";
                    _5XY0(op);
                    break;

                case 0x6000:
                    //op.Description = $"V{op.X} = {op.NN.ToString("X")}";
                    _6XNN(op);
                    break;

                case 0x7000:
                    //op.Description = $"V{op.X} += {op.NN.ToString("X")}";
                    _7XNN(op);
                    break;

                case 0x8000:
                    _8XYN(op);
                    break;

                case 0x9000:
                    _9XY0(op);
                    break;
                
                case 0xA000:
                    //op.Description = $"I = {op.NNN.ToString("X")}";
                    _ANNN(op);
                    break;

                case 0xB000:
                    _BNNN(op);
                    break;

                case 0xC000:
                    //op.Description = $"V{op.X} = RNG & {op.NN.ToString("X")}";
                    _CXNN(op);
                    break;
                
                case 0xD000:
                    //op.Description = $"Sprite at V{op.X}, V{op.Y} ({VRegisters[op.X]}, {VRegisters[op.Y]})";
                    _DXYN(op);
                    break;

                case 0xE000:
                    _EXNN(op);
                    break;

                case 0xF000:
                    _FNNN(op);
                    break;
                
                default:
                    //ProgramCounter += 2;
                    Debugger.Break();
                    break;
            }

            Console.WriteLine($"0x{PrevProgramCounter.ToString("X")}\t{op}");
            InstructionsExecuted++;
        }

        #region Opcodes
        /// <summary>
        /// 
        /// </summary>
        private void _FNNN(DecodedOpcode decodedOpcode)
        {
            switch (decodedOpcode.N)
            {
                case 0x7:
                    VRegisters[decodedOpcode.X] = DelayTimer;
                    break;
                case 0xA:
                    if (!Keys.Any(x => x))
                    {
                        return;
                    }
                    
                    break;
                case 0x8:
                    // Sets the sound timer to VX.
                    if (decodedOpcode.NN == 0x18)
                    {
                        SoundTimer = VRegisters[decodedOpcode.X];
                    }

                    break;
                case 0xE:
                    IndexRegister += VRegisters[decodedOpcode.X];
                    break;
                case 0x9:
                    IndexRegister = (ushort) (VRegisters[decodedOpcode.X] * 5);
                    break;
                case 0x3:
                    Memory[IndexRegister]     = (byte) (VRegisters[decodedOpcode.Y] / 100);
                    Memory[IndexRegister + 1] = (byte) ((VRegisters[decodedOpcode.Y] / 10) % 10);
                    Memory[IndexRegister + 2] = (byte) ((VRegisters[decodedOpcode.Y] % 100) % 10);

                    break;
                case 0x5:
                    // Stores V0 to VX (including VX) in memory starting at address I.
                    if (decodedOpcode.NN == 0x55)
                    {
                        for (var i = 0; i <= decodedOpcode.X; i++)
                        {
                            Memory[IndexRegister + i] = (byte) VRegisters[i];
                        }

                        break;
                    }

                    // Fills V0 to VX (including VX) with values from memory starting at address I.
                    if (decodedOpcode.NN == 0x65)
                    {
                        for (var i = 0; i <= decodedOpcode.X; i++)
                        {
                            VRegisters[i] = Memory[IndexRegister + i];
                        }

                        break;
                    }

                    // Sets the delay timer to VX.
                    if (decodedOpcode.NN == 0x15)
                    {
                        DelayTimer = (byte) VRegisters[decodedOpcode.X];

                        break;
                    }

                    Debugger.Break();
                    break;
                default:
                    Debugger.Break();
                    break;
            }

            ProgramCounter += 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _miscOpcodes(DecodedOpcode decodedOpcode)
        {
            // Return from subroutine.
            if (decodedOpcode.Opcode == 0x00EE)
            {
                StackPointer--;
                ProgramCounter = Stack[StackPointer];
                ProgramCounter += 2;

                return;
            }

            // Clears the screen.
            if (decodedOpcode.Opcode == 0x00E0)
            {
                for (var x = 0; x < Screen.GetLength(0); x++)
                {
                    for (var y = 0; y < Screen.GetLength(1); y++)
                    {
                        Screen[x, y] = false;
                    }
                }

                ProgramCounter += 2;
                return;
            }
        }

        /// <summary>
        /// Jumps to address NNN.   
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _1NNN(DecodedOpcode decodedOpcode)
        {
            if (ProgramCounter == decodedOpcode.NNN)
            {
               //Debugger.Break();
            }

            ProgramCounter = decodedOpcode.NNN;
        }

        /// <summary>
        /// Calls subroutine at NNN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _2NNN(DecodedOpcode decodedOpcode)
        {
            // Add to stack
            Stack[StackPointer] = ProgramCounter;
            StackPointer++;

            // Jump to address
            ProgramCounter = decodedOpcode.NNN;
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
        /// Skips the next instruction if VX doesn't equal NN.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _4XNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            if (VRegisters[decodedOpcode.X] != decodedOpcode.NN)
            {
                ProgramCounter += 2;
            }
        }

        /// <summary>
        /// Skips the next instruction if VX equals VY.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _5XY0(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            if (VRegisters[decodedOpcode.X] == VRegisters[decodedOpcode.Y])
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

            var n = (ushort)(VRegisters[decodedOpcode.X] + decodedOpcode.NN);

            VRegisters[decodedOpcode.X] = n;
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
                    decodedOpcode.Description = $"V{decodedOpcode.X} = V{decodedOpcode.Y}";
                    VRegisters[decodedOpcode.X] = VRegisters[decodedOpcode.Y];
                    break;

                // Sets VX to VX or VY.
                case 0x1:
                    decodedOpcode.Description = $"V{decodedOpcode.X} = V{ decodedOpcode.X} | V{ decodedOpcode.Y}";
                    VRegisters[decodedOpcode.X] = (ushort) (VRegisters[decodedOpcode.X] | VRegisters[decodedOpcode.Y]);
                    break;

                // Sets VX to VX and VY.
                case 0x2:
                    decodedOpcode.Description = $"V{decodedOpcode.X} = V{ decodedOpcode.X} & V{ decodedOpcode.Y}";
                    VRegisters[decodedOpcode.X] = (ushort)(VRegisters[decodedOpcode.X] & VRegisters[decodedOpcode.Y]);
                    break;

                // Sets VX to VX xor VY.
                case 0x3:
                    decodedOpcode.Description = $"V{decodedOpcode.X} = V{ decodedOpcode.X} ^ V{ decodedOpcode.Y}";
                    VRegisters[decodedOpcode.X] = (ushort)(VRegisters[decodedOpcode.X] ^ VRegisters[decodedOpcode.Y]);
                    break;

                // Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                case 0x4:
                    VRegisters[0xF] = (ushort)(VRegisters[decodedOpcode.Y] >= VRegisters[decodedOpcode.X] ? 0 : 1);
                    VRegisters[decodedOpcode.X] += VRegisters[decodedOpcode.Y];
                    break;

                // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                case 0x5:
                    VRegisters[0xF] = (ushort)(VRegisters[decodedOpcode.Y] >= VRegisters[decodedOpcode.X] ? 0 : 1);
                    VRegisters[decodedOpcode.X] -= VRegisters[decodedOpcode.Y];
                    break;
                
                // Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
                case 0x6:
                    VRegisters[0xF] = (ushort) (VRegisters[decodedOpcode.Y] & 0x01);
                    VRegisters[decodedOpcode.X] = (ushort) (VRegisters[decodedOpcode.X] >> 1);
                    break;

                // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                case 0x7:
                    // Set 0xF to 0 if there's a borrow
                    VRegisters[0xF] = (ushort) (VRegisters[decodedOpcode.Y] >= VRegisters[decodedOpcode.X] ? 1 : 0);
                    VRegisters[decodedOpcode.X] = (ushort) (VRegisters[decodedOpcode.Y] - VRegisters[decodedOpcode.X]);
                    break;

                // Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
                case 0xE:
                    VRegisters[0xF] = (ushort) ((VRegisters[decodedOpcode.Y] >> 7) & 0x01);
                    VRegisters[decodedOpcode.X] = (ushort) (VRegisters[decodedOpcode.Y] << 1);
                    break;

                default:
                    throw new Exception($"Unknown opcode: {decodedOpcode}");
            }

            ProgramCounter += 2;
        }

        /// <summary>
        /// Skips the next instruction if VX doesn't equal VY.
        /// </summary>
        private void _9XY0(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            if (VRegisters[decodedOpcode.X] != VRegisters[decodedOpcode.Y])
            {
                ProgramCounter += 2;
            }
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
        /// Jumps to the address NNN plus V0.
        /// </summary>
        /// <param name="decodedOpcode"></param>
        private void _BNNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter = (ushort) (decodedOpcode.NNN + VRegisters[0x0]);
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
            
            VRegisters[0xF] = 0;

            for (var pixelY = 0; pixelY < decodedOpcode.N; pixelY++)
            {
                var spriteLine = Memory[IndexRegister + pixelY];

                for (var pixelX = 0; pixelX < 8; pixelX++)
                {
                    if ((spriteLine & (0x80 >> pixelX)) == 0)
                    {
                        continue;
                    }

                    try
                    {
                        var x = pixelX + VRegisters[decodedOpcode.X];
                        var y = pixelY + VRegisters[decodedOpcode.Y];

                        if (ToggleScreenPixel(x, y))
                        {
                            VRegisters[0xF] = 1;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }
                }
            }
        }

        private void _EXNN(DecodedOpcode decodedOpcode)
        {
            ProgramCounter += 2;

            // Skips the next instruction if the key stored in VX is pressed.
            if (decodedOpcode.N == 0xE)
            {
                if (Keys[VRegisters[decodedOpcode.X]])
                {
                    ProgramCounter += 2;
                }
            }

            // Skips the next instruction if the key stored in VX isn't pressed.
            if (decodedOpcode.N == 0x1)
            {
                if (!Keys[VRegisters[decodedOpcode.X]])
                {
                    ProgramCounter += 2;
                }
            }
        }
        #endregion

        private bool ToggleScreenPixel(int x, int y)
        {
            var flippedFromSet = Screen[x, y];
            Screen[x, y] = !Screen[x, y];

            return flippedFromSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadGame(FileInfo file)
        {
            Console.WriteLine($"Loading '{file.Name}'");

            var bytes = File.ReadAllBytes(file.FullName);

            LoadGame(bytes);
        }

        public void LoadGame(byte[] bytes)
        {
            for (var index = 0; index < bytes.Length; index++)
            {
                Memory[index + 512] = bytes[index];
            }
        }
    }
}