namespace GBEmulator.Tests.CPU;
using Hardware;

[TestClass]
public class ADDTests
{
    [TestMethod]
    public void ADD_AdditionIsZeroWithBothCarries_AppropriateFlagsSetAndResultIsZero()
    {
        // Arrange
        var registers = new Registers();
        var cpu = new Cpu(registers);
        var timer = new Timer();
        var bus = new Bus(cpu, timer);

        // Load Values into registers
        bus.WriteMemory(0x0000, 0x3E); // LD A 0x3A
        bus.WriteMemory(0x0001, 0x3A);

        bus.WriteMemory(0x0002, 0x06); // LD B 0xC6
        bus.WriteMemory(0x0003, 0xC6);

        // Add A and B register
        bus.WriteMemory(0x0004, 0x80); // ADD A B

        // Act
        for (var i = 0; i < 5; i++)
        {
            cpu.Clock();
        }

        // Expected result -> F = 0b10110000 or 0xB0 i.e. Zero flag set + Half Carry Set + Carry Set
        //                    A = 0
        Assert.AreEqual(expected: 0xB0, actual: registers.F);
        Assert.AreEqual(expected: 0x00, actual: registers.A);
    }

    [TestMethod]
    public void ADD_AdditionHasHalfCarry_AppropriateFlagsSet()
    {
        // Arrange
        var registers = new Registers();
        var cpu = new Cpu(registers);
        var timer = new Timer();
        var bus = new Bus(cpu, timer);

        // Load Values into registers
        bus.WriteMemory(0x0000, 0x3E); // LD A 0b00001111
        bus.WriteMemory(0x0001, 0b00001111);

        bus.WriteMemory(0x0002, 0x06); // LD B 0b00000111
        bus.WriteMemory(0x0003, 0b00000111);

        // Add A and B register
        bus.WriteMemory(0x0004, 0x80); // ADD A B

        // Act
        for (var i = 0; i < 5; i++)
        {
            cpu.Clock();
        }

        // Expected result -> F = 0b00100000 or 0x20 i.e. Half Carry Set 
        //                    A = 0
        Assert.AreEqual(expected: 0b00100000, actual: registers.F);
        Assert.AreEqual(expected: 22, actual: registers.A);
    }

    [TestMethod]
    public void ADD_16BitAdditionHasHalfCarryAndCarry_AppropriateFlagsSet()
    {
        // Arrange
        var registers = new Registers();
        var timer = new Timer();
        var cpu = new Cpu(registers);
        var bus = new Bus(cpu, timer);

        // Load Values into registers
        bus.WriteMemory(0x0000, 0x21); // LD HL 0x8A23
        bus.WriteMemory(0x0001, 0x23);
        bus.WriteMemory(0x0002, 0x8A);

        // Add A and B register
        bus.WriteMemory(0x0003, 0x29); // ADD HL HL

        // Act
        for (var i = 0; i < 5; i++)
        {
            cpu.Clock();
        }

        // Expected result -> F = 0b00100000 or 0x20 i.e. Half Carry Set 
        //                    A = 0
        Assert.AreEqual(expected: 0b00110000, actual: registers.F);
        Assert.AreEqual(expected: 0x1446, actual: registers.HL);
    }

    [TestMethod]
    public void ADD_16BitAdditionHasHalfCarry_AppropriateFlagsSet()
    {
        // Arrange
        var registers = new Registers();
        var cpu = new Cpu(registers);
        var timer = new Timer();
        var bus = new Bus(cpu, timer);

        // Load Values into registers
        bus.WriteMemory(0x0000, 0x01); // LD BC 0x0605
        bus.WriteMemory(0x0001, 0x05);
        bus.WriteMemory(0x0002, 0x06);

        bus.WriteMemory(0x0003, 0x21); // LD HL 0x8A23
        bus.WriteMemory(0x0004, 0x23);
        bus.WriteMemory(0x0005, 0x8A);

        // Add A and B register
        bus.WriteMemory(0x0006, 0x09); // ADD HL BC

        // Act
        for (var i = 0; i < 7; i++)
        {
            cpu.Clock();
        }

        // Expected result -> F = 0b00100000 or 0x20 i.e. Half Carry Set 
        //                    A = 0
        Assert.AreEqual(expected: 0b00100000, actual: registers.F);
        Assert.AreEqual(expected: 0x9028, actual: registers.HL);
    }
}