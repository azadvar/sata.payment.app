using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace sata.Payment.App
{
    [Service]
    [IntentFilter(new[] { "ir.sep.android.Service.IProxy" })]
    public class Proxy : Service
    {
        private const string Tag = "Proxy";

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Debug(Tag, "Addition Service created.");
        }
        public override IBinder OnBind(Intent intent) => null;

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug(Tag, "Addition service stopped.");
        }
    }
}