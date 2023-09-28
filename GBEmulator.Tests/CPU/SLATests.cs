namespace GBEmulator.Tests.CPU;

using Core.Enums;
using Abstractions;

[TestClass]
public class SLATests
{
    [TestMethod]
    public void SRA_ResultIsZeroAndHasCarry_ValueAndFlagsAreCorrect()
    {
        // Arrange: When D = 80h, and CY = 0
        var bus = AbstractionHelper.CreateBus();

        bus.GetCpuRegisters().PC = 0x9000;
        bus.GetCpuRegisters().SetFlag(Flag.Carry, false);
        
        bus.WriteMemory(0x9000, 0x16,false); // LD D 0x80
        bus.WriteMemory(0x9001, 0x80,false);
        
        bus.WriteMemory(0x9002, 0xCB, false);
        bus.WriteMemory(0x9003, 0x22, false);
        
        // Act
        for (var i = 0; i < 5; i++)
        {
            bus.ClockCpu();
        }
        
        // Assert
        Assert.AreEqual(expected: 0x00, actual: bus.GetCpuRegisters().D);
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.Carry));
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.Zero));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.HalfCarry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Subtraction));
    }
}