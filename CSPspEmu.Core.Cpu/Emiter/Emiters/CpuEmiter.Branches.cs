﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using CSPspEmu.Core.Memory;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		public SafeILGeneratorEx SafeILGenerator
		{
			get
			{
				return MipsMethodEmiter.SafeILGenerator;
			}
		}

		// Code executed after the delayed slot.
		public void _branch_post(SafeLabel Label)
		{
			MipsMethodEmiter.LoadBranchFlag();
			SafeILGenerator.BranchIfTrue(Label);
		}

		public void _branch_likely(Action Action)
		{
			var NullifyDelayedLabel = SafeILGenerator.DefineLabel("NullifyDelayedLabel");
			MipsMethodEmiter.LoadBranchFlag();
			SafeILGenerator.BranchIfFalse(NullifyDelayedLabel);
			{
				Action();
			}
			NullifyDelayedLabel.Mark();
		}

		private void _branch_pre_vv(SafeBinaryComparison Comparison)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RS);
				MipsMethodEmiter.LoadGPR_Signed(RT);
				SafeILGenerator.CompareBinary(Comparison);
			});
		}

		private void _branch_pre_v0(SafeBinaryComparison Comparison)
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RS);
				SafeILGenerator.Push((int)0);
				SafeILGenerator.CompareBinary(Comparison);
			});
		}

		// Branch on EQuals (Likely).
		public void beq() { _branch_pre_vv(SafeBinaryComparison.Equals); }
		public void beql() { beq(); }

		// Branch on Not Equals (Likely).
		public void bne() { _branch_pre_vv(SafeBinaryComparison.NotEquals); }
		public void bnel() { bne(); }

		// Branch on Less Than Zero (And Link) (Likely).
		public void bltz() { _branch_pre_v0(SafeBinaryComparison.LessThanSigned); }
		public void bltzl() { bltz(); }
		public void bltzal() {
			//_branch_pre_v0_link(OpCodes.Clt);
			//_link();
			//bltz();
			throw (new NotImplementedException());
		}
		public void bltzall() { bltzal(); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { _branch_pre_v0(SafeBinaryComparison.LessOrEqualSigned); }
		public void blezl() { blez(); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { _branch_pre_v0(SafeBinaryComparison.GreaterThanSigned); }
		public void bgtzl() { bgtz(); }

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { _branch_pre_v0(SafeBinaryComparison.GreaterOrEqualSigned); }
		public void bgezl() { bgez(); }
		public void bgezal() { throw (new NotImplementedException()); }
		public void bgezall() { bgezal(); }

		private uint GetJumpAddress()
		{
			//Console.WriteLine("Instruction.JUMP: {0:X}", Instruction.JUMP);
			return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
		}

		public bool PopulateCallStack
		{
			get
			{
				return !(CpuProcessor.Memory is FastPspMemory) && CpuProcessor.PspConfig.TrackCallStack;
			}
		}

		private void _link()
		{
			//Console.WriteLine("LINK: {0:X}", PC);
			if (PopulateCallStack)
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				SafeILGenerator.Push((int)PC);
				SafeILGenerator.Call((Action<CpuThreadState, uint>)CpuThreadState.CallStackPush);
			}
			MipsMethodEmiter.SaveGPR(31, () =>
			{
				SafeILGenerator.Push((int)(PC + 8));
			});
		}

		// Jump (And Link) (Register).
		public void j()
		{
			//Console.WriteLine("JUMP_ADDR: {0:X}", GetJumpAddress());
			MipsMethodEmiter.SavePC(() =>
			{
				SafeILGenerator.Push((int)GetJumpAddress());
			});

			//Console.WriteLine("aaaaaaaaaaaaaa");

			if (CpuProcessor.PspConfig.TraceJal)
			{
				SafeILGenerator.EmitWriteLine(String.Format("{0:X} : JAL 0x{0:X}", PC, GetJumpAddress()));
			}

			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Jmp);
			//SafeILGenerator.LoadArgument0CpuThreadState()
			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldobj, (object)CpuProcessor.CreateAndCacheDelegateForPC(MemoryStream, GetJumpAddress()));
			//var FieldBuilder = MipsMethodEmiter.TypeBuilder.DefineField("testField", typeof(int), FieldAttributes.Static);
			//FieldBuilder.SetValue(null, CpuProcessor.CreateAndCacheDelegateForPC(MemoryStream, GetJumpAddress()));
			//FieldBuilder.SetValue(null, 1);
			
			//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Callvirt);

			SafeILGenerator.Return();
		}
		public void jr() 
		{
			// RETURN
			if (RS == 31)
			{
				if (PopulateCallStack)
				{
					SafeILGenerator.LoadArgument0CpuThreadState();
					SafeILGenerator.Call((Action<CpuThreadState>)CpuThreadState.CallStackPop);
				}
			}

			MipsMethodEmiter.SavePC(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
			});
			SafeILGenerator.Return();
		}

		public void jalr()
		{
			_link();
			jr();
		}

		public void jal()
		{
			_link();
			j();
		}

		// Branch on C1 False/True (Likely).
		public void bc1f()
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
				SafeILGenerator.Push((int)0);
				SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals);
			});
		}
		public void bc1t() {
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
			});
		}
		public void bc1fl() { bc1f(); }
		public void bc1tl() { bc1t(); }
	}
}
