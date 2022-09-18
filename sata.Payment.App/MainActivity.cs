using System;
using System.Collections.Generic;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Ir.Sep.Android.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sata.Payment.App.Dto;

namespace sata.Payment.App
{
    [Activity(Label = "سیستم پرداخت ساتا ", MainLauncher = true)]
    [Obsolete("Obsolete")]
    public class MainActivity : Activity
    {
        private readonly IProxy serviceProxy;
        public static readonly string Tag = "MainActivity";
        private ProxyConnection serviceConnection;
        public static readonly string ApiSrvAddress = "http://172.16.9.245:6060/api/services/app/";
        private Button btnShow;
        public bool IsBound { get; set; }
        private string macAddress;
        private ProgressDialog mDialog;

        private string wlan0 = "wlan0";

        public MainActivity(IProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;
        }

        public MainActivity()
        {
            
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);

            this.macAddress = this.GetMacAddress();

            var nationalCode = this.FindViewById<EditText>(Resource.Id.txtNationalCode);
            if (nationalCode != null)
            {
                nationalCode.AfterTextChanged += this.TxtNtlIdChanged;
                nationalCode.Text = "";
                nationalCode.Touch += delegate
                {
                    var immNationalCode = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                    if (immNationalCode == null)
                    {
                        return;
                    }

                    immNationalCode.HideSoftInputFromWindow(nationalCode.WindowToken, 0);
                    immNationalCode.HideSoftInputFromWindow(nationalCode.WindowToken, 0);
                };

                var immCode = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                if (immCode != null)
                {
                    immCode.HideSoftInputFromWindow(nationalCode.WindowToken, 0);
                    immCode.HideSoftInputFromWindow(nationalCode.WindowToken, 0);
                }
            }

            this.btnShow = this.FindViewById<Button>(Resource.Id.btnShow);
            var button = this.btnShow;
            if (button != null)
            {
                button.Click += this.BtnShowClick;
            }
        }

        private void BtnShowClick(object sender, EventArgs e)
        {
            this.LoadInfo();
        }

        private void TxtNtlIdChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty((sender as EditText)?.Text))
                {
                    return;
                }

                var text = ((EditText)sender).Text;
                if (!(text is { Length: 10 }))
                {
                    return;
                }

                if (IsNationalCodeValid(sender))
                {
                    this.btnShow.Visibility = ViewStates.Visible;
                }
                else
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage("کد ملی معتبر نمی باشد!");
                    builder.SetPositiveButton("متوجه شدم", (mainActivity, args) => { });
                    builder.Show();
                    (sender as EditText).Text = "";
                    this.btnShow.Visibility = ViewStates.Gone;
                }
            }
            catch
            {
                // ignored
            }
        }

        private static bool IsNationalCodeValid(object sender)
        {
            if ((sender as EditText)?.Text == "1111111111" ||
                (sender as EditText)?.Text == "0000000000" ||
                (sender as EditText)?.Text == "2222222222" ||
                (sender as EditText)?.Text == "3333333333" ||
                (sender as EditText)?.Text == "4444444444" ||
                (sender as EditText)?.Text == "5555555555" ||
                (sender as EditText)?.Text == "6666666666" ||
                (sender as EditText)?.Text == "7777777777" ||
                (sender as EditText)?.Text == "8888888888" ||
                (sender as EditText)?.Text == "9999999999")
            {
                return false;
            }
            var c = int.Parse((sender as EditText)?.Text[9].ToString());
            var n = (int.Parse((sender as EditText).Text[0].ToString()) * 10) +
                (int.Parse((sender as EditText).Text[1].ToString()) * 9) +
                (int.Parse((sender as EditText).Text[2].ToString()) * 8) +
                (int.Parse((sender as EditText).Text[3].ToString()) * 7) +
                (int.Parse((sender as EditText).Text[4].ToString()) * 6) +
                (int.Parse((sender as EditText).Text[5].ToString()) * 5) +
                (int.Parse((sender as EditText).Text[6].ToString()) * 4) +
                (int.Parse((sender as EditText).Text[7].ToString()) * 3) + (int.Parse((sender as EditText).Text[8].ToString()) * 2);
            var r = n - ((n / 11) * 11);
            return (r == 0 && r == c) || (r == 1 && c == 1) || (r > 1 && c == 11 - r);
        }



        private void LoadInfo()
        {
            try
            {
                this.ShowLoadingMessage();

                if (!(this.FindViewById<EditText>(Resource.Id.txtNationalCode) is { } txtNationalCode))
                {
                    return;
                }

                var input = this.CreatePatientInfoCashInputDto(txtNationalCode);

                var response = CreateHttpResponse(input);
                if (response.ReasonPhrase == "OK")
                {
                    this.mDialog.Dismiss();
                    var repStr = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JObject.Parse(repStr);
                    JsonConvert.DeserializeObject<List<LoadPatientReturn>>(apiResponse["result"].ToString());
                }
                else
                {
                    this.ShowErrorDialog();
                }
            }
            catch
            {
                this.ShowErrorDialog();
                this.mDialog.Dismiss();
            }
        }

        private void ShowErrorDialog()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage("خطا در بارگذاری اطلاعات!");
            builder.SetPositiveButton("متوجه شدم", (MainActivity, args) => { });
            builder.Show();
        }

        private static HttpResponseMessage CreateHttpResponse(PatientInfoCashInputDto input)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(ApiSrvAddress + "cshRgn/");
            client.DefaultRequestHeaders.Accept.Clear();
            var serializedObject = JsonConvert.SerializeObject(input);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var contentPost = new StringContent(serializedObject, Encoding.UTF8, "application/json");

            var response = client.PostAsync("loadpntcshAndroid ", contentPost).Result;
            return response;
        }

        private PatientInfoCashInputDto CreatePatientInfoCashInputDto(EditText txtNationalCode)
        {
            var input = new PatientInfoCashInputDto
            {
                Date = "",
                MacAddress = this.macAddress,
                PatientId = txtNationalCode.Text
            };
            return input;
        }

        private void ShowLoadingMessage()
        {
            this.mDialog = new ProgressDialog(this);
            this.mDialog.SetMessage("لطفا منتظر بمانید...");
            this.mDialog.SetCancelable(false);
            this.mDialog.Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.ReleaseService();
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


        private string GetMacAddress()
        {
            var all = NetworkInterface.GetAllNetworkInterfaces();
            var res = "";
            foreach (var item in all)
            {
                if (item.Name != this.wlan0)
                {
                    continue;
                }

                var address = item.GetPhysicalAddress();
                var macBytes = address.GetAddressBytes();

                var sb = new StringBuilder();
                foreach (var b in macBytes)
                {
                    sb.Append((b & 0xFF).ToString("X2") + ":");
                }

                res = sb.ToString().Remove(sb.Length - 1);
            }
            return res;
        }

        public override void OnBackPressed()
        { }
    }
}