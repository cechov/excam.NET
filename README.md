# excam.NET
This is the result of the exam for the .NET developer education at Norges Yrkesakademi, former eCademy. The study included subjects required to create web applications using HTML5/JavaScript, Visual Basic.NET, Sky services (Windows Azure), SQL, C # and ASP.NET MVC, as well as development against mobile platforms using Xamarin.

<hr/>
<br/>
<b>Installation instructions:</b>

1.	Unpack the solution on the desktop.<br/><br/>
2.	Enter the folder DeleBil\.vs\config and open the file applicationhost.config <br/>
        &nbsp; &nbsp; •	<b>Locate the following code:</b> <br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;site name="DeleBil" id="2"&gt; <br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;application path="/" applicationPool="Clr4IntegratedAppPool"&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;virtualDirectory path="/" physicalPath="C:\Users\Cecilie\source\repos\DeleBil\DeleBil" /&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;/application&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;bindings&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;binding protocol="http" bindingInformation="*:63467:192.168.10.151" /&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;binding protocol="http" bindingInformation="*:63467:localhost" /&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;/bindings&gt;<br/>
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;/site&gt;<br/>
        &nbsp; &nbsp; •	<b>Change</b> 192.168.10.151 <b>to your own IP-adress.</b> <br/>
        &nbsp; &nbsp; •	<b>Change</b> physicalPath <b>to the solution’s location.</b><br/>
        &nbsp; &nbsp; •	<b>Run Windows Defender Firewall. Create incoming rule:</b><br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>Type:</b> Port<br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>The rule will be used on:</b> TCP<br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>Specific local gates:</b> 63467<br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>Action:</b> Allow the connection<br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>The rule should be used:</b> Domain, Private and Public<br/>
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <b>Enter the desired name.</b><br/>
<br/>
3.	Open Visual Studio as Administrator.<br/><br/>
4.	Open the project, change the following:<br/>
    &nbsp; &nbsp; •	<b>Create App ID and App Secret at</b> facebook.developers.com <br/>
    &nbsp; &nbsp; •	<b>Fill in FacebookAppSecret and FacebookAppId in</b> \DeleBil\Web.config<br/>
    &nbsp; &nbsp; •	<b>Fill in values for facebook_app_id and> fb_login_protocol_scheme in</b> DeleBil\DeleBil.Droid\Resources\values\Strings.xml<br/>
    &nbsp; &nbsp; •	<b>Create API in GoogleAPIConsole.</b> <br>
    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Enable Google Maps Android API and Google Maps Javascript API and create two API-keys. <br/>
    &nbsp; &nbsp; •	<b>Change the following code to contain your own google API:</b><br/>
    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &lt;script async defer
    src="https://maps.googleapis.com/maps/api/js?key=AIzaSyCv_OTPW8qrXwkrIsEJtjj0kHPOFRV3Tkc&callback=initMap"&gt;
    &lt;/script&gt;<br/>
    &nbsp; &nbsp; •	<b>Fill in values for google_maps_key in</b> DeleBil\DeleBil.Droid\Resources\values\Strings.xml<br/>
<br/><br/>
5.	Rebuild, and click run.<br/>

<br/><br/><hr/><br/>
<b>Technical information:</b>

<b>The web application is tested on:</b> Chrome, IE, FireFox and Opera.<br/>
<b>Minimum SDK-version for mobile application:</b> 25<br/>
<b>The solution is tested on Android Emulator:</b> Android 7.1 – API 25<br/><br/>

<b>Predefined users: </b>
1.	Username: testuser
&nbsp; &nbsp; , &nbsp; &nbsp; Password: access2APP!
2.	Username: anotheruser
&nbsp; &nbsp; , &nbsp; &nbsp; Password: access2APP!

Login to the mobile application is through Facebook.

<br/><br/><hr/><br/>
<b>The solution: Web</b>

All functionality on the web application requires a logged in user.<br/><br/>
When logging in, a page will be displayed to manage cars for lending. The page shows an overview of the user's registered cars, which vehicles are registered for lending, as well as loan history.<br/><br/>
By clicking + you will get to a page where a new car can be registered. The licence number, name and image are required. <br/><br/>
By clicking the licence number, the user will be sent to a detailed page for the selected car. If there is no active lease on the chosen car, users will be able to edit and delete the car.<br/><br/>
By clicking Details, users will be sent to a retail page for lending. Showing details for the car, location and who borrows the car. Depending on the status of the lease, it can be deleted or approved for submission.<br/><br/>

<br/><br/><hr/><br/>
<b>The solution: Mobile</b>

All functionality on the mobile application requires a logged in user.<br/><br/>
The download button is activated when the search box contains more than 4 characters. When searching, check first if the car exists in the database. If the car does not exist, an error message will appear to the user. If the car exists, the user is redirected to the correct side. <br/><br/>
On the page to lend a car, the loan release button is inactive until the location is retrieved and at least one image is uploaded. A user can upload up to 3 images. After the car is registered for lending, the page closes and the user is sent back to the front page. A toast will appear with confirmation of the incident.  <br/><br/>
The page for leasing a car will display the car's PickUpLocation, title and image. The user must confirm the loan in a popup before the loan is registered, the activity is closed and a toast confirms the event.<br/><br/>
On the page to return a car, the return button is inactive until the location is retrieved and at least one image is uploaded. (Same functionality as on the page to lend a car.) User must confirm delivery in a popup before the loan is registered, the activity is closed and a toast confirms the event.  <br/><br/>
On the map view, the location of the mobile phone is retrieved and the camera's camera sets at that location. All leases with status Available within mobile location +/- 0.3 latitude / longitude are retrieved and placed with a pin on the map. Clicking a pin opens a popup with the distinguishing number and title. Click on the popup opens the page to borrow a car. I have submitted test data with locations in Norway. Set your location to Longtitude: 5.27353 Latitude: 59.3892 to view the test data.<br/><br/>  
