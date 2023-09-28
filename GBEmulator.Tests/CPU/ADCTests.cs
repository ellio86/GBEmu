namespace GBEmulator.Tests.CPU;

using Core.Enums;
using Abstractions;

[TestClass]
public class ADCTests
{
    [TestMethod]
    public void ADC_ResultHasHalfCarry_ValueAndFlagsAreCorrect()
    {
        // Arrange: When A = E1h, E = 0Fh, CY = 1
        var bus = AbstractionHelper.CreateBus();

        bus.GetCpuRegisters().PC = 0x9000;
        bus.GetCpuRegisters().SetFlag(Flag.Carry, true);
        
        bus.WriteMemory(0x9000, 0x3E,false); // LD A 0xE1
        bus.WriteMemory(0x9001, 0xE1,false);
        
        bus.WriteMemory(0x9002, 0x1E, false); // LD E 0x0F
        bus.WriteMemory(0x9003, 0x0F, false);
        
        bus.WriteMemory(0x9004, 0x8B, false); // ADC A, E
        bus.WriteMemory(0x9005, 0x0F, false);
        
        // Act
        for (var i = 0; i < 3; i++)
        {
            bus.ClockCpu();
        }
        
        // Assert
        Assert.AreEqual(expected: 0xF1, actual: bus.GetCpuRegisters().A);
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Carry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Zero));
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.HalfCarry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Subtraction));
    }
}