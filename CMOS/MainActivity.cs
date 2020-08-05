using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.V7.Widget;
using CMOS.Data_Models;
using CMOS.Adapter;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        RecyclerView ordersRecyclerView;
        List<Order> ordersList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            ordersRecyclerView = (RecyclerView)FindViewById(Resource.Id.ordersRecyclerView);
            CreateData();
            SetupRecyclerView();
        }

        private void SetupRecyclerView()
        {
            ordersRecyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(ordersRecyclerView.Context));
            OrdersAdapter adapter = new OrdersAdapter(ordersList);
            ordersRecyclerView.SetAdapter(adapter);
        }

        private void CreateData()
        {
            ordersList = new List<Order>();
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            var json = new WebClient().DownloadString("http://192.168.1.33/CMOS/CMOSS/GetTableNoClothingOrder");
            JObject googleSearch = JObject.Parse(json);
            IList<JToken> results = googleSearch["data"].Children().ToList();
            foreach (JToken result in results)
            {
                Order searchResult = result.ToObject<Order>();
                searchResult.PositionName = searchResult.PositionName.Replace("\r\n\n", ";");
                searchResult.PercentComplited += "%";
                searchResult.Id = "Заказ №: " + searchResult.Id;
                searchResult.NumberTN = "ПТМЦ №: " + searchResult.NumberTN;
                ordersList.Add(searchResult);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}