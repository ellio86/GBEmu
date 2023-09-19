using GBEmulator.App;

var cpu = new Cpu(new Registers());
var bus = new Bus(cpu);

bus.WriteMemory(0x0000, 0x0E);
bus.WriteMemory(0x0001, 0x45);

for (var i = 0; i < 2; i++)
{
    cpu.Clock();
}

Console.WriteLine("done");