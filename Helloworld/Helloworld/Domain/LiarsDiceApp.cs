using System;
using System.Timers;
using System.Collections.Generic;

namespace LiarsDice
{
	public class LiarsDiceApp : IObservable<Die>
	{
		private Timer timer = new Timer(1000);
		private int timerInterval = 150;
		private int time = 0;

		private IList<Die> dice = new List<Die>();
		private IList<IObserver<Die>> observers = new List<IObserver<Die>>();

		#region ctor
		public LiarsDiceApp ()
		{
			// Add dice
			for(int i = 0; i < 5; i++){ 
				dice.Add(new Die(1, i+1));
			}

			// Create a timer
			this.timer = new System.Timers.Timer(timerInterval);
			// Hook up the Elapsed event for the timer. 
			this.timer.Elapsed += new ElapsedEventHandler(this.Reroll);
			this.timer.AutoReset = true;
		}
		#endregion ctor

		#region public methods
		public void RollDice()
		{
			if (!timer.Enabled) {
				this.timer.Enabled = true;
			} else {
				//timer enabled, no need to fire it again
			}
		}

		public int GetTime()
		{
			return time;
		}

		public void ToggleCheatmode(){
			Die.ToggleCheatmode();
		}

		public void RemoveDie(){
			if (this.dice.Count <= 0) {
				foreach (var observer in observers)
					observer.OnError (new ApplicationException ("There are no dice left! New game?"));
			} else {
				Die dieToRemove = this.dice [dice.Count - 1];
				this.dice.Remove (dieToRemove);
				dieToRemove.Value = -1; //-1 convention for invalid
				this.Notify (dieToRemove);
			}
		}

		public void RefreshDice(){
			foreach(var die in this.dice){
				this.Notify(die);
			}
		}
		#endregion

		private void Reroll(Object source, ElapsedEventArgs e)
		{
			this.time += timerInterval; //add rolltime tracker
			if (time <= timerInterval * 5) { //if is 5th reroll or fewer
				foreach (Die d in dice) {
					d.Roll ();
					this.Notify (d);
				}
			} else {
				this.time = 0;
				this.timer.Enabled = false;
				foreach (var observer in observers)
					observer.OnCompleted();
			}
		}

		#region observer methods
		public IDisposable Subscribe(IObserver<Die> observer){
			if (!this.observers.Contains (observer)) {
				observers.Add (observer);
				foreach (var die in dice)
					observer.OnNext (die);
			}
			return new Unsubscriber<Die> (observers, observer);
		}

		private void Notify(Die die){
			foreach (var observer in observers)
				observer.OnNext (die);
		}
		#endregion
	}

	#region observer pattern
	internal class Unsubscriber<Die> : IDisposable
	{
		private IList<IObserver<Die>> _observers;
		private IObserver<Die> _observer;

		internal Unsubscriber(IList<IObserver<Die>> observers, IObserver<Die> observer){
			this._observers = observers;
			this._observer = observer;
		}

		public void Dispose()
		{
			if (this._observers.Contains (this._observer))
				this._observers.Remove (this._observer);
		}
	}
	#endregion
}