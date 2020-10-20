﻿using Android.App;
using Android.OS;
using Android.Support.V7.App;
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
using Symbol.XamarinEMDK.Barcode;
using Symbol.XamarinEMDK;
using Xamarin.Essentials;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, EMDKManager.IEMDKListener
    {
        //Data
        private int count;
        private string sku;
        //private double weight;
        private Position pos;

        //EMDK
        private BarcodeManager _barcodeManager;
        private EMDKManager _emdkManager;
        private Scanner _scanner;

        //Elements
        private TextView numberTNForOrderlist;
        private RecyclerView ordersRecyclerView;
        private List<Order> ordersList;
        private List<Position> positionsList;
        private OrdersAdapter adapterOrder;
        private PositionsAdapter adapterPosition;
        private ImageButton buttonRemove;
        private ImageButton buttonAplay;
        private Android.Support.V7.Widget.Toolbar toolbarInputData;
        private EditText codeInput;
        private EditText quentityInput;
        private EditText weightInput;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.activity_main);

            numberTNForOrderlist = (TextView)FindViewById(Resource.Id.numberTNForOrderlist);
            ordersRecyclerView = (RecyclerView)FindViewById(Resource.Id.ordersRecyclerView);
            buttonRemove = (ImageButton)FindViewById(Resource.Id.buttonRemove);
            buttonAplay = (ImageButton)FindViewById(Resource.Id.buttonAplay);
            toolbarInputData = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbarInputData);
            codeInput = (EditText)FindViewById(Resource.Id.codeInput);
            weightInput = (EditText)FindViewById(Resource.Id.weightInput);
            quentityInput = (EditText)FindViewById(Resource.Id.quentityInput);

            var results = EMDKManager.GetEMDKManager(Application.Context, this);

            toolbarInputData.Visibility = ViewStates.Invisible;
            buttonRemove.Visibility = ViewStates.Invisible;
            buttonAplay.Visibility = ViewStates.Invisible;

            buttonRemove.Click += Btn_Click;

            InitializingOrdersList();
        }


        void EMDKManager.IEMDKListener.OnClosed()
        {
            if (_emdkManager != null)
            {
                _emdkManager.Release();
                _emdkManager = null;
            }
        }

        void EMDKManager.IEMDKListener.OnOpened(EMDKManager p0)
        {
            _emdkManager = p0;
            InitScanner();
        }

        private void InitScanner()
        {
            if (_emdkManager != null)
            {
                if (_barcodeManager == null)
                {
                    try
                    {
                        _barcodeManager = (BarcodeManager)_emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);
                        _scanner = _barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);
                        if (_scanner != null)
                        {
                            _scanner.Data += Scanner_Data;
                            _scanner.Status += Scanner_Status;
                            _scanner.Enable();
                            SetScannerConfig();
                        }
                        else
                        {
                        }
                    }
                    catch (ScannerException e)
                    {
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        private void Scanner_Status(object sender, Scanner.StatusEventArgs e)
        {
            var state = e.P0.State;
            if (state == StatusData.ScannerStates.Idle)
            {
                try
                {
                    if (_scanner.IsEnabled && !_scanner.IsReadPending)
                    {
                        SetScannerConfig();
                        _scanner.Read();
                    }
                }
                catch (ScannerException e1)
                {
                }
            }
        }

        private void SetScannerConfig()
        {
            var config = _scanner.GetConfig();
            config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
            config.ScanParams.DecodeLEDFeedback = true;
            config.ReaderParams.ReaderSpecific.ImagerSpecific.PicklistEx = ScannerConfig.PicklistEx.Hardware;
            config.DecoderParams.Code39.Enabled = true;
            _scanner.SetConfig(config);
        }

        private void Scanner_Data(object sender, Scanner.DataEventArgs e)
        {

            var scanDataCollection = e.P0;
            if ((scanDataCollection != null) && (scanDataCollection.Result == ScannerResults.Success))
            {
                var scanData = scanDataCollection.GetScanData();
                if (scanData[0].Data == null)
                {
                    return;
                }
                if(pos != null)
                {
                    if(weightInput.Text == "0")
                    {
                        RunOnUiThread(() =>
                        {
                            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                            AlertDialog alert = dialog.Create();
                            alert.SetTitle("Масса не задана");
                            alert.SetMessage(sku + "взвесте ТМЦ");
                            alert.Show();
                        });
                    }
                    else
                    {
                        pos.Rate += count;
                        pos.Weight = Double.Parse(weightInput.Text);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            adapterPosition = new PositionsAdapter(positionsList);
                            ordersRecyclerView.SetAdapter(adapterPosition);
                            //color!!!
                            ordersRecyclerView.ScrollToPosition(adapterPosition.ItemCount - 1);
                        });
                        count = 1;
                        sku = GetSKUID(scanData[0].Data);
                        try
                        {
                            pos = positionsList.First(a => a.Code == sku && a.Rate < a.Norm);
                        }
                        catch
                        {
                            RunOnUiThread(() =>
                            {
                                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                                AlertDialog alert = dialog.Create();
                                alert.SetTitle("ТМЦ не найдено");
                                alert.SetMessage(sku + " - нет/избыток");
                                alert.Show();
                            });
                        }
                        RunOnUiThread(() => codeInput.Text = sku);
                        RunOnUiThread(() => quentityInput.Text = count.ToString());
                        RunOnUiThread(() => weightInput.Text = pos.Weight.ToString());
                    }
                }
                RunOnUiThread(ProcessScan);
            }
        }

        private string GetSKUID(string data)
        {
            return data.Replace("D", "").Replace("01000000", "");
        }

        private void ProcessScan()
        {
            if (string.IsNullOrEmpty(codeInput.Text))
            {
                Toast.MakeText(this, "You must scan or enter a barcode to begin", ToastLength.Long).Show();
                return;
            }
            else
            {
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            DeinitScanner();
            if (_emdkManager != null)
            {
                _emdkManager.Release();
                _emdkManager = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DeinitScanner();
            if (_emdkManager != null)
            {
                _emdkManager.Release();
                _emdkManager = null;
            }
        }

        private void DeinitScanner()
        {
            if (_emdkManager != null)
            {
                if (_scanner != null)
                {
                    try
                    {
                        _scanner.CancelRead();
                        _scanner.Disable();
                        _scanner.Data -= Scanner_Data;
                        _scanner.Status -= Scanner_Status;

                        _scanner.Release();
                    }
                    catch (ScannerException e)
                    {
                    }
                }
            }

            if (_barcodeManager != null)
            {
                _emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
            }

            _barcodeManager = null;
            _scanner = null;
        }

        protected override void OnResume()
        {
            base.OnResume();
            codeInput.Text = string.Empty;
            InitScanner();
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
    }
}