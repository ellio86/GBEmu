﻿using System.Drawing;

namespace GBEmulator.Hardware;
using Core.Interfaces;
using Core.Enums;

public class Bus : IBus
{
    // Hardware components connected to bus
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    private readonly IWindow _window;
    private readonly ICartridge _cartridge;
    
    // Memory
    private readonly byte[] _memory = new byte[1024 * 64];
    private readonly byte[] _rom = new byte[1024 * 32];
    private string? _romPath;

    // ROM loaded?
    public bool CartridgeLoaded { get; private set; } = false;
    public bool SkipBoot { get; set; }

    public Bus(ICpu cpu, ITimer timer, IPpu ppu, IWindow window, ICartridge cartridge, bool skipBoot = false)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _cartridge = cartridge;
        SkipBoot = skipBoot;
        
        // Connect Components
        _cpu.ConnectToBus(this);
        _timer.ConnectToBus(this);
        _ppu.ConnectToBus(this);
        Reset();
    }

    public void Interrupt(Interrupt interruptRequest) => _cpu.Interrupt(interruptRequest);


    public void WriteMemory(ushort address, byte value)
    {
        if (address <= 0x7FFF)
        {
            _cartridge.WriteToRom(address, value);
            return;
        }

        if (address is > 0x9FFF and <= 0xBFFF)
        {
            _cartridge.WriteExternalMemory(address, value);
            return;
        }
        
        _memory[address] = value;
    }
    public byte ReadMemory(ushort address)
    {
        // GBC ONLY - https://gbdev.io/pandocs/CGB_Registers.html
        if (address == 0xFF4D)
        {
            return 0xFF;
        }

        // Temp for debugging using GameBoy Doctor - https://github.com/robert/gameboy-doctor
        if (address == 0xFF44)
        {
           // return 0x90;
        }

        if (address <= 0x3FFF)
        {
            return _cartridge.ReadRom(address);
        }
        
        if (address <= 0x7FFF)
        {
            return _cartridge.ReadUpperRom(address);
        }

        if (address is <= 0xBFFF and > 0x9FFF)
        {
            return _cartridge.ReadExternalMemory(address);
        }
        return _memory[address];
    }
    
    public void Reset()
    {
        _cpu.Reset(SkipBoot);
        
        ClearMemory();
        SetHardwareRegistersToDefaultValues();
    }

    public void FlipWindow() => _window.Flip();

    public void SetBitmap(Bitmap bmp) => _window.SetBitmap(bmp);

    public void ReloadRom()
    {
        if (_romPath is not null)
        {
            ReadFileToRom(_romPath);
        }
        
        for (var i = 0; i < _rom.Length; i++)
        {
            WriteMemory((ushort)i, _rom[i]);
        }
    }

    public void LoadRom(string path)
    {
        _romPath = path;
        ReloadRom();

        CartridgeLoaded = true;

        if (!SkipBoot)
        {
            LoadBootRom();
        }
    }

    private void LoadBootRom()
    {
        // TODO: Add app setting for boot rom path so that the boot rom can be easily swapped
        using var stream = File.Open("..\\..\\..\\..\\GBEmulator.Hardware\\dmg0_boot.bin", FileMode.Open);
        _ = stream.Read(_rom, 0, 255);
        for (var i = 0; i < 256; i++)
        {
            WriteMemory((ushort)i, _rom[i]);
        }
    }

    private void ReadFileToRom(string path)
    {
        using var stream = File.Open(path, FileMode.Open);
        _ = stream.Read(_rom, 0, 32 * 1024);
    }

    private void ClearMemory()
    {
        for (var i = 0; i < _memory.Length; i++)
        {
            _memory[i] = 0x00;
        }
    }

    private void SetHardwareRegistersToDefaultValues()
    {
        WriteMemory((ushort)HardwareRegisters.P1, 0xCF);
        WriteMemory((ushort)HardwareRegisters.SC, 0x7E);
        WriteMemory((ushort)HardwareRegisters.DIV, 0xAB);
        WriteMemory((ushort)HardwareRegisters.TAC, 0xF8);
        WriteMemory((ushort)HardwareRegisters.IF, 0xE1);
        WriteMemory((ushort)HardwareRegisters.NR10, 0x80);
        WriteMemory((ushort)HardwareRegisters.NR11, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR12, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR13, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR14, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR21, 0x3F);
        WriteMemory((ushort)HardwareRegisters.NR23, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR24, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR30, 0x7F);
        WriteMemory((ushort)HardwareRegisters.NR31, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR32, 0x9F);
        WriteMemory((ushort)HardwareRegisters.NR33, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR33, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR41, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR44, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR50, 0x77);
        WriteMemory((ushort)HardwareRegisters.NR51, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR51, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR52, 0xF1);
        WriteMemory((ushort)HardwareRegisters.LCDC, 0x91);
        WriteMemory((ushort)HardwareRegisters.STAT, 0x85);
        WriteMemory((ushort)HardwareRegisters.DMA, 0xFF);
        WriteMemory((ushort)HardwareRegisters.DMA, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BGP, 0xFC);
        WriteMemory((ushort)HardwareRegisters.KEY1, 0xFF);
        WriteMemory((ushort)HardwareRegisters.VBK, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA1, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA2, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA3, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA4, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA5, 0xFF);
        WriteMemory((ushort)HardwareRegisters.RP, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BCPS, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BCPD, 0xFF);
        WriteMemory((ushort)HardwareRegisters.OCPS, 0xFF);
        WriteMemory((ushort)HardwareRegisters.OCPD, 0xFF);
        WriteMemory((ushort)HardwareRegisters.SVBK, 0xFF);
    }
}
