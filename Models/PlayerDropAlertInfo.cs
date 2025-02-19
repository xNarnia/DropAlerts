using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.ComponentModel;
using static DropAlerts.DropAlerts;

namespace DropAlerts.Models
{
	public class PlayerDropAlertInfo
	{
		// We process the rarity toggles as a bit array to save space in the JSON storage
		/// <summary>
		/// Drop alert rarity levels stored as bits in an int.
		/// </summary>
		[JsonProperty("d")]
		[DefaultValue(15)]
		public int DropAlertByte { get; set; }

		/// <summary>
		/// Returns the toggle status of the specified drop alert rarity.<br/>Drop alert starts at rarity 1.
		/// </summary>
		public bool GetDropAlert(int rarityLevel)
		{
			--rarityLevel;
			if (rarityLevel > 4)
				ThrowRarityError();

			BitArray RarityBitArray = GetBitArray();
			return RarityBitArray[rarityLevel];
		}

		/// <summary>
		/// Sets the specified rarity alert to enabled/disabled.<br/>Drop alert starts at rarity 1.
		/// </summary>
		public void SetDropAlert(int rarityLevel, bool enabled)
		{
			--rarityLevel;
			if (rarityLevel > 4)
				ThrowRarityError();

			int mask = 1 << rarityLevel;
			if (enabled)
				DropAlertByte |= mask;
			else
				DropAlertByte &= ~mask;
		}
		
		/// <summary>
		/// Returns whether the specified drop rate alert is disabled by the player's config options.
		/// </summary>
		public bool AlertForDropRateIsDisabled(float dropRate)
		{
			if (dropRate <= DAMod.Config.Percent4Star)
				return !GetDropAlert(4);
			if (dropRate <= DAMod.Config.Percent3Star)
				return !GetDropAlert(3);
			if (dropRate <= DAMod.Config.Percent2Star)
				return !GetDropAlert(2);
			if (dropRate <= DAMod.Config.Percent1Star)
				return !GetDropAlert(1);
			else
				return true;
		}

		/// <summary>
		/// Returns the drop rate trigger threshold for the specified drop alert rarity level.<br/>Drop alert starts at rarity 1.
		/// </summary>
		public float GetDropRateFor(int rarityLevel)
		{
			--rarityLevel;
			if (rarityLevel > 4)
				ThrowRarityError();

			switch (rarityLevel)
			{
				case 0:
					return DAMod.Config.Percent1Star;
				case 1:
					return DAMod.Config.Percent2Star;
				case 2:
					return DAMod.Config.Percent3Star;
				case 3:
					return DAMod.Config.Percent4Star;
				default:
					return 0;
			}
		}

		/// <summary>
		/// The number of rarities included in this object.
		/// </summary>
		public int Count() => 4;

		private void ThrowRarityError() => throw new ArgumentException("rarityLevel must not be greater than 4");

		private BitArray GetBitArray() 
		{
			BitArray bitArray = new BitArray(Count());
			for (int i = 0; i < Count(); i++)
			{
				bitArray[i] = (DropAlertByte & (1 << i)) != 0;
			}
			return bitArray;
		}
	}
}
