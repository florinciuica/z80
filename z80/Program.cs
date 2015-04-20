﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace z80
{
    public static class Z80
    {
        private static byte[] registers = new byte[26];
        private static byte[] ram;

        private const byte B = 0;
        private const byte C = 1;
        private const byte D = 2;
        private const byte E = 3;
        private const byte H = 4;
        private const byte L = 5;
        private const byte F = 6;
        private const byte A = 7;
        private const byte Bp = 8;
        private const byte Cp = 9;
        private const byte Dp = 10;
        private const byte Ep = 11;
        private const byte Hp = 12;
        private const byte Lp = 13;
        private const byte Fp = 14;
        private const byte Ap = 15;

        private const byte I = 16;
        private const byte R = 17;

        private const byte IX = 18;
        private const byte IY = 20;
        private const byte SP = 22;
        private const byte PC = 24;

        private static DateTime clock = DateTime.UtcNow;

        private static void Wait(float ET)
        {
            var ticks = (long) (ET*1000);
            TimeSpan sleep = clock + new TimeSpan(ticks) - DateTime.UtcNow;
            if (sleep.Ticks > 0) Thread.Sleep(sleep);
            clock = clock + new TimeSpan(ticks);
        }

        public static void Parse()
        {
            if (Halted) return;
            byte mc = ReadProgram();
            byte hi = (byte) (mc >> 6);
            byte lo = (byte) (mc & 0x07);
            byte r =  (byte) ((mc >> 3) & 0x07);
            switch (hi)
            {
                case 0:
                    if (lo == 6)
                    {
                        // LD r,n
                        byte n = ReadProgram();
                        registers[r] = n;
                        Console.WriteLine("LD {0}, {1}", RName(r), n, mc);
                        Wait(1.75F);
                        return;
                    }
                    break;
                    if (mc == 0)
                    {
                        // NOP
                        Console.WriteLine("NOP");
                        return;
                    }
                case 1:
                {
                    if (r == 6)
                    {
                        if (lo == 6)
                        {
                            //HALT
                            Console.WriteLine("HALT");
                            Halted = true;
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (lo == 6)
                        {
                            // LD r, (HL)
                            Console.WriteLine("LD {0}, (HL)", RName(r));
                            var addr = (registers[H] << 8) + registers[L];
                            registers[r] = ram[addr];
                            Wait(1.75F);
                            return;
                        } 
                        else
                        {
                            // LD r, r'
                            Console.WriteLine("LD {0}, {1}", RName(r), RName(lo));
                            registers[r] = registers[lo];
                            Wait(1.0F);
                            return;
                        }
                    }
                }
            }
            Console.WriteLine("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
            Halted = true;
        }

        private static string RName(byte n)
        {
            switch (n)
            {
                case 0: return "B";
                case 1: return "C";
                case 2: return "D";
                case 3: return "E";
                case 4: return "H";
                case 5: return "L";
                case 6: return "F";
                case 7: return "A";
            }
            return "";
        }

        private static byte ReadProgram()
        {
            var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
            var ret = ram[pc];
            pc++;
            registers[PC] = (byte) (pc >> 8);
            registers[PC + 1] = (byte) (pc & 0xFF);
            return ret;
        }

        public static void Reset(byte[] ram)
        {
            Array.Clear(registers, 0, registers.Length);
            Z80.ram = ram;
            clock = DateTime.UtcNow;
        }

        public static bool Halted { get; private set; }

        public static string DumpState()
        {
            var ret = "BC    DE    HL    F  A" + Environment.NewLine;
            int i = 0;
            for (; i < 8; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine + "BC'   DE'   HL'   F' A'" + Environment.NewLine;
            for (; i < 16; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine + "I  R  IX    IY    SP    PC    " + Environment.NewLine;
            for (; i < registers.Length; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine;
            return ret;
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] ram = new byte[65536];
            Array.Clear(ram, 0, ram.Length);
            ram[0x0000] = 0x26;
            ram[0x0001] = 0x80; // LD H, 128
            ram[0x0002] = 0x6C; // LD L, H
            ram[0x0003] = 0x7E; // LD A, (HL)
            ram[0x0004] = 0x76; // HALT
            ram[0x8080] = 0x42; // Data
            Z80.Reset(ram);
            while (!Z80.Halted)
            {
                Z80.Parse();
                //Console.Write(Z80.DumpState());
            }
            Console.WriteLine(Environment.NewLine+Z80.DumpState());
            for (int i = 0; i < 0x80; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i%8 == 7) Console.Write("  ");
                if (i%16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (int i = 0x8080; i < 0x80A0; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}