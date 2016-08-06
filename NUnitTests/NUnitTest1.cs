using System.Linq;
using NUnit.Framework;
using OctoChimp;

namespace NUnitTests
{
    [TestFixture]
    public class NUnitTest1
    {
        private Emulator _emulator;

        [SetUp]
        public void SetUp()
        {
            _emulator = new Emulator();
        }

        [Test]
        public void Opcode00E0_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0x00, 0xE0
            };

            _emulator.LoadGame(rom);

            _emulator.Screen[0, 0] = true;
            _emulator.Screen[1, 0] = true;
            _emulator.Screen[2, 0] = true;
            _emulator.Screen[3, 0] = true;
            _emulator.Screen[4, 0] = true;

            Assert.IsTrue(_emulator.Screen.Cast<bool>().Count(x => x) > 0);

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.IsTrue(_emulator.Screen.Cast<bool>().Count(x => x) == 0);
        }

        [Test]
        public void Opcode3XNN_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0x60, 0x01, // Set VRegister[0] to 0x01
                0x30, 0x01, // If VRegister[0] == 0x01
                0x00, 0xE0, // Skip this instruction
                0x60, 0x02  // Set VRegister[0] to 0x02
            };

            _emulator.LoadGame(rom);
            
            // Act + assert
            Assert.True(_emulator.ProgramCounter == 512);

            _emulator.EmulateCycle();
            Assert.True(_emulator.ProgramCounter == 514);

            _emulator.EmulateCycle();
            Assert.True(_emulator.ProgramCounter == 518);

            _emulator.EmulateCycle();
            Assert.True(_emulator.ProgramCounter == 520);
        }

        [Test]
        public void Opcode4XNN_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0x60, 0x00, // Set VRegister[0] to 0x00
                0x40, 0x01, // If VRegister[0] != 0x01
                0x00, 0xE0, // Skip this instruction
                0x00, 0xE0
            };

            _emulator.LoadGame(rom);

            // Act + assert
            Assert.True(_emulator.ProgramCounter == 512);
            _emulator.EmulateCycle();

            Assert.True(_emulator.ProgramCounter == 514);
            _emulator.EmulateCycle();

            Assert.True(_emulator.ProgramCounter == 518);
            _emulator.EmulateCycle();
        }

        [Test]
        public void Opcode5XY0_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0x60, 0x01, // Set VRegister[0] to 0x01
                0x61, 0x01, // Set VRegister[1] to 0x01
                0x50, 0x10, // If VRegister[0] == VRegister[1]
                0x00, 0xE0, // Skip this instruction
                0x00, 0xE0
            };

            _emulator.LoadGame(rom);

            // Act + assert
            Assert.True(_emulator.ProgramCounter == 512);
            _emulator.EmulateCycle();

            Assert.True(_emulator.ProgramCounter == 514);
            _emulator.EmulateCycle();

            Assert.True(_emulator.ProgramCounter == 516);
            _emulator.EmulateCycle();

            Assert.True(_emulator.ProgramCounter == 520);
            _emulator.EmulateCycle();
        }

        [Test]
        public void Opcode6XNN_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0x60, 0xFF,
                0x61, 0x01,
                0x62, 0x02,
            };

            _emulator.LoadGame(rom);

            // Act
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == 0xFF);
            Assert.True(_emulator.VRegisters[1] == 0x01);
            Assert.True(_emulator.VRegisters[2] == 0x02);
        }

        [Test]
        public void Opcode7XNN_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                // Add 1 to VRegister 0 
                0x70, 0x01,

                // Add 2 to VRegister 1 
                0x71, 0x02,

                // Add 3 to VRegister 2
                0x72, 0x03
            };

            _emulator.LoadGame(rom);

            // Set initial register values
            _emulator.VRegisters[0] = 0x00;
            _emulator.VRegisters[1] = 0x01;
            _emulator.VRegisters[2] = 0x02;

            // Act
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == 0x01);
            Assert.True(_emulator.VRegisters[1] == 0x03);
            Assert.True(_emulator.VRegisters[2] == 0x05);
        }

        [Test]
        public void Opcode8XY0_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                // Sets vregister 0 to vregister 1
                0x80, 0x10
            };

            _emulator.LoadGame(rom);

            // Set initial register values
            _emulator.VRegisters[0] = 0x00;
            _emulator.VRegisters[1] = 0x01;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == _emulator.VRegisters[1]);
            Assert.True(_emulator.VRegisters[0] == 0x01);
            Assert.True(_emulator.VRegisters[1] == 0x01);
        }

        [Test]
        public void Opcode8XY1_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                // Sets vregister 0 to vregister 0 | vregister 1
                0x80, 0x11
            };

            _emulator.LoadGame(rom);

            // Set initial register values
            _emulator.VRegisters[0] = 0x02;
            _emulator.VRegisters[1] = 0x03;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == (2 | 3));
            Assert.True(_emulator.VRegisters[0] == 3);
        }

        [Test]
        public void Opcode8XY2_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                // Sets vregister 0 to vregister 0 & vregister 1
                0x80, 0x12
            };

            _emulator.LoadGame(rom);

            // Set initial register values
            _emulator.VRegisters[0] = 0x02;
            _emulator.VRegisters[1] = 0x03;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == (2 & 3));
            Assert.True(_emulator.VRegisters[0] == 2);
        }

        [Test]
        public void Opcode8XY3_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                // Sets vregister 0 to vregister 0 ^ vregister 1
                0x80, 0x13
            };

            _emulator.LoadGame(rom);

            // Set initial register values
            _emulator.VRegisters[0] = 0x02;
            _emulator.VRegisters[1] = 0x03;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.VRegisters[0] == (2 ^ 3));
            Assert.True(_emulator.VRegisters[0] == 1);
        }

        [Test]
        public void OpcodeFX15_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0xF1, 0x15
            };

            _emulator.LoadGame(rom);

            _emulator.VRegisters[1] = 2;

            _emulator.DelayTimer = 0;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.DelayTimer == 1); // 2 - 1
        }

        [Test]
        public void OpcodeFX18_ValidInput_CorrectOutput()
        {
            // Arrange
            var rom = new byte[]
            {
                0xF1, 0x18
            };

            _emulator.LoadGame(rom);

            _emulator.VRegisters[1] = 2;

            _emulator.SoundTimer = 0;

            // Act
            _emulator.EmulateCycle();

            // Assert
            Assert.True(_emulator.SoundTimer == 1); // 2 - 1
        }
    }
}