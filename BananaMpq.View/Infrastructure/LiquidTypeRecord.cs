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
    }
}