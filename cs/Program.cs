using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace disasmmmm {

    internal class Program {
        private static string progName;
        private static List<int> binary = new List<int>();
        private static List<string> Codes = new List<string>();

        static Dictionary<byte, string> mod00 = new Dictionary<byte, string> {
            { 0x00, "[BX+SI]" },
            { 0x01, "[BX+DI]" },
            { 0x02, "[BP+SI]" },
            { 0x03, "[BP+DI]" },
            { 0x04, "[SI]" },
            { 0x05, "[DI]" },
            { 0x06, "disp16" },
            { 0x07, "[BX]" }
        };

        static Dictionary<byte, string> mod01 = new Dictionary<byte, string> {
            { 0x40, "[BX+SI] + " },
            { 0x41, "[BX+DI] + " },
            { 0x42, "[BP+SI] + " },
            { 0x43, "[BP+DI] + " },
            { 0x44, "[SI] + " },
            { 0x45, "[DI] + " },
            { 0x46, "[BP] + " },
            { 0x47, "[BX] + " }
        };

        static Dictionary<byte, string> mod10 = new Dictionary<byte, string> {
            { 0x80, "[BX+SI] + " },
            { 0x81, "[BX+DI] + " },
            { 0x82, "[BP+SI] + " },
            { 0x83, "[BP+DI] + " },
            { 0x84, "[SI] + " },
            { 0x85, "[DI] + " },
            { 0x86, "[BP] + " },
            { 0x87, "[BX] + " }
        };

        static Dictionary<byte, string> mod11 = new Dictionary<byte, string> {
            { 0xC0, "AX" },
            { 0xC1, "CX" },
            { 0xC2, "DX" },
            { 0xC3, "BX" },
            { 0xC4, "SP" },
            { 0xC5, "BP" },
            { 0xC6, "SI" },
            { 0xC7, "DI" }
        };

        static Dictionary<byte, string> opcodes = new Dictionary<byte, string> {
            { 0x88, "MOV" },
            { 0x89, "MOV" },
            { 0x8A, "MOV" },
            { 0x8B, "MOV" },
            { 0xB0, "MOV" },
            { 0xB8, "MOV" },
            { 0xC6, "MOV" },
            { 0xC7, "MOV" }
        };

        static Dictionary<byte, string> instants = new Dictionary<byte, string> {
            {0x90, "NOP / XCHG AX, AX" },
            {0x98, "CBW" },
            {0x99, "CWD" },
            {0x9B, "WAIT" },
            {0x9C, "PUSHF" },
            {0x9D, "POPF" },
            {0x9E, "SAHF" },
            {0x9F, "LAHF" },

            {0xC3, "RET / RETN" },
            {0xCB, "RETF" },
            {0xCC, "INT 3" },
            {0xCE, "INTO" },
            {0xCF, "IRET" },
            {0xD7, "XLAT" },

            {0xF0, "LOCK" },
            {0xF2, "REPNZ / REPNE" },
            {0xF3, "REP / REPZ / REPE" },
            {0xF4, "HLT" },
            {0xF5, "CMC" },

            {0xF8, "CLC" },
            {0xF9, "STC" },
            {0xFA, "CLI" },
            {0xFB, "STI" },
            {0xFC, "CLD" },
            {0xFD, "STD" },

            { 0x70, "JO" },
            { 0x71, "JNO" },
            { 0x72, "JB / JC/ JNAE" },
            { 0x73, "JAE / JNB/ JNC" },
            { 0x74, "JZ / JE" },
            { 0x75, "JNZ / JNE" },
            { 0x76, "JBE / JNA" },
            { 0x77, "JA / JNBE" },
            { 0x78, "JS" },
            { 0x79, "JNS" },
            { 0x7A, "JP / JPE" },
            { 0x7B, "JNP / JPO" },
            { 0x7C, "JL / JNGE" },
            { 0x7D, "JGE / JNL" },
            { 0x7E, "JLE / JNG" },
            { 0x7F, "JG / JNLE" },
            { 0xEB, "JMP (short)" },
            { 0xE9, "JMP (near)" },
            { 0xEA, "JMP (far)" }
        };

        static Dictionary<byte, string> GetModTable(int val) {
            return getMod(val) switch {
                0x00 => mod00,
                0x01 => mod01,
                0x10 => mod10,
                0x11 => mod11,
                _ => throw new ArgumentException("Invalid MOD value.")
            };
        }

        static int getMod(int val) {
            return (val >> 6) & 0b11;
        }

        static int getReg(int val) {
            return (val >> 3) & 0b111;
        }

        static int getRM(int val) {
            return val & 0b111;
        }

        static int CalculateTargetAddress(List<int> binary, int i, int cur) {
            if (i >= binary.Count - 1) return cur;

            byte opcode = (byte)binary[i];
            int offsetSize = GetOffsetSize(opcode);
            if (offsetSize == 0) return cur;

            int offset = 0;

            for (int j = 0; j < offsetSize; j++)
                offset |= (binary[i + 1 + j] << (j * 8));
            

            if (offsetSize == 1) offset = (sbyte)offset;
            else if (offsetSize == 2) offset = (short)offset;

            return cur + offset + offsetSize + 1;
        }


        static int GetOffsetSize(byte opcode, bool isFarJump = false, bool isProtectedMode = false) {
            if ((opcode >= 0x70 && opcode <= 0x7F) || opcode == 0xEB) {
                return 1;
            } else if (opcode == 0xE9 || opcode == 0xE8) {
                return isProtectedMode ? 4 : 2;
            } else if (opcode == 0xEA || isFarJump) {
                return 8;
            }
            return 0;
        }

        static void matchCodes() {
            for (int i = 0; i < binary.Count; i++) {
                byte opcode = (byte)binary[i];
                if (opcode == 0x00) {
                    ++i;
                    continue;
                }

                if (!instants.ContainsKey(opcode) && opcodes.ContainsKey(opcode)) {
                    string operation = opcodes[opcode];

                    if (operation == "MOV") {
                        if (i + 1 >= binary.Count) {
                            Console.WriteLine("Incomplete MOD-REG-R/M byte for MOV");
                            break;
                        }

                        int modrm = binary[++i];
                        int mod = getMod(modrm);
                        int reg = getReg(modrm);
                        int rm = getRM(modrm);

                        string firstArg = mod11.ContainsKey((byte)(0xC0 + reg)) ? mod11[(byte)(0xC0 + reg)] : $"UNKNOWN (REG: {reg})";
                        string secondArg = "";

                        if (mod == 0x11) {
                            secondArg = mod11.ContainsKey((byte)rm) ? mod11[(byte)rm] : $"UNKNOWN (R/M: {rm})";
                        } else if (mod == 0x00 && rm == 0x06) {
                            if (i + 2 >= binary.Count) {
                                Console.WriteLine("Missing 16-bit displacement for memory address");
                                break;
                            }

                            int disp16 = binary[++i] | (binary[++i] << 8);

                            if (mod11.ContainsKey((byte)(0xC0 + reg))) {
                                firstArg = mod11[(byte)(0xC0 + reg)];
                            } else {
                                firstArg = $"UNKNOWN (REG: {reg})";
                            }

                            secondArg = $"[0x{disp16:X4}]";
                        } else {
                            var modTable = GetModTable(mod);
                            if (modTable.ContainsKey((byte)rm)) {
                                secondArg = modTable[(byte)rm];
                            } else {
                                secondArg = $"UNKNOWN (R/M: {rm})";
                            }

                            if (mod == 0x01) {
                                if (i + 1 >= binary.Count) {
                                    Console.WriteLine("Missing 8-bit displacement");
                                    break;
                                }
                                int disp8 = binary[++i];
                                secondArg += $" + 0x{disp8:X2}";
                            } else if (mod == 0x10) {
                                if (i + 2 >= binary.Count) {
                                    Console.WriteLine("Missing 16-bit displacement");
                                    break;
                                }
                                int disp16 = binary[++i] | (binary[++i] << 8);
                                secondArg += $" + 0x{disp16:X4}";
                            }
                        }


                        if (opcode >= 0xB0 && opcode <= 0xBF) {
                            if (i + 1 >= binary.Count) {
                                Console.WriteLine("Missing immediate value");
                                break;
                            }
                            int imm = binary[++i];
                            firstArg = mod11.ContainsKey((byte)(0xC0 + (opcode & 0x07)))
                                    ? mod11[(byte)(0xC0 + (opcode & 0x07))]
                                    : "UNKNOWN";
                            secondArg = $"0x{imm:X2}";
                        } else if (opcode == 0xC6 || opcode == 0xC7) {
                            if (i + 1 >= binary.Count) {
                                Console.WriteLine("Missing immediate value");
                                break;
                            }
                            int imm = binary[++i];
                            secondArg += $" 0x{imm:X2}";
                        }

                        if (!(firstArg == "UNKNOWN" && secondArg == "UNKNOWN")) {
                            Console.WriteLine($"CS:{i}   {operation} {firstArg}, {secondArg}");
                            //Console.WriteLine($"        MOD: {mod}, REG: {reg}, R/M: {rm}\n");
                        }
                    }
                } else {
                    
                    if (instants.TryGetValue(opcode, out string value)) {
                        if (instants.ContainsKey(opcode) && (opcode >= 0x70 && opcode <= 0x7F || opcode == 0xEB || opcode == 0xE9 || opcode == 0xEA)) {
                            int targetAddress = CalculateTargetAddress(binary, i, i);
                            Console.WriteLine($"CS:{i}   {value} 0x{targetAddress:X4}");
                        } else {
                            Console.WriteLine($"CS:{i}   {value}");
                        }
                    }
                }
            }
        }

        static void ReadBinary() {
            using (FileStream fopen = new FileStream(progName, FileMode.Open, FileAccess.Read)) {
                if (!fopen.CanRead)
                    return;

                while (fopen.Position < fopen.Length) {
                    binary.Add(fopen.ReadByte());
                }
            }
        }

        static int Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Only 1 argument is allowed");
                return 1;
            }
            progName = args[0].ToLower();

            if (!progName.EndsWith(".com") && !progName.EndsWith(".exe")) {
                Console.WriteLine("Invalid program name");
                return 2;
            }

            ReadBinary();
            matchCodes();

            return 0;
        }
    }
}