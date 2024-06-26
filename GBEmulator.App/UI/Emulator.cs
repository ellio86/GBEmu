﻿namespace GBEmulator.App.UI;

using Core.Options;
using Hardware.Components;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

public partial class Emulator : Form
{
    private readonly GameBoy _gameBoy;
    private readonly AppSettings _appSettings;
    private bool _usingMenu;
    private bool _menuShown;

    public Emulator(AppSettings appSettings, GameBoy gameBoy)
    {
        _gameBoy = gameBoy ?? throw new ArgumentNullException(nameof(gameBoy));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        InitializeComponent();
    }

    private void Emulator_Load(object sender, EventArgs e)
    {
        _gameBoy.Initialise(this);
        ClientSize = new Size(Lcd.Width * _appSettings.Scale, Lcd.Height * _appSettings.Scale);

        menuStrip1.Hide();

        menuStrip1.MouseLeave += HideMenu;

        // Force Console
        if (_appSettings.ForceConsole) AllocConsole();
    }

    private void MoveMouse(object? sender, MouseEventArgs e)
    {
        if (menuStrip1.ClientRectangle.Contains(e.Location))
        {
            _usingMenu = true;
            menuStrip1.Show();
        }
        else
        {
            _usingMenu = false;
        }
    }

    private void ShowMenu(object? sender, EventArgs e)
    {
        menuStrip1.Show();
    }

    private void HideMenu(object? sender, EventArgs e)
    {
        if (!_usingMenu) menuStrip1.Hide();
    }


    private void Emulator_KeyDown(object sender, KeyEventArgs e)
    {
        _gameBoy.Controller.HandleKeyDown(GetKeyBit(e));
    }

    private void Emulator_KeyUp(object sender, KeyEventArgs e)
    {
        _gameBoy.Controller.HandleKeyUp(GetKeyBit(e));
    }

    private void Emulator_Closing(object sender, CancelEventArgs e)
    {
        _gameBoy.Save();
    }



    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    private byte GetKeyBit(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            // Right
            case Keys.D:
            case Keys.Right:
                return 0x11;

            // Left
            case Keys.A:
            case Keys.Left:
                return 0x12;

            // Up
            case Keys.W:
            case Keys.Up:
                return 0x14;

            // Down
            case Keys.S:
            case Keys.Down:
                return 0x18;

            // A
            case Keys.J:
            case Keys.Z:
                return 0x21;

            // B
            case Keys.K:
            case Keys.X:
                return 0x22;

            // Select
            case Keys.Space:
            case Keys.C:
                return 0x24;

            // Start
            case Keys.Enter:
            case Keys.V:
                return 0x28;
        }
        return 0;
    }

    private void OpenGameBoyRom(object sender, EventArgs e)
    {
        var openFileDialog1 = new OpenFileDialog
        {
            Filter = "GameBoy rom file|*.gb"
        };
        var result = openFileDialog1.ShowDialog();
        if (result == DialogResult.OK)
        {
            var file = openFileDialog1.FileName;
            _gameBoy.LoadNewRom(file);
        }
    }

    private void GameBoySave(object sender, EventArgs e)
    {
        _gameBoy.Save();
    }

    private void GameBoySaveAs(object sender, EventArgs e)
    {
        var saveFileDialog1 = new SaveFileDialog();
        saveFileDialog1.Filter = "GameBoy save file|*.sav";
        var result = saveFileDialog1.ShowDialog();
        if (result == DialogResult.OK)
        {
            _gameBoy.Save(saveFileDialog1.FileName);
        }
    }

    private void OpenGameBoySaveFile(object sender, EventArgs e)
    {
        var openFileDialog1 = new OpenFileDialog();
        openFileDialog1.Filter = "GameBoy save file|*.sav";
        var result = openFileDialog1.ShowDialog();
        if (result == DialogResult.OK)
        {
            var file = openFileDialog1.FileName;
            _gameBoy.LoadSaveFile(file);
        }
    }

    private void Exit(object sender, EventArgs e)
    {
        Close();
    }
}
