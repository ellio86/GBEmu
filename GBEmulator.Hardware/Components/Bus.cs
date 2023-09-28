namespace GBEmulator.Hardware.Components;

using Core.Interfaces;
using Core.Enums;
using System.Drawing;

public class Bus : IBus
{
    // Hardware components connected to bus
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    private readonly IWindow _window;
    
    private ICartridge _cartridge;

    // Memory
    private readonly byte[] _memory = new byte[1024 * 64];

    // ROM loaded?
    public bool CartridgeLoaded { get; private set; } = false;

    public Bus(ICpu cpu, ITimer timer, IPpu ppu, IWindow window, IController controller)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
        _window = window ?? throw new ArgumentNullException(nameof(window));

        // Connect Components
        _cpu.ConnectToBus(this);
        _timer.ConnectToBus(this);
        _ppu.ConnectToBus(this);
        controller.ConnectToBus(this);
        Reset();
    }

    public IRegisters GetCpuRegisters() => _cpu.Registers;
    public int ClockCpu(StreamWriter? logWriter = null) => _cpu.Clock(logWriter);
    public void ClockPpu(int cycleNum) => _ppu.Clock(cycleNum);
    public void ClockTimer(int cycleNum) => _timer.Clock(cycleNum);
    public void HandleInterrupts() => _cpu.HandleInterrupts();

    public void LoadCartridge(ICartridge cartridgeToLoad)
    {
        _cartridge = cartridgeToLoad ?? throw new ArgumentNullException(nameof(cartridgeToLoad));
        CartridgeLoaded = true;
    }

    /// <summary>
    /// Requests the provided interrupt that will get handled after executing an instruction
    /// </summary>
    /// <param name="interruptRequest"></param>
    public void Interrupt(Interrupt interruptRequest) => _cpu.Interrupt(interruptRequest);

    /// <summary>
    /// Whenever we read or write, we also need to tick our components 4t cycles (or 2 in the case of the ppu)
    /// </summary>
    public void TickComponents()
    {
        _ppu.Clock(4);
        _timer.Clock(4);
    }
    
    public void WriteMemory(ushort address, byte value, bool consumesCycle = true)
    {
        // We want to tick components 4t cycles every 1m cycle and reading takes 1m cycle
        if(consumesCycle) TickComponents();
        
        // If the current program is trying to write to cartridge rom
        if (address <= 0x7FFF)
        {
            _cartridge.WriteToRom(address, value);
            return;
        }

        // If the current program is trying to write to external memory (located in cartridge and controlled by MBCs)
        if (address is > 0x9FFF and <= 0xBFFF)
        {
            _cartridge.WriteExternalMemory(address, value);
            return;
        }

        // If the current program is trying to write to the DMA register, initiate a DMA transfer
        if (address == (ushort)HardwareRegisters.DMA)
        {
            DMATransfer(value);
            return;
        }

        _memory[address] = value;
    }

    /// <summary>
    /// Copies 0x9F bytes of data starting at address 0x[data]00 to the OAM
    /// </summary>
    /// <param name="data"></param>
    private void DMATransfer(byte data)
    {
        var address = (ushort)(data << 8);
        for (var i = 0; i < 0xA0; i++)
        {
            WriteMemory((ushort)(0xFE00 + i), ReadMemory((ushort)(address + i)));
        }
    }

    /// <summary>
    /// Reads the byte from the specified address in memory. If necessary, control is handed to the cartridge MBC 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public byte ReadMemory(ushort address, bool consumesCycle = true)
    {
        // We want to tick components 4t cycles every 1m cycle and reading takes 1m cycle
        if(consumesCycle) TickComponents();
        
        // 0xFF4D is GBC ONLY - https://gbdev.io/pandocs/CGB_Registers.html
        if (address == 0xFF4D)
        {
            return 0xFF;
        }

        // If the current program is trying to read from cartridge rom, let cartridge/cartridge mbc handle request
        if (address <= 0x3FFF)
        {
            return _cartridge.ReadRom(address);
        }

        // If the current program is trying to read from upper cartridge rom, let cartridge/cartridge mbc handle request
        if (address <= 0x7FFF)
        {
            return _cartridge.ReadUpperRom(address);
        }

        // If the current program is trying to read from external ram, let cartridge/cartridge mbc handle request
        if (address is <= 0xBFFF and > 0x9FFF)
        {
            return _cartridge.ReadExternalMemory(address);
        }

        return _memory[address];
    }

    /// <summary>
    /// Resets the CPU, Clears memory and resets the hardware registers to their default values
    /// </summary>
    public void Reset()
    {
        _cpu.Reset();
        ClearMemory();
        SetHardwareRegistersToDefaultValues();
    }

    /// <summary>
    /// Draws the frame that has just been rendered onto the screen
    /// </summary>
    public void FlipWindow() => _window.Flip();

    /// <summary>
    /// Associate the bitmap to the output window
    /// </summary>
    /// <param name="bmp"></param>
    public void SetBitmap(Bitmap bmp) => _window.SetBitmap(bmp);

    /// <summary>
    /// Sets every byte in the memory array to 0x00
    /// </summary>
    private void ClearMemory()
    {
        for (var i = 0; i < _memory.Length; i++)
        {
            _memory[i] = 0x00;
        }
    }

    /// <summary>
    /// Sets Hardware registers to the default values. See https://gbdev.io/pandocs/Hardware_Reg_List.html
    /// </summary>
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