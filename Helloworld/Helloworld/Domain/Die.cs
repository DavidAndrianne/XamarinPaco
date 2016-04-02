using System;

namespace LiarsDice
{
	public class Die
	{
		private static Random random = new Random();
		private static bool cheatMode = false;

		#region properties
		public int DieNr { get; set; }
		public const int MaxValue = 6;
		public int Value { get; set;}
		#endregion properties

		#region constructors
		public Die (){this.Value = 1; this.DieNr = 1;}
		public Die(int i, int number){this.Value = i; this.DieNr = number;}
		#endregion

		#region public methods
		public void Roll(){
			if (!cheatMode) {
				Value = Die.random.Next (1, MaxValue + 1);
			} else {
				Value = new Random().Next (1, MaxValue + 1);
			}
		}

		public static void SetCheatmode(bool flag){
			Die.cheatMode = flag;
		}

		public static void ToggleCheatmode(){
			Die.cheatMode = !Die.cheatMode;
		}
		#endregion
	}
}