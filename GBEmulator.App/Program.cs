using GBEmulator.App;
using GBEmulator.Core.Interfaces;
using System.Diagnostics;

var registers = new Registers();
var cpu = new Cpu(registers);
var bus = new Bus(cpu, @"..\..\..\cpu_instrs.gb");
bus.Reset();

var stopwatch = Stopwatch.StartNew();
var clockRunning = true;
using var writer = new StreamWriter(@"..\..\..\log.txt");
while (clockRunning)
{
    // pause for 0.25 milliseconds to simulate 4KHz (4000 times a second)
    //if (_stopwatch.ElapsedMilliseconds < 0.25)
    //{
    //    continue;
    //}
    //_stopwatch = Stopwatch.StartNew();

    // Tick the clock
    var cycleNum = cpu.Clock(writer);

    // Listen to serial io port for test results
    if (bus.ReadMemory(0xff02) == 0x81)
    {
        var c = (char)bus.ReadMemory(0xff01);
        Console.Write(c);
        bus.WriteMemory(0xff02, 0x00);
    }
}
