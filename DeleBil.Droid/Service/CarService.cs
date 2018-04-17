using Android.Graphics;
using Android.Util;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Xamarin.Facebook;

namespace DeleBil.Droid.Service
{
    public class CarService
    {
        public enum LoginStatus
        {
            LoggedOut,
            NeedsWebApiToken,
            LoggedIn
        }

        private readonly string baseUrl;
        private readonly string GetCarsUrl = "api/Cars/";
        private readonly string GetLeasesUrl = "api/Leases/";
        private const string CreateLeaseUrl = "/api/Leases/NewLease";
        private const string UpdateLeaseUrl = "/api/Leases/UpdateLease";
        private readonly string VerifyExternalTokenUrl = "/api/Account/VerifyExternalToken";
        private ExternalTokenResponse delebilToken;
        private AndroidSecureDataProvider secureDataProvider;
        private const string DeleBilStoreKey = "DeleBil";

        public LoginStatus GetLoginStatus()
        {
            var facebookToken = AccessToken.CurrentAccessToken;
            if (facebookToken == null || string.IsNullOrWhiteSpace(facebookToken.Token) || facebookToken.Expires.Before(new Date()))
            {
                return LoginStatus.LoggedOut;
            }

            if (delebilToken == null || string.IsNullOrWhiteSpace(delebilToken.AccessToken) || delebilToken.Expires < System.DateTime.Now)
            {
                return LoginStatus.NeedsWebApiToken;
            }

            return LoginStatus.LoggedIn;
        }

        private string AuthorizationHeader
        {
            get
            {
                return "Bearer " + delebilToken.AccessToken;
            }
        }

        public CarService()
        {
#if DEBUG
            this.baseUrl = "http://192.168.10.151:63467/";
#else
            this.baseUrl = "";
#endif
            secureDataProvider = new AndroidSecureDataProvider();
            delebilToken = secureDataProvider.Retreive(DeleBilStoreKey)
                .FromDictionary<ExternalTokenResponse>();
        }

        public void SignOut()
        {
            delebilToken = null;
            secureDataProvider.Clear(DeleBilStoreKey);
        }

        private WebClient CreateWebClient()
        {
            return new WebClient()
            {
                BaseAddress = baseUrl
            };
        }

        private void EnsureLoggedIn()
        {
            var status = GetLoginStatus();
            if (status == LoginStatus.LoggedIn)
                return;
            throw new NotLoggedInException(status);
        }

        public async Task SignInWithFacebookToken(string token)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    var request = new ExternalTokenRequest
                    {
                        Provider = "Facebook",
                        Token = token
                    };

                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                    var result = await client.UploadStringTaskAsync(
                        VerifyExternalTokenUrl,
                        "POST",
                        JsonConvert.SerializeObject(request));

                    var response = JsonConvert.DeserializeObject<ExternalTokenResponse>(result);

                    secureDataProvider.Store(response.UserId, DeleBilStoreKey, response.ToDictionary());

                    delebilToken = response;
                }
            }
            catch (Exception ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Error signing in");
            }
        }

        //Hent en liste med leases (Available) til kart
        public async Task<Lease[]> GetLeases(double lat, double lng)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("DeleBil", "Getting leases");
                    var json = await client.DownloadStringTaskAsync(GetLeasesUrl + lat + "/" + lng + "/");
                    return JsonConvert.DeserializeObject<Lease[]>(json);
                }
            }
            catch (Exception ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Could not get leases");
                return new Lease[0];
            }
        }

        //Hent en bil
        public async Task<int> GetCar(string regNr)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("DeleBil", "Getting lease");
                    var json = await client.DownloadStringTaskAsync(GetCarsUrl + regNr);
                    return JsonConvert.DeserializeObject<int>(json);
                }
            }
            catch (WebException ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Could not get lease");
                return -1;
            }
        }

        //Hent et lease
        public async Task<Lease> GetLease(string regNr)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("DeleBil", "Getting lease");
                    var json = await client.DownloadStringTaskAsync(GetLeasesUrl + regNr);
                    return JsonConvert.DeserializeObject<Lease>(json);
                }
            }
            catch (WebException ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Could not get lease");
                return null;
            }
        }

        //Opprett et lease
        public async Task<int> CreateLease(string regNr, double latitude, double longtitude, IList<Picture> pictures)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("DeleBil", "Creating lease");
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    var request = new { RegNr = regNr, Lat = latitude, Lng = longtitude, Pictures = pictures };
                    var result = await client.UploadStringTaskAsync(CreateLeaseUrl, "POST", JsonConvert.SerializeObject(request));
                    var carId = JsonConvert.DeserializeObject<int>(result);
                    return carId;
                }
            }
            catch (WebException ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Could not create lease");
                return -1;
            }
        }

        //Oppdater et lease til Status = Rented/Delivered
        public async Task<int> PutLease(string regNr, double latitude, double longtitude, IList<Picture> pictures)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("DeleBil", "Updating lease");
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    var request = new { RegNr = regNr, Lat = latitude, Lng = longtitude, Pictures = pictures };
                    var result = await client.UploadStringTaskAsync(UpdateLeaseUrl, "PUT", JsonConvert.SerializeObject(request));
                    var leaseId = JsonConvert.DeserializeObject<int>(result);
                    return leaseId;
                }
            }
            catch (WebException ex)
            {
                Log.Error("DeleBil", Java.Lang.Throwable.FromException(ex), "Could not update lease");
                return -1;
            }
        }
    }
}