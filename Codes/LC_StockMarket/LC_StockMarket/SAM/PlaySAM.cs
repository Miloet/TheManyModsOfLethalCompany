using System.Collections.Generic;
using UnityEngine;
using SAM;
using LC_StockMarketIndex;


namespace SAM
{
	public static class PlaySAM
	{
		public static void SayString(string s, AudioSource source)
		{
			if (!StockMarketIndexMod.voice.Value) return;


			StopAudio(source);

			// HACK!
			s = s + ".";        // TODO: my C# port seems to crash without final punctuation.

			string output = null;

			int[] ints = null;

			bool phonetic = false;
			if (phonetic)
			{
				ints = UnitySAM.IntArray(s);

				var L = new List<int>(ints);
				L.Add(155);
				ints = L.ToArray();

				output = s + "\0x9b";
			}
			else
			{
				output = UnitySAM.TextToPhonemes(s + "[", out ints);
			}

			UnitySAM.SetInput(ints);

			var buf = UnitySAM.SAMMain();
			if (buf == null)
			{
				Debug.LogError("Buffer was null");
			}
			else
			{
				Debug.Log("Buffer size is " + buf.GetSize());
			}

			AudioClip ac = AudioClip.Create("Hello", buf.GetSize(), 1, 22050, false);
			ac.SetData(buf.GetFloats(), 0);
			source.PlayOneShot(ac);
		}

		public static void StopAudio(AudioSource source)
		{
			source.Stop();
		}
	}
}