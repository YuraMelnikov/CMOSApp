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
using Android.Content;
using Android.Runtime;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, EMDKManager.IEMDKListener
    {
        //Data
        private int maxCount;
        private string sku;
        private int positionItem;
        private Position pos;
        private int orderId;
        private string partNumber;
        private bool isEdit;
        private bool isShortList;

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
        private ImageButton buttonPlus;
        private ImageButton buttonMinus;
        private ImageButton buttonComplited;
        private RadioGroup radioGroupFiltering;
        private RadioButton radioButtonAll;
        private RadioButton radioButtonDef;
        private RadioButton radioButtonWeight;

        public Context ACTION_USB_PERMISSION { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.activity_main);
            numberTNForOrderlist = (TextView)FindViewById(Resource.Id.numberTNForOrderlist);
            ordersRecyclerView = (RecyclerView)FindViewById(Resource.Id.ordersRecyclerView);
            buttonRemove = (ImageButton)FindViewById(Resource.Id.buttonRemove);
            buttonAplay = (ImageButton)FindViewById(Resource.Id.buttonAplay);
            buttonPlus = (ImageButton)FindViewById(Resource.Id.buttonPlus);
            buttonMinus = (ImageButton)FindViewById(Resource.Id.buttonMinus);
            buttonComplited = (ImageButton)FindViewById(Resource.Id.buttonComplited);
            toolbarInputData = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbarInputData);
            codeInput = (EditText)FindViewById(Resource.Id.codeInput);
            weightInput = (EditText)FindViewById(Resource.Id.weightInput);
            quentityInput = (EditText)FindViewById(Resource.Id.quentityInput);
            radioGroupFiltering = (RadioGroup)FindViewById(Resource.Id.radioGroupFiltering);
            radioButtonAll = (RadioButton)FindViewById(Resource.Id.radioButtonAll);
            radioButtonDef = (RadioButton)FindViewById(Resource.Id.radioButtonDef);
            radioButtonWeight = (RadioButton)FindViewById(Resource.Id.radioButtonWeight);
            try
            {
                var results = EMDKManager.GetEMDKManager(Application.Context, this);
            }
            catch
            {
            }
            buttonRemove.Click += ButtonRemove_Click; 
            buttonPlus.Click += ButtonPlus_Click;
            buttonMinus.Click += ButtonMinus_Click;
            buttonAplay.Click += ButtonAplay_Click;
            buttonComplited.Click += ButtonComplited_Click;
            radioButtonAll.Click += RadioButtonAll_Click;
            radioButtonDef.Click += RadioButtonDef_Click;
            radioButtonWeight.Click += RadioButtonWeight_Click;
            InitializingOrdersList();
            isEdit = false;
            isShortList = false;
        }

        private void RadioButtonAll_Click(object sender, EventArgs e)
        {
            if(isEdit == true)
                UpdateDataToServer();
            CreatePositionsData(orderId);
            SetupPositionsRecyclerView();
            isEdit = false;
            isShortList = false;
        }

        private void RadioButtonDef_Click(object sender, EventArgs e)
        {
            if (isEdit == true)
                UpdateDataToServer();
            if(isShortList == true)
            {
                CreatePositionsData(orderId);
                SetupPositionsRecyclerView();
            }
            positionsList = positionsList.Where(a => a.Rate != a.Norm || a.Id == 0).ToList();
            RunOnUiThread(() => codeInput.Text = "");
            RunOnUiThread(() => quentityInput.Text = "");
            RunOnUiThread(() => weightInput.Text = "");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                adapterPosition = new PositionsAdapter(positionsList);
                adapterPosition.ItemClick += OnPositionClick;
                ordersRecyclerView.SetAdapter(adapterPosition);
                ordersRecyclerView.ScrollToPosition(0);
            });
            codeInput.ClearFocus();
            quentityInput.ClearFocus();
            weightInput.ClearFocus();
            isEdit = false;
            isShortList = true;
        }

        private void RadioButtonWeight_Click(object sender, EventArgs e)
        {
            if (isEdit == true)
                UpdateDataToServer();
            if (isShortList == true)
            {
                CreatePositionsData(orderId);
                SetupPositionsRecyclerView();
            }
            positionsList = positionsList.Where(a => a.IsWeight == true || a.Id == 0).ToList();
            RunOnUiThread(() => codeInput.Text = "");
            RunOnUiThread(() => quentityInput.Text = "");
            RunOnUiThread(() => weightInput.Text = "");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                adapterPosition = new PositionsAdapter(positionsList);
                adapterPosition.ItemClick += OnPositionClick;
                ordersRecyclerView.SetAdapter(adapterPosition);
                ordersRecyclerView.ScrollToPosition(0);
            });
            codeInput.ClearFocus();
            quentityInput.ClearFocus();
            weightInput.ClearFocus();
            isEdit = false;
            isShortList = true;
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
            ScannerConfig config = _scanner.GetConfig();
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
                sku = GetSKUID(scanData[0].Data);
                try
                {
                    pos = positionsList.First(a => a.Code == sku && a.Rate < a.Norm);
                    positionItem = positionsList.FindIndex(a => a.Id == pos.Id);
                    pos.Rate++;
                    maxCount = pos.Norm;
                    RunOnUiThread(() => codeInput.Text = sku);
                    RunOnUiThread(() => quentityInput.Text = pos.Rate.ToString());
                    RunOnUiThread(() => weightInput.Text = pos.Weight.ToString());
                    isEdit = true;
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
                        return;
                    });
                }
                if (weightInput.Text == "0")
                {
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                        AlertDialog alert = dialog.Create();
                        alert.SetTitle("Масса не задана");
                        alert.SetMessage(sku + " взвесте ТМЦ");
                        alert.Show();
                        return;
                    });
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    adapterPosition = new PositionsAdapter(positionsList);
                    ordersRecyclerView.SetAdapter(adapterPosition);
                    ordersRecyclerView.ScrollToPosition(positionItem);
                });
                RunOnUiThread(ProcessScan);
            }
        }

        private string GetSKUID(string data)
        {
            return data.Replace("D", "").Replace("01000000", "").Replace(partNumber, "");
        }

        private void ProcessScan()
        {
            if (string.IsNullOrEmpty(codeInput.Text))
            {
                Toast.MakeText(this, "Вы можете отсканировать код позже", ToastLength.Long).Show();
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

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            InitializingOrdersList();
        }

        private void ButtonPlus_Click(object sender, EventArgs e)
        {
            try
            {
                int que = Convert.ToInt32(quentityInput.Text);
                if (que >= maxCount)
                {
                }
                else
                {
                    que++;
                }
                quentityInput.Text = que.ToString();
            }
            catch
            {
                quentityInput.Text = "1";
            }
        }

        private void ButtonMinus_Click(object sender, EventArgs e)
        {
            try
            {
                int que = Convert.ToInt32(quentityInput.Text);
                que--;
                if (que <= 0)
                    quentityInput.Text = "1";
                else
                    quentityInput.Text = que.ToString();
            }
            catch
            {
                quentityInput.Text = "1";
            }
        }

        private void ButtonAplay_Click(object sender, EventArgs e)
        {
            ordersRecyclerView.Visibility = ViewStates.Invisible;
            UpdateDataToServer();
            InitializingOrdersList();
            ordersRecyclerView.Visibility = ViewStates.Visible;
            isEdit = false;
        }

        private void UpdateDataToServer()
        {
            int stopPosition = positionsList.Count - 1;
            for (int i = 0; i < stopPosition; i++)
            {
                try
                {
                    string link = positionsList[i].Id.ToString() + "a" + positionsList[i].Rate.ToString().Replace(".", ",") + "a" + positionsList[i].Weight.ToString().Replace(".", ",");
                    new WebClient().DownloadString("http://192.168.1.33/CMOS/CMOSS/PostPositionsPreorderApi/" + link);
                }
                catch (Exception ex)
                {
                    ordersRecyclerView.Visibility = ViewStates.Visible;
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                        AlertDialog alert = dialog.Create();
                        alert.SetTitle("Сервер не отвечает");
                        alert.SetMessage("Повторите попытку" + " Ошибка: " + ex.Message);
                        alert.Show();
                        return;
                    });
                }
            }
            isEdit = false;
        }

        private void ButtonComplited_Click(object sender, EventArgs e)
        {
            SavePosData();
        }

        private void SavePosData()
        {
            try
            {
                if (pos != null)
                {
                    int quentity = Int32.Parse(quentityInput.Text);
                    if (quentity > pos.Norm)
                        quentity = pos.Norm;
                    pos.Rate = quentity;
                    if (pos.Weight != Double.Parse(weightInput.Text.Replace(".", ",")))
                        pos.IsWeight = true;
                    pos.Weight = Double.Parse(weightInput.Text.Replace(".", ","));
                    positionItem = positionsList.FindIndex(a => a.Id == pos.Id);
                    foreach (var t in positionsList)
                    {
                        if (t.Code == pos.Code)
                            t.Weight = pos.Weight;
                    }
                    RunOnUiThread(() => codeInput.Text = pos.Code);
                    RunOnUiThread(() => quentityInput.Text = pos.Rate.ToString());
                    RunOnUiThread(() => weightInput.Text = pos.Weight.ToString());
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        adapterPosition = new PositionsAdapter(positionsList);
                        adapterPosition.ItemClick += OnPositionClick;
                        ordersRecyclerView.SetAdapter(adapterPosition);
                        ordersRecyclerView.ScrollToPosition(positionItem);
                    });
                    codeInput.ClearFocus();
                    quentityInput.ClearFocus();
                    weightInput.ClearFocus();
                    isEdit = true;
                }
            }
            catch (Exception ex)
            {
                RunOnUiThread(() =>
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Ошибка сохранения");
                    alert.SetMessage("Ошибка: " + ex.Message);
                    alert.Show();
                    return;
                });
            }
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
            isEdit = false;
        }

        private void CreateOrdersData()
        {
            try
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
                isEdit = false;
            }
            catch(Exception ex)
            {
                RunOnUiThread(() =>
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Сервер не отвечает");
                    alert.SetMessage("Обратитесь к администратору" + ex.Message);
                    alert.Show();
                    return;
                });
            }
        }

        private void CreatePositionsData(int id)
        {
            try
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
                positionsList.Add(new Position { Code = "", Color = "", Loading = "", Name = "", Norm = 0, Order = "", ShortName = "" });
                isEdit = false;
            }
            catch (Exception ex)
            {
                RunOnUiThread(() =>
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Сервер не отвечает");
                    alert.SetMessage("Обратитесь к администратору" + ex.Message);
                    alert.Show();
                    return;
                });
            }
        }

        private void InitializingOrdersList()
        {
            toolbarInputData.Visibility = ViewStates.Invisible;
            buttonRemove.Visibility = ViewStates.Invisible;
            buttonAplay.Visibility = ViewStates.Invisible;
            buttonPlus.Visibility = ViewStates.Invisible;
            buttonMinus.Visibility = ViewStates.Invisible;
            buttonComplited.Visibility = ViewStates.Invisible;
            radioGroupFiltering.Visibility = ViewStates.Invisible;
            CreateOrdersData();
            SetupOrdersRecyclerView();
            numberTNForOrderlist.Text = "  Документы поступления";
            isEdit = false;
        }

        void OnItemClick(object sender, int position)
        {
            buttonRemove.Visibility = ViewStates.Visible;
            buttonAplay.Visibility = ViewStates.Visible;
            toolbarInputData.Visibility = ViewStates.Visible;
            radioGroupFiltering.Visibility = ViewStates.Visible; 
            Toast.MakeText(this, ordersList[position].Id, ToastLength.Short).Show();
            orderId = Convert.ToInt32(ordersList[position].Id.Replace("Заказ №: ", ""));
            CreatePositionsData(orderId);
            SetupPositionsRecyclerView();
            numberTNForOrderlist.Text = "         " + ordersList[position].NumberTN;
            partNumber = GetPartNumberForBarcode(ordersList[position].NumberTN);
            isEdit = false;
        }

        private string GetPartNumberForBarcode(string number)
        {
            number = number.Replace("ПТМЦ №: ", ""); 
            if (number.Length == 1)
                return "1000000" + number;
            else if (number.Length == 2)
                return "100000" + number;
            else if (number.Length == 3)
                return "10000" + number;
            else
                return "1000" + number;
        }

        void OnPositionClick(object sender, int position)
        {
            pos = positionsList[position];
            maxCount = pos.Norm;
            positionItem = positionsList.FindIndex(a => a.Id == pos.Id);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                adapterPosition = new PositionsAdapter(positionsList);
                adapterPosition.ItemClick += OnPositionClick;
                ordersRecyclerView.SetAdapter(adapterPosition);
                ordersRecyclerView.ScrollToPosition(positionItem);
            });
            RunOnUiThread(() => codeInput.Text = pos.Code);
            if(pos.Rate == 0)
                RunOnUiThread(() => quentityInput.Text = "");
            else
                RunOnUiThread(() => quentityInput.Text = pos.Rate.ToString());
            RunOnUiThread(() => weightInput.Text = pos.Weight.ToString());
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Android.Views.Keycode.Enter)
                SavePosData();
            return base.OnKeyUp(keyCode, e);
        }
    }
}