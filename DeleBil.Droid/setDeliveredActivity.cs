using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Widget;
using DeleBil.Droid.Service;
using static Android.Gms.Common.Apis.GoogleApiClient;

namespace DeleBil.Droid
{
    [Activity(Label = "Lever tilbake bil")]
    public class setDeliveredActivity : Activity, IOnMapReadyCallback, IConnectionCallbacks, IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        private const int SelectImageRequest = 67676;
        private const int MY_PERMISSION_REQUEST_CODE = 7171;
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 7172;
        private TextView txtCoordinates;
        private Button btnConfirm;
        private ImageButton btnGetCoordinates, btnUpload;
        private bool mRequestinglocationUpdates = false;
        private LocationRequest mLocationRequest;
        private GoogleApiClient mGoogleApiClient;
        private Location mLastLocation;
        private GoogleMap mMap;
        private string RegNr;
        private ImageView imageView;
        private IList<Picture> pictures = new List<Picture>(3);
        private Android.Net.Uri imageUri;
        private int imageCount = 0;

        private static int UPDATE_INTERVAL = 5000; // SEC
        private static int FATEST_INTERVAL = 3000; // SEC
        private static int DISPLACEMENT = 10; // METERS

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case MY_PERMISSION_REQUEST_CODE:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (CheckPlayServices())
                        {
                            BuildGoogleApiClient();
                            CreateLocationRequest();
                        }
                    }
                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.setDelivered);
            SetUpMap();

            btnGetCoordinates = FindViewById<ImageButton>(Resource.Id.setDelivered_getPos);
            btnUpload = FindViewById<ImageButton>(Resource.Id.setDelivered_uploadPicture);
            btnConfirm = FindViewById<Button>(Resource.Id.setDelivered_Confirm);
            btnConfirm.Enabled = false;
            RegNr = Intent.GetStringExtra("findCarInput") ?? "Data not available";

            btnGetCoordinates.Click += delegate { FindLocation(); };
            btnUpload.Click += delegate { SelectImage(); };
            btnConfirm.Click += delegate { ConfirmDialog(); };
        }

        #region Select images
        private async void CreateLease()
        {
            int carId;
            RegNr = Intent.GetStringExtra("findCarInput") ?? "Data not available";
            await Task.Run(async () =>
            {
                carId = await new CarService().CreateLease(RegNr, mLastLocation.Latitude, mLastLocation.Longitude, pictures);
            });
        }

        private void SelectImage()
        {
            var intent = new Intent(Intent.ActionGetContent, MediaStore.Images.Media.ExternalContentUri);
            intent.SetType("image/*");
            intent.AddCategory(Intent.CategoryOpenable);

            StartActivityForResult(intent, SelectImageRequest);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case SelectImageRequest:
                    if (resultCode == Result.Ok)
                    {
                        byte[] inputData = convertImageToByte(data.Data);
                        if (pictures.Count < 3) pictures.Add(new Picture { PictureData = inputData });
                        else pictures[imageCount].PictureData = inputData;

                        switch (imageCount)
                        {
                            case 0:
                                imageView = FindViewById<ImageView>(Resource.Id.setDelivered_Picture);
                                imageCount++;
                                break;
                            case 1:
                                imageView = FindViewById<ImageView>(Resource.Id.setDelivered_Picture2);
                                imageCount++;
                                break;
                            case 2:
                                imageView = FindViewById<ImageView>(Resource.Id.setDelivered_Picture3);
                                imageCount = 0;
                                break;
                            default:
                                break;
                        }

                        imageUri = data.Data;
                        imageView.SetImageURI(imageUri);

                        if (mLastLocation != null) btnConfirm.Enabled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        public byte[] convertImageToByte(Android.Net.Uri uri)
        {
            Stream stream = ContentResolver.OpenInputStream(uri);
            byte[] byteArray;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }
            return byteArray;
        }
        #endregion

        #region Update lease
        private void ConfirmDialog()
        {
            AlertDialog.Builder mBuilder = new AlertDialog.Builder(this);
            AlertDialog alert = mBuilder.Create();
            alert.SetTitle("Lever tilbake bil");
            alert.SetIcon(Resource.Drawable.logo);
            alert.SetMessage("Trykk bekreft for å levere bilen tilbake.");
            alert.SetButton("Bekreft", (c, ev) => 
            {
                UpdateLease();
                Toast.MakeText(this, RegNr + " er nå registrert som levert.", ToastLength.Long).Show();
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
                leaseId = await new CarService().PutLease(RegNr, mLastLocation.Latitude, mLastLocation.Longitude, pictures);
            });

        }
        #endregion

        #region Find location
        private void FindLocation()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, MY_PERMISSION_REQUEST_CODE);
            }
            else
            {
                if (CheckPlayServices())
                {
                    BuildGoogleApiClient();
                    CreateLocationRequest();
                }
            }
        }

        private void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FATEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
        }

        private void BuildGoogleApiClient()
        {
            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API)
                .Build();
            mGoogleApiClient.Connect();
        }

        private bool CheckPlayServices()
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    GooglePlayServicesUtil.GetErrorDialog(resultCode, this, PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, "This device does not support Google Play Services", ToastLength.Long).Show();
                    Finish();
                }
                return false;
            }
            return true;
        }

        private void DisplayLocation()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            mLastLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);
            if (mLastLocation != null)
            {
                double lat = mLastLocation.Latitude;
                double lng = mLastLocation.Longitude;
                Toast.MakeText(this, $"{lat} / {lng}", ToastLength.Long).Show();
                if (imageUri != null)
                {
                    btnConfirm.Enabled = true;
                }

                LatLng latlng = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
                mMap.MoveCamera(camera);
                MarkerOptions options = new MarkerOptions()
                .SetPosition(latlng)
                .SetTitle("Din posisjon");
                mMap.AddMarker(options);
            }
            else
            {
                txtCoordinates.Text = "Couldn't get the location.";
            }
        }

        private void StartLocationUpdates()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, this);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {

        }

        public void OnConnected(Bundle connectionHint)
        {
            DisplayLocation();
            if (mRequestinglocationUpdates)
            {
                StartLocationUpdates();
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            mGoogleApiClient.Connect();
        }

        public void OnLocationChanged(Location location)
        {
            mLastLocation = location;
            DisplayLocation();
        }
        #endregion

        #region Map
        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.setDelivered_Map).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;
        }
        #endregion
    }
}