namespace OctoChimp
{
    public class DecodedOpcode
    {
        public DecodedOpcode(ushort currentOpcode)
        {
            Opcode = currentOpcode;
        }

        public ushort Opcode { get; set; }

        public ushort NNN => (ushort)(Opcode & 0x0FFF);

        public ushort NN => (ushort)(Opcode & 0x00FF);

        public ushort N => (ushort)(Opcode & 0x000F);

        public byte X => (byte)((Opcode & 0x0F00) >> 8);

        public byte Y => (byte)((Opcode & 0x00F0) >> 4);
    }
}
