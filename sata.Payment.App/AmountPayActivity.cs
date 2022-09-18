using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Ir.Sep.Android.Service;
using Java.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sata.Payment.App.Dto;
using sata.Payment.App.Resources;

namespace sata.Payment.App
{
    [Activity(Label = "AmountPayActivity")]
    public class AmountPayActivity : Activity
    {
        public IProxy ServiceProxy { get; set; }
        public static readonly string Tag = "ListPaymentActivity";
        public static readonly string ApiServiceAddress = "http://172.16.9.245:6060/api/services/app/";
        public bool IsBound { get; set; }
        public PaymentInfo PayInfo;
        private readonly WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
        private int verifyIndex;
        private ProxyConnection serviceConnection;
        protected override void OnStart()
        {
            base.OnStart();
            this.InitService();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.AmountPay);

            this.SetPayInfo();

            if (this.SetAmount(out var txtAmount))
            {
                return;
            }

            this.SetBtnPayClickEvent();

            this.SetBtnCancelClickEvent();

            this.SetTxtAmountTouchEvent(txtAmount);

            var imm = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            if (imm == null)
            {
                return;
            }

            imm.HideSoftInputFromWindow(txtAmount.WindowToken, 0);
            imm.HideSoftInputFromWindow(txtAmount.WindowToken, 0);
        }

        private void SetTxtAmountTouchEvent(EditText txtAmount)
        {
            txtAmount.Touch += delegate
            {
                var immNtlId = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                if (immNtlId == null)
                {
                    return;
                }

                immNtlId.HideSoftInputFromWindow(txtAmount.WindowToken, 0);
                immNtlId.HideSoftInputFromWindow(txtAmount.WindowToken, 0);
            };
        }

        private void SetBtnCancelClickEvent()
        {
            var btnCancel = this.FindViewById<Button>(Resource.Id.btnCancel);
            if (btnCancel != null)
            {
                btnCancel.Click += delegate
                {
                    var intent = new Intent(this, typeof(MainActivity));
                    this.StartActivity(intent);
                };
            }
        }

        private void SetBtnPayClickEvent()
        {
            var btnPay = this.FindViewById<Button>(Resource.Id.btnPay);
            if (btnPay != null)
            {
                btnPay.Click += this.PaymentClick;
            }
        }

        private bool SetAmount(out EditText txtAmount)
        {
            txtAmount = this.FindViewById<EditText>(Resource.Id.txtAmount);
            if (txtAmount == null)
            {
                return true;
            }

            txtAmount.TextChanged += this.TextChangeMoney;
            txtAmount.Text = this.PayInfo.ReturnAmount.ToString();
            return false;
        }

