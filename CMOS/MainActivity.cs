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
using Android.Views;
using Symbol.XamarinEMDK;
using System.IO;
using System.Xml;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, EMDKManager.IEMDKListener
    {
        private TextView numberTNForOrderlist;
        private RecyclerView ordersRecyclerView;
        private List<Order> ordersList;
        private List<Position> positionsList;
        private OrdersAdapter adapterOrder;
        private PositionsAdapter adapterPosition;
        private ImageButton buttonRemove;
        private ImageButton buttonAplay;
        private Android.Support.V7.Widget.Toolbar toolbarInputData;

        private EMDKManager emdkManager = null;
        private ProfileManager profileManager = null;
        private EditText codeInput = null;

        private Button button1 = null;
        private int count;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            numberTNForOrderlist = (TextView)FindViewById(Resource.Id.numberTNForOrderlist);
            ordersRecyclerView = (RecyclerView)FindViewById(Resource.Id.ordersRecyclerView);
            buttonRemove = (ImageButton)FindViewById(Resource.Id.buttonRemove);
            buttonAplay = (ImageButton)FindViewById(Resource.Id.buttonAplay);
            toolbarInputData = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbarInputData);
            codeInput = (EditText)FindViewById(Resource.Id.codeInput);


            EMDKResults results = EMDKManager.GetEMDKManager(ApplicationContext, this);
            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                codeInput.SetText("Failed", TextView.BufferType.Normal);
            }
            else
            {
                codeInput.SetText("00000", TextView.BufferType.Normal);
            }

            count = 0;
            button1 = (Button)FindViewById(Resource.Id.button1);
            button1.Click += delegate { button1.Text = string.Format("{0} clicks!", count++); };
            button1.Click += delegate { ApplyProfile(); };

            toolbarInputData.Visibility = ViewStates.Invisible;
            buttonRemove.Visibility = ViewStates.Invisible;
            buttonAplay.Visibility = ViewStates.Invisible;

            buttonRemove.Click += Btn_Click;

            InitializingOrdersList();
        }

        private void ApplyProfile()
        {
            if (profileManager != null)
            {
                EMDKResults results = profileManager.ProcessProfile("ClockProfile", ProfileManager.PROFILE_FLAG.Set, new String[] { "" });
                if (results.StatusCode == EMDKResults.STATUS_CODE.Success)
                {
                    codeInput.Text = "applied";
                }
                else if (results.StatusCode == EMDKResults.STATUS_CODE.CheckXml)
                {
                    using (XmlReader reader = XmlReader.Create(new StringReader(results.StatusString)))
                    {
                        String checkXmlStatus = "Status:\n\n";
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    switch (reader.Name)
                                    {
                                        case "parm-error":
                                            checkXmlStatus += "Parm Error:\n";
                                            checkXmlStatus += reader.GetAttribute("name") + " - ";
                                            checkXmlStatus += reader.GetAttribute("desc") + "\n\n";
                                            break;
                                        case "characteristic-error":
                                            checkXmlStatus += "characteristic Error:\n";
                                            checkXmlStatus += reader.GetAttribute("type") + " - ";
                                            checkXmlStatus += reader.GetAttribute("desc") + "\n\n";
                                            break;
                                    }
                                    break;
                            }
                        }

                        if (checkXmlStatus == "Status:\n\n")
                        {
                            codeInput.Text = "St Ok.";
                        }
                        else
                        {
                            codeInput.Text = checkXmlStatus;
                        }

                    }
                }
                else
                {
                    codeInput.Text = "faild" + results.StatusCode;
                }
            }
            else
            {
                codeInput.Text = "is null";
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            InitializingOrdersList();
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
            adapterPosition.ItemClick += OnPositionClick;
            ordersRecyclerView.SetAdapter(adapterPosition);
        }

        private void CreateOrdersData()
        {
            ordersList = new List<Order>();
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            var json = new WebClient().DownloadString("http://192.168.1.33/CMOS/CMOSS/GetTableNoClothingOrderApi");
            JObject googleSearch = JObject.Parse(json);
            IList<JToken> results = googleSearch["data"].Children().ToList();
            foreach (JToken result in results)
            {
                Order searchResult = result.ToObject<Order>();
                searchResult.PositionName = searchResult.PositionName;
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

        private void InitializingOrdersList()
        {
            toolbarInputData.Visibility = ViewStates.Invisible;
            buttonRemove.Visibility = ViewStates.Invisible;
            buttonAplay.Visibility = ViewStates.Invisible;
            CreateOrdersData();
            SetupOrdersRecyclerView();
            numberTNForOrderlist.Text = "         Документы поступления";
        }

        void OnItemClick(object sender, int position)
        {
            buttonRemove.Visibility = ViewStates.Visible;
            buttonAplay.Visibility = ViewStates.Visible;
            toolbarInputData.Visibility = ViewStates.Visible;
            Toast.MakeText(this, ordersList[position].Id, ToastLength.Short).Show();
            CreatePositionsData(Convert.ToInt32(ordersList[position].Id.Replace("Заказ №: ", "")));
            SetupPositionsRecyclerView();
            numberTNForOrderlist.Text = "         " + ordersList[position].NumberTN;
        }

        void OnPositionClick(object sender, int position)
        {

        }

        public void OnClosed()
        {
            codeInput.Text = "failed";
            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnOpened(EMDKManager emdkManager)
        {
            codeInput.Text = "Opened";
            this.emdkManager = emdkManager;
            try
            {
                profileManager = (ProfileManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Profile);
            }
            catch (Exception e)
            {
                codeInput.Text = e.Message;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (profileManager != null)
            {
                profileManager = null;
            }
            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }
    }
}