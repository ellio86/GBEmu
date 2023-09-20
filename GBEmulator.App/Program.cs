using System.Diagnostics;
using GBEmulator.App;

var registers = new Registers();
var cpu = new Cpu(registers);
var bus = new Bus(cpu, @"..\..\..\cpu_instrs.gb");

cpu.Reset();