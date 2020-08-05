using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.V7.Widget;
using CMOS.Data_Models;
using CMOS.Adapter;
using System.Collections.Generic;

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
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 1233", Customer = "Эковуд", OrderId = "№ заказа: 48", Percent = "0%", Positions = "2015 - Модуль; 2016 - Модуль; 2017 - Модуль; 2018 - Модуль;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 5633", Customer = "Армис", OrderId = "№ заказа: 12", Percent = "9%", Positions = "2085-РУНН" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 4578", Customer = "Гратиус", OrderId = "№ заказа: 45", Percent = "98%", Positions = "2153-Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 2467", Customer = "Эковуд", OrderId = "№ заказа: 486", Percent = "90%", Positions = "2369-Модуль; 2015 - ШОТ;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 4534", Customer = "Эковуд", OrderId = "№ заказа: 5614", Percent = "15%", Positions = "2015-КРМ" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 2235", Customer = "Армис", OrderId = "№ заказа: 12", Percent = "18%", Positions = "2015 - Модуль; 2016 - Модуль;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 4531", Customer = "Эковуд", OrderId = "№ заказа: 1131", Percent = "0%", Positions = "2015 - ШОТ" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 1111", Customer = "Армис", OrderId = "№ заказа: 466", Percent = "13%", Positions = "2015 - ШТМ" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 4538", Customer = "Гратиус", OrderId = "№ заказа: 224", Percent = "100%", Positions = "2015 - Модуль; 2016 - Модуль;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 5200", Customer = "Эковуд", OrderId = "№ заказа: 4537", Percent = "99%", Positions = "2015 - Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 1237", Customer = "Армис", OrderId = "№ заказа: 78", Percent = "13%", Positions = "2015 - Модуль; 2015 - ШОТ;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 1112", Customer = "Эковуд", OrderId = "№ заказа: 42", Percent = "31%", Positions = "2015 - Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 3537", Customer = "Гратиус", OrderId = "№ заказа: 33", Percent = "52%", Positions = "2015 - ШОТ; 2015 - ШОТ;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 5344", Customer = "Армис", OrderId = "№ заказа: 6449", Percent = "64%", Positions = "2015 - Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 3969", Customer = "Эковуд", OrderId = "№ заказа: 45", Percent = "69%", Positions = "2015 - Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 7531", Customer = "Армис", OrderId = "№ заказа: 246", Percent = "87%", Positions = "2015 - Модуль; 2016 - Модуль;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 9942", Customer = "Гратиус", OrderId = "№ заказа: 254", Percent = "54%", Positions = "2015 - ШОТ" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 3145", Customer = "Эковуд", OrderId = "№ заказа: 250", Percent = "66%", Positions = "2015 - ШОТ; 2015 - ШОТ" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 1235", Customer = "Армис", OrderId = "№ заказа: 630", Percent = "64%", Positions = "2015 - Модуль" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 4578", Customer = "Эковуд", OrderId = "№ заказа: 2545", Percent = "65%", Positions = "2015 - Модуль; 2016 - Модуль;" });
            ordersList.Add(new Order { NumberTN = "№ ПТМЦ: 5436", Customer = "Гратиус", OrderId = "№ заказа: 123", Percent = "25%", Positions = "2015 - Модуль" });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}