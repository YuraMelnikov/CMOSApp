using Android.App;
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
using System.Timers;

namespace CMOS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, EMDKManager.IEMDKListener
    {
        //Data
        private int maxCount;
        private string sku;
        private string skuPart;
        private int positionItem;
        private Position pos;
        private int orderId;
        private string part;
        private bool isEdit;
        private bool isShortList;

        //EMDK
        private BarcodeManager _barcodeManager;
        private EMDKManager _emdkManager;
        private Scanner _scanner;

        //Elements
        private Timer backToMainActivityTimer;
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
        private RadioGroup radioGroupFiltering;
        private RadioButton radioButtonAll;
        private RadioButton radioButtonDef;

        public Context ACTION_USB_PERMISSION { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
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
                radioGroupFiltering = (RadioGroup)FindViewById(Resource.Id.radioGroupFiltering);
                radioButtonAll = (RadioButton)FindViewById(Resource.Id.radioButtonAll);
                radioButtonDef = (RadioButton)FindViewById(Resource.Id.radioButtonDef);
                try
                {
                    var results = EMDKManager.GetEMDKManager(Application.Context, this);
                }
                catch
                {
                }
                buttonAplay.Click += ButtonAplay_Click;
                buttonRemove.Click += ButtonRemove_Click;
                radioButtonAll.Click += RadioButtonAll_Click;
                radioButtonDef.Click += RadioButtonDef_Click;
                InitializingOrdersList();
                isEdit = false;
                isShortList = false;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnCreate :" + ex.Message);
            }
        }

        private void CreateTimer()
        {
            backToMainActivityTimer = new Timer();
            backToMainActivityTimer.AutoReset = true;
            backToMainActivityTimer.Interval = 600000;
            backToMainActivityTimer.Elapsed += OnTimedEvent;
            backToMainActivityTimer.Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                AlertDialog alert = dialog.Create();
                alert.SetTitle("Вы давно не сохранялись");
                alert.SetMessage("Возможно стоит нажать кнопку СОХРАНИТЬ? :)");
                alert.Show();
                return;
            });
        }

        private void ErrorMessage(string ex)
        {
            RunOnUiThread(() =>
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                AlertDialog alert = dialog.Create();
                alert.SetTitle("Ошибка программы");
                alert.SetMessage(ex);
                alert.Show();
                return;
            });
        }

        private void RadioButtonAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (isEdit == true)
                    UpdateDataToServer();
                CreatePositionsData(orderId);
                SetupPositionsRecyclerView();
                isEdit = false;
                isShortList = false;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - RadioButtonAll_Click :" + ex.Message);
            }
        }

        private void RadioButtonDef_Click(object sender, EventArgs e)
        {
            try
            {
                if (isEdit == true)
                    UpdateDataToServer();
                if (isShortList == true)
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
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - RadioButtonDef_Click :" + ex.Message);
            }
        }

        void EMDKManager.IEMDKListener.OnClosed()
        {
            try
            {
                if (_emdkManager != null)
                {
                    _emdkManager.Release();
                    _emdkManager = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - EMDKManager.IEMDKListener.OnClosed :" + ex.Message);
            }
        }

        void EMDKManager.IEMDKListener.OnOpened(EMDKManager p0)
        {
            try
            {
                _emdkManager = p0;
                InitScanner();
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - EMDKManager.IEMDKListener.OnOpened :" + ex.Message);
            }
        }

        private void InitScanner()
        {
            try
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
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - InitScanner:" + ex.Message);
            }
        }

        private void Scanner_Status(object sender, Scanner.StatusEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - Scanner_Status:" + ex.Message);
            }
        }

        private void SetScannerConfig()
        {
            try
            {
                ScannerConfig config = _scanner.GetConfig();
                config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
                config.ScanParams.DecodeLEDFeedback = true;
                config.ReaderParams.ReaderSpecific.ImagerSpecific.PicklistEx = ScannerConfig.PicklistEx.Hardware;
                config.DecoderParams.Code39.Enabled = true;
                _scanner.SetConfig(config);
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - SetScannerConfig:" + ex.Message);
            }
        }

        private string GetSKU(string scanDataRes)
        {
            try
            {
                char[] charArray = scanDataRes.ToCharArray();
                string res = "";

                if (charArray.Length == 12)
                {
                    for (int i = 5; i > 0; i--)
                    {
                        res += charArray[12 - i];
                    }
                }
                else
                {
                    for (int i = 5; i > 0; i--)
                    {
                        res += charArray[13 - i];
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - GetSKU:" + ex.Message);
                return "";
            }
        }

        private string GetSKUPart(string scanDataRes)
        {
            try
            {
                bool isNull = false;
                char[] charArray = scanDataRes.ToCharArray();
                string res = "";
                if (charArray.Length == 13)
                {
                    for (int i = 9; i > 5; i--)
                    {
                        if (charArray[13 - i] != '0' || isNull == true)
                        {
                            res += charArray[13 - i];
                            isNull = true;
                        }
                    }
                }
                else
                {
                    for (int i = 8; i > 5; i--)
                    {
                        if (charArray[12 - i] != '0' || isNull == true)
                        {
                            res += charArray[12 - i];
                            isNull = true;
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - GetSKUPart:" + ex.Message);
                return "";
            }
        }

        private void Scanner_Data(object sender, Scanner.DataEventArgs e)
        {
            try
            {
                if (part != "")
                {
                    var scanDataCollection = e.P0;
                    if ((scanDataCollection != null) && (scanDataCollection.Result == ScannerResults.Success))
                    {
                        var scanData = scanDataCollection.GetScanData();
                        if (scanData[0].Data == null)
                        {
                            return;
                        }
                        string scanDataRes = GetSKUID(scanData[0].Data);
                        sku = GetSKU(scanDataRes);
                        skuPart = GetSKUPart(scanDataRes);
                        if (part != skuPart && skuPart != "")
                        {
                            RunOnUiThread(() =>
                            {
                                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                                AlertDialog alert = dialog.Create();
                                alert.SetTitle("Ошибка партии");
                                alert.SetMessage(sku + " из другой партии");
                                alert.Show();
                                return;
                            });
                        }
                        else
                        {
                            try
                            {
                                pos = positionsList.First(a => a.Code == sku && a.Rate < a.Norm);
                                pos.IsUpdate = true;
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
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                        AlertDialog alert = dialog.Create();
                        alert.SetTitle("Партия не выбрана");
                        alert.SetMessage("Нажмите на пункт партии");
                        alert.Show();
                        return;
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - Scanner_Data:" + ex.Message);
            }
        }

        private string GetSKUID(string data)
        {
            try
            {
                return data.Replace("D", "");
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - GetSKUID:" + ex.Message);
                return "";
            }
        }

        private void ProcessScan()
        {
            try
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
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - ProcessScan:" + ex.Message);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                DeinitScanner();
                if (_emdkManager != null)
                {
                    _emdkManager.Release();
                    _emdkManager = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnPause:" + ex.Message);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                DeinitScanner();
                if (_emdkManager != null)
                {
                    _emdkManager.Release();
                    _emdkManager = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnDestroy:" + ex.Message);
            }
        }

        private void DeinitScanner()
        {
            try
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
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - DeinitScanner:" + ex.Message);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                codeInput.Text = string.Empty;
                InitScanner();
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnResume:" + ex.Message);
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            try
            {
                backToMainActivityTimer.Stop();
                buttonRemove.Visibility = ViewStates.Invisible;
                InitializingOrdersList();
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - ButtonRemove_Click:" + ex.Message);
            }
        }

        private void ButtonAplay_Click(object sender, EventArgs e)
        {
            try
            {
                backToMainActivityTimer.Stop();
                UpdateDataToServer();
                isEdit = false;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - ButtonAplay_Click:" + ex.Message);
            }
        }

        private void UpdateDataToServer()
        {
            try
            {
                int stopPosition = positionsList.Count - 1;
                for (int i = 0; i < stopPosition; i++)
                {
                    if (positionsList[i].IsUpdate == true)
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
                }
                isEdit = false;
                CreateTimer();
                RunOnUiThread(() =>
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Сохранено");
                    alert.SetMessage("");
                    alert.Show();
                    return;
                });

            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - UpdateDataToServer:" + ex.Message);
            }
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
            try
            {
                ordersRecyclerView.SetLayoutManager(new LinearLayoutManager(ordersRecyclerView.Context));
                adapterOrder = new OrdersAdapter(ordersList);
                adapterOrder.ItemClick += OnItemClick;
                ordersRecyclerView.SetAdapter(adapterOrder);
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - SetupOrdersRecyclerView:" + ex.Message);
            }
        }

        private void SetupPositionsRecyclerView()
        {
            try
            {
                ordersRecyclerView.SetLayoutManager(new LinearLayoutManager(ordersRecyclerView.Context));
                adapterPosition = new PositionsAdapter(positionsList);
                adapterPosition.ItemClick += OnPositionClick;
                ordersRecyclerView.SetAdapter(adapterPosition);
                isEdit = false;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - SetupPositionsRecyclerView:" + ex.Message);
            }
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
            try
            {
                skuPart = "";
                toolbarInputData.Visibility = ViewStates.Invisible;
                buttonRemove.Visibility = ViewStates.Invisible;
                buttonAplay.Visibility = ViewStates.Invisible;
                radioGroupFiltering.Visibility = ViewStates.Invisible;
                CreateOrdersData();
                SetupOrdersRecyclerView();
                numberTNForOrderlist.Text = "Документы";
                isEdit = false;
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - InitializingOrdersList:" + ex.Message);
            }
        }

        void OnItemClick(object sender, int position)
        {
            try
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
                part = GetPart(ordersList[position].NumberTN);
                isEdit = false;
                CreateTimer();
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnItemClick:" + ex.Message);
            }
        }

        private string GetPart(string number)
        {
            try
            {
                return number.Replace("ПТМЦ №: ", "");
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - GetPart:" + ex.Message);
                return "";
            }
        }

        void OnPositionClick(object sender, int position)
        {
            try
            {
                pos = positionsList[position];
                pos.IsUpdate = true;
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
                if (pos.Rate == 0)
                    RunOnUiThread(() => quentityInput.Text = "");
                else
                    RunOnUiThread(() => quentityInput.Text = pos.Rate.ToString());
                RunOnUiThread(() => weightInput.Text = pos.Weight.ToString());
            }
            catch (Exception ex)
            {
                ErrorMessage("MainActivity - OnPositionClick:" + ex.Message);
            }
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Enter)
                SavePosData();
            return base.OnKeyUp(keyCode, e);
        }
    }
}