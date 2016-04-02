using Android.App;
using Android.Widget;
using Android.OS;
using LiarsDice;
using System;
using Android.Views;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Media;
using Android.Content.PM;

namespace BullshitDice
{
	[Activity (Label = "Bullshit dice", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity, IObserver<Die>
	{
		#region properties
		private LiarsDiceApp app;
		private bool hidden = false;
		private MediaPlayer _player;
		#endregion

		protected async override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			if (this.app == null) {
				this.app = new LiarsDiceApp();
				this.app.Subscribe(this);
			}

			/*var _imageView = FindViewById<ImageButton>(Resource.Id.Die1);
			BitmapFactory.Options options = await GetBitmapOptionsOfImageAsync();
			Bitmap bitmapToDisplay = await LoadScaledDownBitmapForDisplayAsync (Resources, options, 130, 130);
			RunOnUiThread (delegate {
				_imageView.SetImageBitmap (bitmapToDisplay);
			});*/

			// Get our button from the layout resource,
			// and attach an event to it
			Button rollBtn = FindViewById<Button> (Resource.Id.RollDiceBtn);
			rollBtn.Click += delegate {
				rollBtn.Text = "Rolling";
				rollBtn.Enabled = false;
				this.PlaySound(Resource.Raw.DICE);
				this.app.RollDice();
			};

			// Get our remove button and attach an event to it
			FindViewById<ImageButton> (Resource.Id.RemoveDieBtn).Click += delegate {
				this.PlaySound(Resource.Raw.gasps);
				this.app.RemoveDie();
			};

			FindViewById<Button>(Resource.Id.ResetBtn).Click += delegate(object sender, EventArgs e) {
				this.app = new LiarsDiceApp();
				this.app.Subscribe(this);
				for(int i = 0; i < 5; i++)
					this.RenderDie(this.FindDie(i+1), 1);
			};

			FindViewById<ImageButton>(Resource.Id.CheatBtn).Click += delegate {
				this.app.ToggleCheatmode();
				Toast.MakeText(this, "Cheatmode toggled!", ToastLength.Short).Show();
			};

			FindViewById<Button>(Resource.Id.HideToggleBtn).Click += delegate {
				this.hidden = !this.hidden;
				this.PlaySound(Resource.Raw.curtain);
				if(hidden){
					for(int i = 0; i < 5; i++)
						this.RenderDie(this.FindDie(i+1), 0);
				} else {
					app.RefreshDice();
				}
			};
		}

		#region observer pattern
		public virtual void OnNext(Die die){
			RunOnUiThread (delegate {
				ImageButton dieToBeUpdated = this.FindDie (die.DieNr);
				this.RenderDie (dieToBeUpdated, die.Value);
			});
		}

		public virtual void OnError(Exception e){
			Toast.MakeText(this, e.Message, ToastLength.Long).Show();
		}

		public virtual void OnCompleted()
		{
			RunOnUiThread (delegate {
				Button rollButton = FindViewById<Button> (Resource.Id.RollDiceBtn);
				rollButton.Enabled = true;
				rollButton.SetText (Resource.String.roll);
			});
		}
		#endregion

		#region private methods
		private void PlaySound(int res){
			_player = MediaPlayer.Create(this, res);
			_player.Start();
		}

		private ImageButton FindDie(int dieNr){
			switch (dieNr){
			case 1:
				return FindViewById<ImageButton> (Resource.Id.Die1);
			case 2:
				return FindViewById<ImageButton> (Resource.Id.Die2);
			case 3:
				return FindViewById<ImageButton> (Resource.Id.Die3);
			case 4:
				return FindViewById<ImageButton> (Resource.Id.Die4);
			case 5:
				return FindViewById<ImageButton> (Resource.Id.Die5);
			default:
				throw new IndexOutOfRangeException("Die number not recognised:"+dieNr+"! Please supply a dieNr from 1 to 5.");
			}
		}

		private void RenderDie(ImageButton button, int value){
			if(value > 0)
				button.Visibility = ViewStates.Visible;
			switch (value) {
			case -1:
				button.Visibility = ViewStates.Gone;
				break;
			case 0:
				button.SetImageResource (Resource.Drawable.unknown);
				break;
			case 1:
				button.SetImageResource (Resource.Drawable.One_side);
				break;
			case 2:
				button.SetImageResource (Resource.Drawable.Two_side);
				break;
			case 3:
				button.SetImageResource (Resource.Drawable.Three_side);
				break;
			case 4:
				button.SetImageResource (Resource.Drawable.Four_side);
				break;
			case 5:
				button.SetImageResource (Resource.Drawable.Five_side);
				break;
			case 6:
				button.SetImageResource (Resource.Drawable.Six_side);
				break;
			default:
				throw new IndexOutOfRangeException("Die value not recognised:"+value+"! Please supply a value from 1 to 6.");
			}
		}
		#endregion

		/*#region private scale methods
		private async Task<BitmapFactory.Options> GetBitmapOptionsOfImageAsync()
		{
			BitmapFactory.Options options = new BitmapFactory.Options
			{
				InJustDecodeBounds = true
			};

			// The result will be null because InJustDecodeBounds == true.
			Bitmap result=  await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.One_side, options);

			int imageHeight = options.OutHeight;
			int imageWidth = options.OutWidth;

			string debug = string.Format("Original Size= {0}x{1}", imageWidth, imageHeight);

			return options;
		}

		private async Task<Bitmap> LoadScaledDownBitmapForDisplayAsync(Resources res, BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Calculate inSampleSize
			options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

			// Decode bitmap with inSampleSize set
			options.InJustDecodeBounds = false;

			return await BitmapFactory.DecodeResourceAsync(res, Resource.Drawable.One_side, options);
		}

		public int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Raw height and width of image
			float height = options.OutHeight;
			float width = options.OutWidth;
			double inSampleSize = 1D;

			if (height*2 > reqHeight || width*3 > reqWidth)
			{
				int halfHeight = (int)(height / 2);
				int halfWidth = (int)(width / 2);

				// Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
				while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
				{
					inSampleSize *= 2;
				}
			}

			return (int)inSampleSize;
		}
		#endregion*/
	}
}
