namespace GBEmulator.Hardware.Components;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

public class Timer: HardwareComponent, ITimer
{
    private int timerCycles = 0;
    private int divCycles = 0;

    private byte DIV
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.DIV, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.DIV, value, false);
    }
    private byte TMA
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.TMA, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.TMA, value, false);
    }
    
    private byte TAC
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.TAC, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.TAC, value, false);
    }
    
    private byte TIMA
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.TIMA, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.TIMA, value, false);
    }

    private int TACFrequency
    {
        get
        {
            switch (TAC & 0b11)
            {
                case 00:
                    return 1024;
                case 01:
                    return 16;
                case 10:
                    return 64;
                case 11:
                    return 256;
            }
            
            // Unreachable
            return -1;
        }
    }

    private const int DIVFrequency = 256;

    public void Clock(int numOfCycles)
    {
        ClockDiv(numOfCycles);
        ClockTimer(numOfCycles);
    }
    
    private void ClockDiv(int cycles) {
        divCycles += cycles;
        while (divCycles >= DIVFrequency) {
            DIV++;
            divCycles -= DIVFrequency;
        }
    }

    private void ClockTimer(int numOfCycles)
    {
        if ((TAC & 0b00000100) > 0) {
            timerCycles += numOfCycles;
            while (timerCycles >= TACFrequency) {
                TIMA++;
                timerCycles -= TACFrequency;
            }
            if (TIMA == 0xFF) {
                _bus.Interrupt(Interrupt.TIMER);
                TIMA = TMA;
            }
        }
    }
}