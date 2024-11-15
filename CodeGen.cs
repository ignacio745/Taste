using System;
using System.IO;

namespace Taste {
	
public enum Op { // opcodes
	ADD, SUB, MUL, DIV, EQU, LSS, GTR, NEG,
	LOAD, LOADG, STO, STOG, CONST,
	CALL, RET, ENTER, LEAVE, JMP, FJMP, WRITE, TECLADO
}

public class CodeGenerator {
	
	string[] opcode =
	  {"ADD  ", "SUB  ", "MUL  ", "DIV  ", "EQU  ", "LSS  ", "GTR  ", "NEG  ",
	   "LOAD ", "LOADG", "STO  ", "STOG ", "CONST", "CALL ", "RET  ", "ENTER",
	   "LEAVE", "JMP  ", "FJMP ", "WRITE", "TECLADO"};

	public int progStart;	// address of first instruction of main program
	public int pc;				// program counter
	byte[] code = new byte[3000];

	// data for Interpret
	int[] globals = new int[100];
	int[] stack = new int[100];
	int top;	// top of stack
	int bp;		// base pointer

	public CodeGenerator() {
		pc = 1; progStart = -1;
	}

	//----- code generation methods -----
	
	public void Put(int x) {
		code[pc++] = (byte)x;
	}
	
	public void Emit (Op op) {
		Put((int)op);
	}

	public void Emit (Op op, int val) {
		Emit(op); Put(val>>8); Put(val);
	}

	public void Patch (int adr, int val) {
		code[adr] = (byte)(val>>8); code[adr+1] = (byte)val;
	}

	public void Decode() {
		int maxPc = pc;
		pc = 1;
		while (pc < maxPc) {
			Op code = (Op)Next();
				Console.Write("{0,3}: {1} ", pc - 1, opcode[(int)code]);
				switch (code) {
				case Op.LOAD: case Op.LOADG: case Op.CONST: case Op.STO: case Op.STOG: 
				case Op.CALL: case Op.ENTER: case Op.JMP: case Op.FJMP:
                Console.WriteLine(Next2()); break;
				case Op.ADD: case Op.SUB: case Op.MUL: case Op.DIV: case Op.NEG:
				case Op.EQU: case Op.LSS: case Op.GTR: case Op.RET: case Op.LEAVE: 
				case Op.WRITE:
						break;  Console.WriteLine(); break;
			}
		}
	}

	//----- interpreter methods -----
	
	int Next () {
		return code[pc++];
	}

	int Next2 () {
		int x,y; 
		x = (sbyte)code[pc++]; y = code[pc++];
		return (x << 8) + y;
	}

	int Int (bool b) {
		if (b) return 1; else return 0;
	}

	void Push (int val) {
		stack[top++] = val;
	}

	int Pop() {
		return stack[--top];
	}

	int ReadInt(FileStream s) {
		int ch, sign;
		do {ch = s.ReadByte();} while (!(ch >= '0' && ch <= '9' || ch == '-'));
		if (ch == '-') {sign = -1; ch = s.ReadByte();} else sign = 1;
		int n = 0;
		while (ch >= '0' && ch <= '9') {
			n = 10 * n + (ch - '0');
			ch = s.ReadByte();
		}
		return n * sign;
	}
	
	public void Interpret () { 
		int val;
		try {
			//FileStream s = new FileStream(data, FileMode.Open);
				Console.WriteLine();
				pc = progStart; stack[0] = 0; top = 1; bp = 0;
			for (;;) {
				switch ((Op)Next()) {
					case Op.CONST: Push(Next2()); break;
					case Op.LOAD:  Push(stack[bp+Next2()]); break;
					case Op.LOADG: Push(globals[Next2()]); break;
					case Op.STO:   stack[bp+Next2()] = Pop(); break;
					case Op.STOG:  globals[Next2()] = Pop(); break;
					case Op.ADD:   Push(Pop()+Pop()); break;
					case Op.SUB:   Push(-Pop()+Pop()); break;
					case Op.DIV:   val = Pop(); Push(Pop()/val); break;
					case Op.MUL:   Push(Pop()*Pop()); break;
					case Op.NEG:   Push(-Pop()); break;
					case Op.EQU:   Push(Int(Pop()==Pop())); break;
					case Op.LSS:   Push(Int(Pop()>Pop())); break;
					case Op.GTR:   Push(Int(Pop()<Pop())); break;
					case Op.JMP:   pc = Next2(); break;
					case Op.FJMP:  val = Next2(); if (Pop()==0) pc = val; break;
					case Op.WRITE: Console.WriteLine(Pop()); break;
					case Op.CALL:  Push(pc+2); pc = Next2(); break;
					case Op.RET:   pc = Pop(); if (pc == 0) return; break;
					case Op.ENTER: Push(bp); bp = top; top = top + Next2(); break;
					case Op.LEAVE: top = bp; bp = Pop(); break;
					case Op.TECLADO: val = Convert.ToInt32(Console.ReadLine()); Push(val); break;
                        default:    throw new Exception("illegal opcode");
				}
			}
		} catch (IOException) {
			Console.WriteLine("--- Error accessing file {0}");
			System.Environment.Exit(0);
		}
	}

} // end CodeGenerator

} // end namespace