using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Views;
using Android.Widget;
using sata.Payment.App.Dto;

namespace sata.Payment.App
{
    public class PaymentAdapter : BaseAdapter<PaymentInfo>
    {
        private readonly List<PaymentInfo> items;
        private readonly Activity context;
        private PaymentInfo item;

        public PaymentAdapter(Activity context, List<PaymentInfo> items)
        {
            this.context = context;
            this.items = items;
        }
        public override PaymentInfo this[int position] => this.items[position];

        public override int Count => this.items.Count;

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            this.item = this.items[position];
            var cultureInfo = new CultureInfo("en-US");

            var contextLayoutInflater = this.context.LayoutInflater;
            if (contextLayoutInflater == null)
            {
                return null;
            }

            var view = convertView ?? contextLayoutInflater.Inflate(Resource.Layout.PaymentInfoCard, null);
            var txtPntName = view?.FindViewById<TextView>(Resource.Id.txtPntName);
            if (txtPntName != null)
            {
                txtPntName.Text = "نام بیمار: " + this.item.PatientName;
            }

            if (view != null)
            {
                var txtPhyName = view.FindViewById<TextView>(Resource.Id.txtPhyName);
                var patientName = "";
                if (this.item.DoctorName != null)
                {
                    patientName = this.item.DoctorName;
                }

                if (txtPhyName != null)
                {
                    txtPhyName.Text = "نام پزشک : " + patientName;
                }
            }

            var txtTitle = view?.FindViewById<TextView>(Resource.Id.txtTlt);
            if (txtTitle != null)
            {
                txtTitle.Text = this.item.TypDes;
            }

            var txtDate = view?.FindViewById<TextView>(Resource.Id.txtDate);
            if (txtDate != null)
            {
                txtDate.Text = "تاریخ :" + this.item.AdmD;
            }

            var txtPntAmn = view?.FindViewById<TextView>(Resource.Id.txtPntamn);
            if (txtPntAmn != null)
            {
                txtPntAmn.Text = "سهم بیمار  :" + string.Format(cultureInfo, "{0:C}", this.item.PaymentAmount.ToString());
            }

            var txtRmnAmn = view?.FindViewById<TextView>(Resource.Id.txtRmnamn);
            if (txtRmnAmn != null)
            {
                txtRmnAmn.Text = "مبلغ قابل پرداخت : " + $"{this.item.ReturnAmount}";
            }

            return view;

        }
    }
}