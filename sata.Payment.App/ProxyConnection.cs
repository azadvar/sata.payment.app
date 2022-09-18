using Android.Content;
using Android.OS;
using Ir.Sep.Android.Service;

namespace sata.Payment.App
{
    public class ProxyConnection : Java.Lang.Object, IServiceConnection
    {
        private readonly AmountPayActivity activity;
        public ProxyConnection(AmountPayActivity activity)
        {
            this.activity = activity;
        }

        public IProxy Service
        {
            get; private set;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            this.Service = IProxyStub.AsInterface(service);
            this.activity.ServiceProxy = this.Service;
            this.activity.IsBound = this.Service != null;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            this.Service = null;
            this.activity.ServiceProxy = null;
            this.activity.IsBound = false;
        }
    }
}