using System.Diagnostics;
using System.Runtime.InteropServices;
using CrystalMpq.DataFormats;

namespace BananaMpq.View.Infrastructure
{

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("LiquidTypeRecord: Id={Id}, Name={Name}, TypeClass={TypeClass}")]
    public struct LiquidTypeRecord
    {
        [Id] public int Id;
        public string Name;
        public int Flags;
        public int TypeClass;
        public int Unknown0;
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public int Unknown6;
        public int Unknown7;
        public int Unknown8;
        public int Unknown9;
        public int Unknown10;
        public int Unknown11;
        public int Unknown12;
        public int Unknown13;
        public int Unknown14;
        public int Unknown15;
        public int Unknown16;
        public int Unknown17;
        public int Unknown18;
        public int Unknown19;
        public int Unknown20;
        public int Unknown21;
        public int Unknown22;
        public int Unknown23;
        public int Unknown24;
        public int Unknown25;
        public int Unknown26;
        public int Unknown27;
        public int Unknown28;
        public int Unknown29;
        public int Unknown30;
        public int Unknown31;
        public int Unknown32;
        public int Unknown33;
        public int Unknown34;
        public int Unknown35;
        public int Unknown36;
        public int Unknown37;
        public int Unknown38;
        public int Unknown39;
        public int Unknown40;
    }
}