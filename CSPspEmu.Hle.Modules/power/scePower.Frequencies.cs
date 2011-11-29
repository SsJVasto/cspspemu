﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.power
{
	unsafe public partial class scePower
	{
		/// <summary>
		/// Set CPU Frequency
		/// </summary>
		/// <param name="CpuFrequency">new CPU frequency, valid values are 1 - 333</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x843FBF43, FirmwareVersion = 150)]
		public int scePowerSetCpuClockFrequency(int CpuFrequency)
		{
			HleState.PspConfig.CpuFrequency = CpuFrequency;
			return 0;
		}

		/// <summary>
		/// Get CPU Frequency as Integer
		/// </summary>
		/// <returns>frequency as int</returns>
		[HlePspFunction(NID = 0xFDB5BFE9, FirmwareVersion = 150)]
		public int scePowerGetCpuClockFrequencyInt()
		{
			return HleState.PspConfig.CpuFrequency;
		}

		/// <summary>
		/// Get Bus fequency as Integer
		/// </summary>
		/// <returns>frequency as int</returns>
		[HlePspFunction(NID = 0xBD681969, FirmwareVersion = 150)]
		public int scePowerGetBusClockFrequencyInt()
		{
			return HleState.PspConfig.BusFrequency;
		}

		/// <summary>
		/// Get CPU Frequency as Float
		/// </summary>
		/// <returns>frequency as float</returns>
		[HlePspFunction(NID = 0xB1A52C83, FirmwareVersion = 150)]
		public float scePowerGetCpuClockFrequencyFloat()
		{
			return (float)scePowerGetCpuClockFrequency();
		}

		/// <summary>
		/// Get Bus frequency as Float
		/// </summary>
		/// <returns>frequency as float</returns>
		[HlePspFunction(NID = 0x9BADB3EB, FirmwareVersion = 150)]
		public float scePowerGetBusClockFrequencyFloat()
		{
			return (float)scePowerGetBusClockFrequency();
		}

		/// <summary>
		/// Alias for scePowerGetCpuClockFrequencyInt
		/// </summary>
		/// <returns>frequency as int</returns>
		[HlePspFunction(NID = 0xFEE03A2F, FirmwareVersion = 150)]
		public int scePowerGetCpuClockFrequency()
		{
			return HleState.PspConfig.CpuFrequency;
		}

		/// <summary>
		/// Alias for scePowerGetBusClockFrequencyInt
		/// </summary>
		/// <returns>frequency as int</returns>
		[HlePspFunction(NID = 0x478FE6F5, FirmwareVersion = 150)]
		public int scePowerGetBusClockFrequency()
		{
			return HleState.PspConfig.BusFrequency;
		}

		/// <summary>
		/// Set Clock Frequencies
		/// cpufreq <= pllfreq
		/// busfreq*2 <= pllfreq
		/// </summary>
		/// <param name="PllFrequency">pll frequency, valid from 19-333</param>
		/// <param name="CpuFrequency">cpu frequency, valid from 1-333</param>
		/// <param name="BusFrequency">bus frequency, valid from 1-167</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x737486F2, FirmwareVersion = 150)]
		//mixin(registerFunction!(0xEBD177D6, scePowerSetClockFrequency));
		public int scePowerSetClockFrequency(int PllFrequency, int CpuFrequency, int BusFrequency)
		{
			HleState.PspConfig.PllFrequency = PllFrequency;
			HleState.PspConfig.CpuFrequency = CpuFrequency;
			HleState.PspConfig.BusFrequency = BusFrequency;

			return 0;
		}

		/// <summary>
		/// Set Bus Frequency
		/// </summary>
		/// <param name="BusFrequency">new BUS frequency, valid values are 1 - 167</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB8D7B3FB, FirmwareVersion = 150)]
		public int scePowerSetBusClockFrequency(int BusFrequency)
		{
			HleState.PspConfig.BusFrequency = BusFrequency;
			return 0;
		}
	}
}