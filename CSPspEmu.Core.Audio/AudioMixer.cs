﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Audio
{
	public class AudioMixer
	{
		/*
		static public short[] Convert(short[] Input, int InputRate, int InputChannels, int OutputRate, int OutputChannels)
		{
			var Output = new short[(Input.Length * InputRate * InputChannels) / (OutputRate * OutputChannels)];
			if (InputRate != OutputRate)
		}
		*/

		static public StereoShortSoundSample[] Convert_Mono22050_Stereo44100(short[] Input)
		{
			var Output = new StereoShortSoundSample[Input.Length * 2];
			int m = 0;
			for (int n = 0; n < Input.Length; n++)
			{
				//Input[n]
				short Sample1 = Input[n];
				Output[m++].MonoLeftRight = Sample1;
				//Output[m++] = Sample1;
				if (n < Input.Length - 1)
				{
					short Sample2 = Input[n + 1];
					Output[m++].MonoLeftRight = (short)((Sample1 + Sample2) / 2);
					//Output[m++] = (short)((Sample1 + Sample2) / 2);
				}
				else
				{
				}
			}
			return Output;
		}
	}
}
