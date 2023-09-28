namespace GBEmulator.Hardware.Components;
using Core.Enums;
using Core.Models;

public class Controller : HardwareComponent
{
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;

        public void HandleKeyDown(byte keyBit) {
            if ((keyBit & PAD_MASK) == PAD_MASK) 
            {
                pad = (byte)(pad & ~(keyBit & 0xF));
            } 
            else if((keyBit & BUTTON_MASK) == BUTTON_MASK) 
            {
                buttons = (byte)(buttons & ~(keyBit & 0xF));
            }
        }

        public void HandleKeyUp(byte keyBit) {
            if ((keyBit & PAD_MASK) == PAD_MASK) 
            {
                pad = (byte)(pad | (keyBit & 0xF));
            } 
            else if ((keyBit & BUTTON_MASK) == BUTTON_MASK) 
            {
                buttons = (byte)(buttons | (keyBit & 0xF));
            }
        }

        public void Update()
        {
            var gamePadStatus = _bus.ReadMemory((ushort)HardwareRegisters.P1);
            if (!((gamePadStatus & 0b00010000) > 0))
            {
                _bus.WriteMemory((ushort)HardwareRegisters.P1, (byte)((gamePadStatus & 0xF0) | pad));
                if (pad != 0xF)
                {
                    _bus.Interrupt(Interrupt.GAMEPADINPUT);
                }
            }

            if (!((gamePadStatus & 0b00100000) > 0))
            {
                _bus.WriteMemory((ushort)HardwareRegisters.P1, (byte)((gamePadStatus & 0xF0) | buttons));
                if (buttons != 0xF)
                {
                    _bus.Interrupt(Interrupt.GAMEPADINPUT);
                }
            }

            if ((gamePadStatus & 0b00110000) == 0b00110000)
            {
                _bus.WriteMemory((ushort)HardwareRegisters.P1, 0xFF);
            }
        }
}