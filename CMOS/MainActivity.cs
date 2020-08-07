using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.V7.Widget;
using CMOS.Data_Models;
using CMOS.Adapter;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using Android.Widget;
using System;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        RecyclerView ordersRecyclerView;
        List<Order> ordersList;
        List<Position> positionsList;
        OrdersAdapter adapterOrder;
        PositionsAdapter adapterPosition;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            ordersRecyclerView = (RecyclerView)FindViewById(Resource.Id.ordersRecyclerView);
            CreateOrdersData();
            SetupOrdersRecyclerView();
        }

        private void SetupOrdersRecyclerView()
        {
            ordersRecyclerView.SetLayoutManager(new LinearLayoutManager(ordersRecyclerView.Context));
            adapterOrder = new OrdersAdapter(ordersList);
            adapterOrder.ItemClick += OnItemClick;
            ordersRecyclerView.SetAdapter(adapterOrder);
        }

        private void SetupPositionsRecyclerView()
        {
            ordersRecyclerView.SetLayoutManager(new LinearLayoutManager(ordersRecyclerView.Context));
            adapterPosition = new PositionsAdapter(positionsList);
            ordersRecyclerView.SetAdapter(adapterPosition);
        }

        private void CreateOrdersData()
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

        private void CreatePositionsData(int id)
        {
            positionsList = new List<Position>();
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            var json = new WebClient().DownloadString("http://192.168.1.33/CMOS/CMOSS/GetPositionsPreorderApi/" + id.ToString());
            JObject googleSearch = JObject.Parse(json);
            IList<JToken> results = googleSearch["data"].Children().ToList();
            foreach (JToken result in results)
            {
                Position searchResult = result.ToObject<Position>();
                positionsList.Add(searchResult);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void OnItemClick(object sender, int position)
        {
            Toast.MakeText(this, "This is photo number " + ordersList[position].Id, ToastLength.Short).Show();
            CreatePositionsData(Convert.ToInt32(ordersList[position].Id.Replace("Заказ №: ", "")));
            SetupPositionsRecyclerView();
        }
    }
}