﻿namespace GBEmulator.Core.Interfaces;
using Enums;

public interface ICpu : IHardwareComponent
{
    /// <summary>
    /// Execute one tick of the clock
    /// </summary>
    public int Clock(TextWriter writer);

    /// <summary>
    /// Reset function - clears memory, flags, registers etc
    /// </summary>
    public void Reset(bool skipBoot);

    public void Interrupt(Interrupt requestedInterrupt);

    public void HandleInterrupts();

}