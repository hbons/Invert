using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;


namespace Invert
{
	[Activity (Label = "Invert", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, TextureView.ISurfaceTextureListener
	{
		Camera _camera;
		TextureView _textureView;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			RequestWindowFeature (WindowFeatures.NoTitle);

			_textureView = new TextureView (this);
			_textureView.SurfaceTextureListener = this;
			_textureView.Touch += OnTouch;

			SetContentView (_textureView);
		}


		public void OnSurfaceTextureAvailable (Android.Graphics.SurfaceTexture surface, int width, int height)
		{
			_camera = Camera.Open ();
			ConfigureCamera ();

			Preview preview = new Preview ();
			preview.OnFrame += OnPreviewFrame;
			_camera.SetPreviewCallback (preview);

			_textureView.LayoutParameters = new FrameLayout.LayoutParams (width, height);

			try {
				_camera.SetPreviewTexture (surface);
				_camera.StartPreview ();
			
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}


		void ConfigureCamera ()
		{
			Camera.Parameters parameters = _camera.GetParameters ();

			parameters.ColorEffect  = Camera.Parameters.EffectNegative;
			parameters.WhiteBalance = Camera.Parameters.WhiteBalanceAuto;
			parameters.FocusMode    = Camera.Parameters.FocusModeContinuousPicture;

			if (parameters.IsVideoStabilizationSupported)
				parameters.VideoStabilization = true;

			// Negatives are often held to a light source like a bulb or screen,
			// so increase the exposure compensation for better contrast.
		    // parameters.ExposureCompensation = parameters.MaxExposureCompensation;

			if (parameters.IsZoomSupported) {
				if (parameters.MaxZoom >= 20)
					parameters.Zoom = 20;
				else
					parameters.Zoom = parameters.MaxZoom;
			}

			_camera.SetDisplayOrientation (90);
			_camera.SetParameters (parameters);
		}


		void OnPreviewFrame (byte [] data) {
		/*
			new Thread (() => {
				string yuvFrame = "Frame: ";

				foreach (byte b in data)
					yuvFrame += "" + b + " ";

				Console.WriteLine (yuvFrame);
			
			}).Start ();
		*/
		}


		protected override void OnPause ()
		{
			if (_camera != null) {
				_camera.StopPreview ();
				_camera.SetPreviewCallback (null);
				_camera.Release ();

				_camera = null;
			}

			base.OnPause ();
		}


		void OnTouch (object sender, View.TouchEventArgs args)
		{
			if (_camera != null)
				_camera.AutoFocus (null);
		}


		public bool OnSurfaceTextureDestroyed (Android.Graphics.SurfaceTexture surface)
		{
			return true;
		}


		public void OnSurfaceTextureSizeChanged (Android.Graphics.SurfaceTexture surface, int width, int height)
		{
		}


		public void OnSurfaceTextureUpdated (Android.Graphics.SurfaceTexture surface)
		{
		}
	}


	public class Preview : Java.Lang.Object, Camera.IPreviewCallback
	{
		public delegate void OnFrameEventHandler (byte[] data);
		public event OnFrameEventHandler OnFrame;


		public void OnPreviewFrame (byte [] data, Camera camera) {
			if (OnFrame != null)
				OnFrame (data);
		}
	}
}
