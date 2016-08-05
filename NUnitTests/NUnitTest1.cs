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
        public void Opcode3XNN_ValidInput_CorrectOutput()
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
    }
}