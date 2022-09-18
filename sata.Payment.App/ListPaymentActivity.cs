using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using sata.Payment.App.Dto;

namespace sata.Payment.App
{
    [Activity(Label = "ListPaymentActivity")]
    public class ListPaymentActivity : Activity
    {
        public List<PaymentInfo> PayList;
        public PaymentInfo SelectedItem;
       
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.ListPayment);
            var intent = this.Intent;
            if (intent != null)
            {
                var intentList = intent.GetStringExtra("DptList");
                var patientsInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LoadPatientReturn>>(intentList);
                this.PayList = new List<PaymentInfo>();
                var index = 0;
                foreach (var item in patientsInfo)
                {
                    index++;
                    var payItem = CreatePaymentInfo(index, item);
                    this.PayList.Add(payItem);
                }
            }

            var liPayment = this.FindViewById<ListView>(Resource.Id.liPayment);

            if (liPayment != null)
            {
                liPayment.Adapter = new PaymentAdapter(this, this.PayList);
                liPayment.ChoiceMode = ChoiceMode.Single;
                liPayment.ItemClick += this.LiPaymentItemClick;
            }

            var btnBack = this.FindViewById<Button>(Resource.Id.btnBack);
            if (btnBack != null)
            {
                btnBack.Click += delegate
                {
                    var intent = new Intent(this, typeof(MainActivity));
                    this.StartActivity(intent);
                };
            }
        }

        private static PaymentInfo CreatePaymentInfo(int index, LoadPatientReturn item)
        {
            var payItem = new PaymentInfo
            {
                Row = index,
                Id = item.Id,
                AdmD = item.AdmD,
                PaymentAmount = item.PaymentAmount,
                ReturnAmount = item.ReturnAmount,
                TypDes = item.TypDes,
                TerminalId = item.TerminalId,
                PatientId = item.PatientId,
                PatientName = item.PatientFirstName + " " + item.PatientLastName,
                DoctorName = item.PatientNumber
            };
            return payItem;
        }

        private void LiPaymentItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                this.SelectedItem = this.PayList[e.Position];
                var intent = new Intent(this, typeof(AmountPayActivity));
                intent.PutExtra("paymentItem", Newtonsoft.Json.JsonConvert.SerializeObject(this.SelectedItem));
                this.StartActivity(intent);
            }
            catch
            {
                this.ShowErrorMessage("\"خطا در لود اطلاعات!\"");
            }
        }

        private void ShowErrorMessage(string error)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(error);
            builder.SetPositiveButton("متوجه شدم", (appointmentTimeListActivity, args) => { });
            builder.Show();
        }

        public override void OnBackPressed()
        { }
    }
}
    