        private void SetPayInfo()
        {
            var intent = this.Intent;
            if (intent != null)
            {
                var paymentItem = intent.GetStringExtra("paymentItem");
                this.PayInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentItem);
            }
        }

        private void PaymentClick(object sender, EventArgs e)
        {
            try
            {
                var text = this.FindViewById<EditText>(Resource.Id.txtAmount)?.Text;
                if (text == null)
                {
                    return;
                }

                var amn = text.Replace(",", "");
                var txtError = this.FindViewById<TextView>(Resource.Id.txtError);

                if (int.Parse(amn) < 1000)
                {
                    if (txtError == null)
                    {
                        return;
                    }

                    txtError.Text = Resource_String.pay;
                    txtError.Visibility = ViewStates.Visible;
                }
                else if (int.Parse(amn) > this.PayInfo.ReturnAmount)
                {
                    if (txtError == null)
                    {
                        return;
                    }

                    txtError.Text = $"حداکثر مبلغ پرداختی {this.PayInfo.ReturnAmount} ریا ل می باشد";
                    txtError.Visibility = ViewStates.Visible;
                }
                else
                {
                    if (txtError != null)
                    {
                        txtError.Visibility = ViewStates.Gone;
                    }

                    var uuid = UUID.RandomUUID();
                    if (uuid == null)
                    {
                        return;
                    }

                    var randomUuidString = uuid.ToString();  //  *  اﯾﺠﺎد ﯾﮏ ﻣﻘﺪار ﯾﻮﻧﯿﮏ ﺗﻮﺳﻂ ﺑﺮﻧﺎﻣﻪ ﻣﺸﺘﺮي

                    var intent = this.CreateIntent(amn, randomUuidString);
                    base.StartActivityForResult(intent, 1);
                }
            }
            catch
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage("دوباره سعی کنید");
                builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                builder.Show();
            }
        }

        private Intent CreateIntent(string amn, string randomUuidString)
        {
            var intent = new Intent();
            intent.PutExtra("TransType", 1);
            intent.PutExtra("Amount", amn);
            intent.PutExtra("ResNum", randomUuidString);
            intent.PutExtra("AppId", "1");
            intent.SetComponent(new ComponentName("ir.sep.android.smartpos", "ir.sep.android.smartpos.ThirdPartyActivity"));
            intent.SetFlags(0);
            return intent;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                if (requestCode != 1)
                {
                    return;
                }

                if (resultCode == Result.Ok)
                {
                    var state = data.GetIntExtra("State", -1); // Response Code Switch                    
                    var refNum = data.GetStringExtra("RefNum"); // Reference number 

                    var resNum = data.GetStringExtra("ResNum");
                    if (refNum == "null")
                    {
                        return;
                    }

                    var builder = new AlertDialog.Builder(this);
                    var text = this.FindViewById<EditText>(Resource.Id.txtAmount)?.Text;
                    if (text == null)
                    {
                        return;
                    }

                    var amn = text.Replace(",", "");
                    if (refNum == null)
                    {
                        return;
                    }

                    var input = this.CreateInsertPatientInputDto(amn, refNum);

                    if (state == (int)TransactionState.Success)
                    {
                        this.ApplyTransaction(refNum, resNum, builder, input);
                    }
                    else
                    {
                        builder.SetMessage("ﺗﺮاﮐﻨﺶ ﻧﺎﻣﻮﻓﻖ!");
                        builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                        builder.Show();
                    }
                }
                else
                {

                    var resultMdg = data.GetStringExtra("result");
                    if (resultMdg != "ترمینال شامل تراکنش باز می باشد. لطفا مجددا امتحان کنید")
                    {
                        return;
                    }

                    var builderMsg = new AlertDialog.Builder(this);
                    builderMsg.SetMessage(" عدم تایید تراکنش از سوی بانک لطفا به صندوق مراجعه فرمایید");
                    builderMsg.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                    builderMsg.Show();

                }
            }
            catch (Exception)
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(" ﺗﺮاﮐﻨﺶ ﻧﺎﻣﻮﻓﻖ");
                builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                builder.Show();
            }
        }

        private InsertPatientInputDto CreateInsertPatientInputDto(string amn, string refNum)
        {
            var input = new InsertPatientInputDto
            {
                PatientId = this.PayInfo.PatientId,
                Amount = int.Parse(amn),
                RefNoOut = long.Parse(refNum),
                SwitchDateOut = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ChqNo = "0",
                TerminalId = this.PayInfo.TerminalId,
                TraceNoOut = "0",
                PaymentId = this.PayInfo.Id
            };
            return input;
        }

        private void ApplyTransaction(string refNum, string resNum, AlertDialog.Builder builder, InsertPatientInputDto input)
        {
            var verifyResult = this.ServiceProxy.VerifyTransaction(1, refNum, resNum);
            switch (verifyResult)
            {
                case (int)TransactionState.Success:
                    this.PrintReceipt(refNum);
                    this.ApplyService(refNum, builder, input);
                    break;
                case 1:
                    this.ServiceProxy.PrintByRefNum(refNum);
                    this.PrintReceipt(refNum);
                    this.ApplyService(refNum, builder, input);
                    break;
                case -1 when this.verifyIndex < 6:
                    this.verifyIndex++;
                    this.ApplyTransaction(refNum, resNum, builder, input);
                    break;
                case -1:
                    {
                        var builderMsg = new AlertDialog.Builder(this);
                        builderMsg.SetMessage(" عدم تایید تراکنش از سوی بانک لطفا به صندوق مراجعه فرمایید");
                        builderMsg.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                        builderMsg.Show();
                        break;
                    }
                case -2 when this.verifyIndex == 0:
                    {
                        var builderMsg = new AlertDialog.Builder(this);
                        builderMsg.SetMessage(" عدم تایید تراکنش از سوی بانک لطفا به صندوق مراجعه فرمایید");
                        builderMsg.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                        builderMsg.Show();
                        break;
                    }
                case -2:
                    {
                        var builderMsg = new AlertDialog.Builder(this);
                        builderMsg.SetMessage(" تراکنش با موفقیت انجام گردید");
                        builderMsg.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                        builderMsg.Show();
                        break;
                    }
            }
        }

        private void ApplyService(string refNum, AlertDialog.Builder builder, InsertPatientInputDto input)
        {
            var serializedObject = JsonConvert.SerializeObject(input);
            try
            {
                using var client = new HttpClient();
                if (!this.wifiManager.IsWifiEnabled)
                {
                    return;
                }

                client.BaseAddress = new Uri(ApiServiceAddress + "cshRgn/");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var contentPost = new StringContent(serializedObject, Encoding.UTF8, "application/json");

                var response = client.PostAsync("InsertPntCshAndroid", contentPost).Result;
                if (response.ReasonPhrase == "OK")
                {
                    var repStr = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JObject.Parse(repStr);
                    var dptList = JsonConvert.DeserializeObject<InsertPatientReturnDto>(apiResponse["result"].ToString());
                    if (dptList.MsgId == 1)
                    {
                        builder.SetMessage(" پرداخت انجام شد شماره پیگیری شما !" + refNum + "می باشد");
                        builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) =>
                        {
                            var intent = new Intent(this, typeof(MainActivity));
                            this.StartActivity(intent);
                        });
                        builder.Show();

                    }
                    else
                    {
                        builder.SetMessage(" عملیات پرداخت انجام شده است اما درسیستم ثبت نگردید لطفا به صندوق مراجعه نمایید");
                        builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                        builder.Show();
                    }
                }
                else
                {
                    builder.SetMessage(" عملیات پرداخت انجام شده است اما درسیستم ثبت نگردید لطفا به صندوق مراجعه نمایید");
                    builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                    builder.Show();

                }
            }
            catch
            {
                builder.SetMessage(" عملیات پرداخت انجام شده است اما درسیستم ثبت نگردید لطفا به صندوق مراجعه نمایید");
                builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
                builder.Show();
                var intent = new Intent(this, typeof(MainActivity));
                this.StartActivity(intent);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.ReleaseService();
        }

        private void InitService()
        {
            this.serviceConnection = new ProxyConnection(this);
            var additionServiceIntent = new Intent();
            additionServiceIntent.SetClassName("ir.sep.android.smartpos", "ir.sep.android.Service.Proxy");
            var applicationContext = this.ApplicationContext;
            bool ret = applicationContext != null && applicationContext.BindService(additionServiceIntent, this.serviceConnection, Bind.AutoCreate);
            Log.Debug(Tag, "initService() bound value:" + ret);
        }

        private void ReleaseService()
        {
            if (!this.IsBound)
            {
                return;
            }

            var applicationContext = this.ApplicationContext;
            applicationContext?.UnbindService(this.serviceConnection);

            this.IsBound = false;
            this.serviceConnection = null;
            Log.Debug(Tag, "Service released.");
        }

        public void TextChangeMoney(object sender, TextChangedEventArgs e)
        {
            var ed = (EditText)sender;
            if (e.BeforeCount < e.AfterCount)
            {
                BeforeCountLessThanAfterCount(ed);
            }
            else if ((e.BeforeCount > e.AfterCount) & !string.IsNullOrEmpty(ed.Text))
            {
                BeforeCountGratherThanAfterCount(ed);
            }
        }

        private static void BeforeCountGratherThanAfterCount(EditText ed)
        {
            var d = Convert.ToDecimal(ed.Text);
            var uiCulture = CultureInfo.CreateSpecificCulture("fa-IR");
            var str = d.ToString("c", uiCulture);
            ed.Text = str.Replace("ریال", "");
            ed.SetSelection(ed.Text.Length);
        }

        private static void BeforeCountLessThanAfterCount(EditText ed)
        {
            var d = Convert.ToDecimal(ed.Text);
            var uiCulture = CultureInfo.CreateSpecificCulture("fa-IR");
            var str = d.ToString("c", uiCulture);
            ed.Text = str.Replace("ریال", "");
            ed.SetSelection(ed.Text.Length);
        }

        public void PrintReceipt(string refNum)
        {
            var type = "نام خدمت: ";
            if (this.PayInfo.TypDes.Contains("پذیرش"))
            {
                var index = this.PayInfo.TypDes.IndexOf("پذیرش", StringComparison.Ordinal);
                type += this.PayInfo.TypDes.Substring(0, index);
                var num = "شماره " + this.PayInfo.TypDes.Substring(index);
                this.ServiceProxy.PrintByString(type);
                this.ServiceProxy.PrintByString(num);
            }
            else
            {
                type += this.PayInfo.TypDes;
                this.ServiceProxy.PrintByString(type);
            }
            var dt = "تاریخ پذیرش :" + this.PayInfo.AdmD;
            this.ServiceProxy.PrintByString(dt);
            var patientName = "نام بیمار : " + this.PayInfo.PatientName;
            this.ServiceProxy.PrintByString(patientName);
            var docName = "نام پزشک : " + this.PayInfo.DoctorName;
            this.ServiceProxy.PrintByString(docName);
            this.ServiceProxy.PrintByString("");
            this.ServiceProxy.PrintByString("");
            this.ServiceProxy.PrintByString("");
        }
        public override void OnBackPressed() { }
    }

    public enum TransactionState
    {
        Success
    }
}