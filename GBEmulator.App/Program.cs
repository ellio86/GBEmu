using System.Diagnostics;
using GBEmulator.App;

var registers = new Registers();
var cpu = new Cpu(registers);
var bus = new Bus(cpu, @"..\..\..\03-op sp,hl.gb");

cpu.Reset();