using System;
using Xamarin.Facebook;

namespace DeleBil.Droid.Service
{
    public class FacebookTokenTracker : AccessTokenTracker
    {
        private readonly CarService service;
        public Action HandleLoggedIn { get; set; }
        public Action HandleLoggedOut { get; set; }

        public FacebookTokenTracker(CarService service)
        {
            this.service = service;
        }

        protected override async void OnCurrentAccessTokenChanged(AccessToken oldAccessToken, AccessToken currentAccessToken)
        {
            if (currentAccessToken == null)
            {
                service.SignOut();
                HandleLoggedOut?.Invoke();
            }
            else
            {
                await service.SignInWithFacebookToken(currentAccessToken.Token);
                HandleLoggedIn?.Invoke();
            }
        }
    }
}