using System.Diagnostics;
using System.Diagnostics.Contracts;
using GBEmulator.Hardware;
using HtmlAgilityPack;

// This script generates each line of the instruction list found in InstructionHelper. This list is then used to populate the Instruction Lookup, so
// that we can process each instruction.

// Load HTML 
var doc = new HtmlDocument();
doc.Load(@"..\..\..\16BitopcodeTableHtml.html");

// Button elements in the HTML doc contain the info we need
var buttons = doc.DocumentNode.SelectNodes("/tbody/tr/td/button[1]");

foreach (var button in buttons)
{
    // Process the string to get the contents of the aria-label attribute
    var temp = button.OuterHtml.Replace("<button type=\"button\" aria-label=\"", "");
    var index = temp.IndexOf("\"");
    if (index >= 0)
        temp = temp.Substring(0, index);

    // Get Opcode and instruction from aria-label text
    var opcode = temp.Split(";")[0].Split(":")[1].Trim();
    var instruction = temp.Split(";")[1].Split(":")[1].Trim();

    // Strings for properties we need to generate
    string instructionType;
    string param1;
    string param2;

    // If the instruction has parameters
    if (instruction.Contains(" "))
    {
        instructionType = instruction.Split(" ")[0].Trim();
        param1 = GetFormattedParam(instruction.Split(" ")[1].Trim().Replace(",", ""));

        // Some instructions only take one parameter so this try-catch will make the second parameter NoParameter if it doesn't exist
        try
        {
            param2 = GetFormattedParam(instruction.Split(" ")[2].Trim().Replace(",", ""));
        }
        catch
        {
            param2 = "NoParameter";
        }
    }

    // If the instruction has no parameters
    else
    {
        instructionType = instruction;
        param1 = "NoParameter";
        param2 = "NoParameter";
    }

    // Get byte num and cycle num from span elements within the button
    var byteNum = button.ChildNodes[1].InnerText;
    var cycleNum = button.ChildNodes[2].InnerText;

    // Generate the line to be copied into the code
    var line = $"new() {{  Opcode = {opcode}, Type = InstructionType.{instructionType}, NumberOfBytes = {byteNum}, NumberOfCycles = {cycleNum}, Param1 = InstructionParam.{param1}, Param2 = InstructionParam.{param2} }},";
    
    // Display the line in the console so that it can be copied to source
    Console.WriteLine(line);
}

// Had to rename some Instruction Parameter names, this function parses each param so that we use the updated name
string GetFormattedParam(string param)
{
    switch (param)
    {
        case "(BC)":
            return "BCMem";
        case "(DE)":
            return "DEMem";
        case "(HL)":
            return "HLMem";
        case "(a16)":
            return "a16Mem";
        case "(HL+)":
            return "HLIMem";
        case "(HL-)":
            return "HLDMem";
        case "(C)":
            return "CMem";
        case "a16":
            return "a16Mem";
        default:
            return param;
    }
}

