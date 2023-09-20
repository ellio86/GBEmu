using GBEmulator.App;

var registers = new Registers();
var cpu = new Cpu(registers);
var bus = new Bus(cpu);

while(true)
    cpu.Clock();