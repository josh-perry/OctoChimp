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
    }
}