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
                0x30, 0xFF,
                0x31, 0x01,
                0x32, 0x02,
            };

            _emulator.LoadGame(rom);

            // Act
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();
            _emulator.EmulateCycle();

            // Assert
            Assert.Equals(_emulator.VRegisters[0], 0xFF);
            Assert.Equals(_emulator.VRegisters[1], 0x01);
            Assert.Equals(_emulator.VRegisters[2], 0x02);
        }
    }
}