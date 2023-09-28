namespace GBEmulator.Tests.CPU;

using Core.Enums;
using Abstractions;

[TestClass]
public class SRATests
{
    [TestMethod]
    public void SRA_ResultHasNoFlagsSet_ValueAndFlagsAreCorrect()
    {
        // Arrange: When D = 8Ah, and CY = 0
        var bus = AbstractionHelper.CreateBus();

        bus.GetCpuRegisters().PC = 0x9000;
        bus.GetCpuRegisters().SetFlag(Flag.Carry, false);
        
        bus.WriteMemory(0x9000, 0x16,false); // LD D 0x80
        bus.WriteMemory(0x9001, 0x8A,false);
        
        bus.WriteMemory(0x9002, 0xCB, false); // SRA D
        bus.WriteMemory(0x9003, 0x2A, false);
        
        // Act
        for (var i = 0; i < 4; i++)
        {
            bus.ClockCpu();
        }
        
        // Assert
        Assert.AreEqual(expected: 0xC5, actual: bus.GetCpuRegisters().D);
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Carry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Zero));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.HalfCarry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Subtraction));
    }
}