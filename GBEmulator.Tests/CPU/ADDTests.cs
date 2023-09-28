namespace GBEmulator.Tests.CPU;

using Core.Enums;
using Abstractions;

[TestClass]
public class ADDTests
{
    [TestMethod]
    public void ADD_AdditionIsZeroWithBothCarries_AppropriateFlagsSetAndResultIsZero()
    {
        // Arrange
        var bus = AbstractionHelper.CreateBus();
        
        // Set program counter to 0x9000 as writes to < 0x8000 will try to interact with a non-existent cartridge
        bus.GetCpuRegisters().PC = 0x9000;
        
        // Load test program into memory
        bus.WriteMemory(0x9000, 0x3E, false); // LD A 0x3A
        bus.WriteMemory(0x9001, 0x3A, false);

        bus.WriteMemory(0x9002, 0x06, false); // LD B 0xC6
        bus.WriteMemory(0x9003, 0xC6, false);

        // Add A and B register
        bus.WriteMemory(0x9004, 0x80, false); // ADD A B

        // Act
        for (var i = 0; i < 5; i++)
        {
            bus.ClockCpu();
        }

        // Expected result -> F = 0b10110000 or 0xB0 i.e. Zero flag set + Half Carry Set + Carry Set
        //                    A = 0
        Assert.AreEqual(expected: 0xB0, actual: bus.GetCpuRegisters().F);
        Assert.AreEqual(expected: 0x00, actual: bus.GetCpuRegisters().A);
    }

    [TestMethod]
    public void ADD_AdditionHasHalfCarry_AppropriateFlagsSet()
    {
        // Arrange
        var bus = AbstractionHelper.CreateBus();
        bus.GetCpuRegisters().PC = 0x9000;

        // Load test program into memory
        bus.WriteMemory(0x9000, 0x3E, false); // LD A 0b00001111
        bus.WriteMemory(0x9001, 0b00001111, false);

        bus.WriteMemory(0x9002, 0x06, false); // LD B 0b00000111
        bus.WriteMemory(0x9003, 0b00000111, false);

        // Add A and B register
        bus.WriteMemory(0x9004, 0x80, false); // ADD A B

        

        // Act
        for (var i = 0; i < 5; i++)
        {
            bus.ClockCpu();
        }

        // Expected result -> F = 0b00100000 or 0x20 i.e. Half Carry Set 
        //                    A = 0
        Assert.AreEqual(expected: 0b00100000, actual: bus.GetCpuRegisters().F);
        Assert.AreEqual(expected: 22, actual: bus.GetCpuRegisters().A);
    }

    [TestMethod]
    public void ADD_16BitAdditionHasHalfCarryAndCarry_AppropriateFlagsSet()
    {
        // Arrange
        var bus = AbstractionHelper.CreateBus();
        bus.GetCpuRegisters().PC = 0x9000;

        // Load test program into memory
        bus.WriteMemory(0x9000, 0x21, false); // LD HL 0x8A23
        bus.WriteMemory(0x9001, 0x23, false);
        bus.WriteMemory(0x9002, 0x8A, false);

        // Add A and B register
        bus.WriteMemory(0x9003, 0x29, false); // ADD HL HL

        // Act
        for (var i = 0; i < 5; i++)
        {
            bus.ClockCpu();
        }

        // Assert
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.HalfCarry));
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.Carry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Subtraction));
        Assert.AreEqual(expected: 0x1446, actual: bus.GetCpuRegisters().HL);
    }

    [TestMethod]
    public void ADD_16BitAdditionHasHalfCarry_AppropriateFlagsSet()
    {
        // Arrange
        var bus = AbstractionHelper.CreateBus();
        bus.GetCpuRegisters().PC = 0x9000;

        // Load test program into memory
        bus.WriteMemory(0x9000, 0x01, false); // LD BC 0x0605
        bus.WriteMemory(0x9001, 0x05, false);
        bus.WriteMemory(0x9002, 0x06, false);

        bus.WriteMemory(0x9003, 0x21, false); // LD HL 0x8A23
        bus.WriteMemory(0x9004, 0x23, false);
        bus.WriteMemory(0x9005, 0x8A, false);

        // Add A and B register
        bus.WriteMemory(0x9006, 0x09, false); // ADD HL BC

        // Act
        for (var i = 0; i < 7; i++)
        {
            bus.ClockCpu();
        }

        // Assert
        Assert.AreEqual(expected: true, actual: bus.GetCpuRegisters().GetFlag(Flag.HalfCarry));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Subtraction));
        Assert.AreEqual(expected: false, actual: bus.GetCpuRegisters().GetFlag(Flag.Carry));
        Assert.AreEqual(expected: 0x9028, actual: bus.GetCpuRegisters().HL);
    }
}