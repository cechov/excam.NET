using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Xamarin.Facebook;
using DeleBil.Droid.Service;
using Xamarin.Facebook.Login;
using Android.Util;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Android.Views;
using Xamarin.Facebook.Login.Widget;

namespace DeleBil.Droid
{
    [Activity(MainLauncher = true)]
    public class MainActivity : Activity
    {
        private string txtSearch;
        private AutoCompleteTextView searchCar;
        private Button findCar;
        private Button showAvailableCars;
        ICallbackManager callbackManager;
        FacebookTokenTracker tokenTracker;
        CarService carService;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FacebookSdk.SdkInitialize(this);
            SetContentView(Resource.Layout.Main);

            FindViewById<LoginButton>(Resource.Id.login_button).SetReadPermissions("email");
            callbackManager = CallbackManagerFactory.Create();

            var loginCallback = new FacebookCallback<LoginResult>
            {
                HandleSuccess = SignInWithFacebookToken,
                HandleCancel = () => Log.Debug(
                    Application.PackageName,
                    "Canceled"),
                HandleError = error => Log.Error(
                    Application.PackageName,
                    Java.Lang.Throwable.FromException(error),
                    "No access")
            };
            LoginManager.Instance.RegisterCallback(callbackManager, loginCallback);

            carService = new CarService();
            tokenTracker = new FacebookTokenTracker(carService)
            {
                HandleLoggedIn = UpdateButtons,
                HandleLoggedOut = UpdateButtons
            };

            tokenTracker.StartTracking();

            if (carService.GetLoginStatus() == CarService.LoginStatus.NeedsWebApiToken)
            {
                await SignInWithFacebookToken(AccessToken.CurrentAccessToken.Token);
            }
            
            UpdateButtons();

            searchCar = FindViewById<AutoCompleteTextView>(Resource.Id.searchCar);
            findCar = FindViewById<Button>(Resource.Id.findCar);
            showAvailableCars = FindViewById<Button>(Resource.Id.showAvailableCars);

            findCar.Click += findCar_Click;
            findCar.Enabled = false;
            searchCar.TextChanged += (sender, e) =>
            {
                if (searchCar.Text.Length > 4)
                {
                    findCar.Enabled = true;
                }
                else findCar.Enabled = false;
            };
            
            showAvailableCars.Click += (sender, args) => StartActivity(typeof(allAvailableActivity));
        }

        #region Search car
        private void ConfirmDialog()
        {
            AlertDialog.Builder mBuilder = new AlertDialog.Builder(this);
            AlertDialog alert = mBuilder.Create();
            alert.SetTitle("Feil.");
            alert.SetIcon(Resource.Drawable.logo);
            alert.SetMessage("Finner ikke bilen du leter etter. Prøv igjen.");
            alert.SetButton("OK", (c, ev) =>
            {
                Toast.MakeText(this, "Prøv igjen.", ToastLength.Long).Show();
                searchCar.Text = "";
            });
            alert.Show();
        }

        private async void findCar_Click(object sender, System.EventArgs e)
        {
            int doesCarExist = 0;
            Lease lease = new Lease();
            txtSearch = FindViewById<AutoCompleteTextView>(Resource.Id.searchCar).Text;

            await Task.Run(async () =>
            {
                doesCarExist = await new CarService().GetCar(txtSearch);
            });

            switch (doesCarExist)
            {
                case 1:
                    await Task.Run(async () => { lease = await new CarService().GetLease(txtSearch); });
                    switch (lease.Status)
                    {
                        case LeaseStatus.Available:
                            var setRentedActivity = new Intent(this, typeof(setRentedActivity));
                            setRentedActivity.PutExtra("findCarInput", txtSearch);
                            StartActivity(setRentedActivity);
                            break;
                        case LeaseStatus.Rented:
                            var setDeliveredActivity = new Intent(this, typeof(setDeliveredActivity));
                            setDeliveredActivity.PutExtra("findCarInput", txtSearch);
                            StartActivity(setDeliveredActivity);
                            break;
                        default:
                            var setAvailableActivity = new Intent(this, typeof(setAvailableActivity));
                            setAvailableActivity.PutExtra("findCarInput", txtSearch);
                            StartActivity(setAvailableActivity);
                            break;
                    }
                    break;
                default:
                    ConfirmDialog();
                    break;
            }
        }
        #endregion

        #region Log in
        protected override void OnDestroy()
        {
            tokenTracker.StopTracking();
            base.OnDestroy();
        }

        private void UpdateButtons()
        {
            var searchCar = FindViewById<AutoCompleteTextView>(Resource.Id.searchCar);
            var findCar = FindViewById<Button>(Resource.Id.findCar);
            var showAvailableCars = FindViewById<Button>(Resource.Id.showAvailableCars);
            switch (carService.GetLoginStatus())
            {
                case CarService.LoginStatus.LoggedOut:
                //case CarService.LoginStatus.NeedsWebApiToken:
                    searchCar.Enabled = false;
                    findCar.Enabled = false;
                    showAvailableCars.Enabled = false;
                    break;
                case CarService.LoginStatus.LoggedIn:
                    searchCar.Enabled = true;
                    showAvailableCars.Enabled = true;
                    break;
                default:
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        private void SignInWithFacebookToken(LoginResult loginResult)
        {
            var token = loginResult.AccessToken.Token;
            Log.Debug(Application.PackageName, token);
            Task.Run(async () =>
            {
                await SignInWithFacebookToken(token);
                RunOnUiThread(() => UpdateButtons());
            });
        }

        private async Task SignInWithFacebookToken(string token)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await carService.SignInWithFacebookToken(token);
                    RunOnUiThread(() => UpdateButtons());
                }
                catch (System.Exception ex)
                {
                    RunOnUiThread(() => Toast.MakeText(this, ex.Message, ToastLength.Long)
                            .Show());
                }
            });
        }
        #endregion
    }
}

