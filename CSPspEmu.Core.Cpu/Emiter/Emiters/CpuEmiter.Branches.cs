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

		// Code executed after the delayed slot.
		public void _branch_post(SafeLabel Label)
		{
			if (this.AndLink)
			{
				var SkipBranch = SafeILGenerator.DefineLabel("SkipBranch");
				MipsMethodEmiter.LoadBranchFlag();
				SafeILGenerator.BranchIfFalse(SkipBranch);
				{
					MipsMethodEmiter.SaveGPR(31, () =>
					{
						SafeILGenerator.Push((int)(BranchPC + 8));
					});

					SafeILGenerator.BranchAlways(Label);
				}
				SkipBranch.Mark();
			}
			else
			{
				MipsMethodEmiter.LoadBranchFlag();
				SafeILGenerator.BranchIfTrue(Label);
			}
		}

		bool AndLink = false;
		uint BranchPC = 0;

		public void _branch_pre(bool AndLink = false)
		{
			this.AndLink = AndLink;
			this.BranchPC = PC;
		}

		// Branch on EQuals (Likely).
		public void beq() { _branch_pre(); _branch_pre_vv(SafeBinaryComparison.Equals); }
		public void beql() { beq(); }

		// Branch on Not Equals (Likely).
		public void bne() { _branch_pre(); _branch_pre_vv(SafeBinaryComparison.NotEquals); }
		public void bnel() { bne(); }

		// Branch on Less Than Zero (And Link) (Likely).
		public void bltz() { _branch_pre(); _branch_pre_v0(SafeBinaryComparison.LessThanSigned); }
		public void bltzl() { bltz(); }

		[PspUntested]
		public void bltzal() { _branch_pre(AndLink: true); _branch_pre_v0(SafeBinaryComparison.LessThanSigned); }
		public void bltzall() { bltzal(); }

		// Branch on Less Or Equals than Zero (Likely).
		public void blez() { _branch_pre(); _branch_pre_v0(SafeBinaryComparison.LessOrEqualSigned); }
		public void blezl() { blez(); }

		// Branch on Great Than Zero (Likely).
		public void bgtz() { _branch_pre();  _branch_pre_v0(SafeBinaryComparison.GreaterThanSigned); }
		public void bgtzl() { bgtz(); }

		// Branch on Greater Equal Zero (And Link) (Likely).
		public void bgez() { _branch_pre();  _branch_pre_v0(SafeBinaryComparison.GreaterOrEqualSigned); }
		public void bgezl() { bgez(); }
		[PspUntested]
		public void bgezal() {
			_branch_pre(AndLink: true); _branch_pre_v0(SafeBinaryComparison.GreaterOrEqualSigned); 
		}
		public void bgezall() { bgezal(); }
		
#if true
		private uint GetJumpAddress()
		{
			return Instruction.GetJumpAddress(PC);
		}
#else
		private uint GetJumpAddress()
		{
			//Console.WriteLine("Instruction.JUMP: {0:X}", Instruction.JUMP);
			return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
		}
#endif

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
				AnalyzePCEvent(PC + 8);
			});
		}

		/// <summary>
		/// Jump
		/// </summary>
		public void j()
		{
			//Console.WriteLine("JUMP_ADDR: {0:X}", GetJumpAddress());
			MipsMethodEmiter.SavePC(() =>
			{
				SafeILGenerator.Push((int)GetJumpAddress());
				AddPcToAnalyze(GetJumpAddress());
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

		/// <summary>
		/// Jump Register
		/// </summary>
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

		public event Action<uint> AnalyzePCEvent;
		private void AddPcToAnalyze(uint PC)
		{
			if (AnalyzePCEvent != null) AnalyzePCEvent(PC);
		}

		/// <summary>
		/// Jump And Link Register
		/// </summary>
		public void jalr()
		{
			_link();
			jr();
		}

		/// <summary>
		/// Jump And Link
		/// </summary>
		public void jal()
		{
			_link();
			j();
		}

		/// <summary>
		/// Branch on C1 False
		/// </summary>
		public void bc1f()
		{
			// Branch on C1 False/True (Likely).
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
				SafeILGenerator.Push((int)0);
				SafeILGenerator.CompareBinary(SafeBinaryComparison.Equals);
			});
		}
		/// <summary>
		/// Branch on C1 True
		/// </summary>
		public void bc1t()
		{
			MipsMethodEmiter.StoreBranchFlag(() =>
			{
				MipsMethodEmiter.LoadFCR31_CC();
				SafeILGenerator.Push((int)0);
				SafeILGenerator.CompareBinary(SafeBinaryComparison.NotEquals);
			});
		}

		/// <summary>
		/// Branch on C1 False (Likely)
		/// </summary>
		public void bc1fl()
		{
			bc1f();
		}

		/// <summary>
		/// Branch on C1 True (Likely)
		/// </summary>
		public void bc1tl()
		{
			bc1t();
		}
	}
}
