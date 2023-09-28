namespace GBEmulator.Tests;

using Core.Interfaces;
using System.Drawing;

public class TestLcd : ILcd
{
        public Bitmap Bitmap { get; }
        public void SetPixel(int x, int y, int colour)
        {
                
        }

        public int GetPixel(int x, int y)
        {
                return 0;
        }
}