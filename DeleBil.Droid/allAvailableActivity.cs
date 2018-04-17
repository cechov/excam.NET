using System;
using System.Collections.Generic;
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
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DeleBil.Droid.Service;
using static Android.Gms.Common.Apis.GoogleApiClient;
using static Android.Gms.Maps.GoogleMap;

namespace DeleBil.Droid
{
    [Activity(Label = "Tilgjengelige biler")]
    public class allAvailableActivity : Activity, IOnMapReadyCallback, IInfoWindowAdapter, IOnInfoWindowClickListener, IConnectionCallbacks, IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        private LocationRequest mLocationRequest;
        private GoogleApiClient mGoogleApiClient;
        private Location mLastLocation;
        private const int MY_PERMISSION_REQUEST_CODE = 7171;
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 7172;
        private static int UPDATE_INTERVAL = 5000; // SEC
        private static int FATEST_INTERVAL = 3000; // SEC
        private static int DISPLACEMENT = 10; // METERS
        private GoogleMap mMap;
        private IList<Lease> leases;

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

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.allAvailable);
            
            SetUpMap();
        }

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

        public void OnConnected(Bundle connectionHint)
        {
            DisplayLocationAsync();
            StartLocationUpdates();
        }

        private void StartLocationUpdates()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, this);
        }

        public void OnConnectionSuspended(int cause)
        {
            mGoogleApiClient.Connect();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {

        }

        public void OnLocationChanged(Location location)
        {
            mLastLocation = location;
        }

        private async void DisplayLocationAsync()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            mLastLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);
            if (mLastLocation != null)
            {
                LatLng myPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);

                CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(myPosition, 12);
                mMap.MoveCamera(camera);

                await Task.Run(async () =>
                {
                    leases = await new CarService().GetLeases(mLastLocation.Latitude, mLastLocation.Longitude);
                });

                foreach (Lease lease in leases)
                {
                    LatLng latlng = new LatLng(lease.latitudePickUpLocation, lease.longtitudePickUpLocation);
                    mMap.AddMarker(new MarkerOptions()
                    .SetPosition(latlng)
                    .SetTitle(lease.CarLicensePlate)
                    .SetSnippet(lease.CarName));

                    mMap.SetInfoWindowAdapter(this);
                    mMap.SetOnInfoWindowClickListener(this);
                }
            }
        }
        #endregion

        #region Map
        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;
            FindLocation();

            
        }

        public View GetInfoContents(Marker marker)
        {
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            View view = LayoutInflater.Inflate(Resource.Layout.mapInfoWindow, null, false);
            view.FindViewById<TextView>(Resource.Id.txtReg).Text = marker.Title;
            view.FindViewById<TextView>(Resource.Id.txtTitle).Text = marker.Snippet;
            return view;
        }

        public void OnInfoWindowClick(Marker marker)
        {
            var setRentedActivity = new Intent(this, typeof(setRentedActivity));
            setRentedActivity.PutExtra("findCarInput", marker.Title);
            StartActivity(setRentedActivity);
        }
        #endregion
    }
}