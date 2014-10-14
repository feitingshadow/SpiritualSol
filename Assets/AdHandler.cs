using UnityEngine;
using System.Collections;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class AdHandler : MonoBehaviour {

	private BannerView bannerView;
	// Use this for initialization
	void Start () {
		// Create a 320x50 banner at the bottom of the screen.
		 bannerView = new BannerView(
			"ca-app-pub-6297248724638398/7870547062", AdSize.Banner, AdPosition.Top);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
