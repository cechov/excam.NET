using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DeleBil.Droid.Service;
using static Android.Gms.Maps.GoogleMap;

namespace DeleBil.Droid
{
    [Activity(Label = "Lån bil")]
    public class setRentedActivity : Activity, IOnMapReadyCallback
    {
        private GoogleMap mMap;
        private string RegNr, CarName;
        private double Latitude, Longtitude;
        private Button Confirm;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.setRented);

            Lease lease = new Lease();
            RegNr = Intent.GetStringExtra("findCarInput") ?? "Data not available";
            await Task.Run(async () =>
            {
                lease = await new CarService().GetLease(RegNr);
            });

            Latitude = lease.latitudePickUpLocation;
            Longtitude = lease.longtitudePickUpLocation;
            CarName = lease.CarName;
            FindViewById<TextView>(Resource.Id.setRented_CarName).Text = CarName;
            FindViewById<ImageView>(Resource.Id.setRented_Image).SetImageBitmap(convertByteToImage(lease.CarPicture.PictureData));
            Confirm = FindViewById<Button>(Resource.Id.setRented_Confirm);
            Confirm.Click += delegate { ConfirmDialog(); };
            
            SetUpMap();
        }

        public Bitmap convertByteToImage(byte[] array)
        {
            return BitmapFactory.DecodeByteArray(array, 0, array.Length);
        }

        #region Update lease
        private void ConfirmDialog()
        {
            AlertDialog.Builder mBuilder = new AlertDialog.Builder(this);
            AlertDialog alert = mBuilder.Create();
            alert.SetTitle("Lån bil");
            alert.SetIcon(Resource.Drawable.logo);
            alert.SetMessage("Vennligst bekreft lån av bil.");
            alert.SetButton("Bekreft", (c, ev) =>
            {
                UpdateLease();
                Toast.MakeText(this, "Du er nå registrert som låner av " + RegNr + ".", ToastLength.Long).Show();
                Finish();
            });
            alert.SetButton2("Avbryt", (c, ev) =>
            {
                Toast.MakeText(this, "Lån av bil er avbrutt.", ToastLength.Long).Show();
            });
            alert.Show();
        }

        private async void UpdateLease()
        {
            int leaseId;
            
            await Task.Run(async () =>
            {
                leaseId = await new CarService().PutLease(RegNr, 0, 0, null);
            });
        }
        #endregion

        #region Map
        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.setRented_Map).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;

            LatLng latlng = new LatLng(Latitude, Longtitude);

            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
            mMap.MoveCamera(camera);

            MarkerOptions options = new MarkerOptions()
                .SetPosition(latlng)
                .SetTitle(RegNr)
                .SetSnippet(CarName);

            mMap.AddMarker(options);
        }
        #endregion
    }
}