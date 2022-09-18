using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ir.Sep.Android.Service;

namespace sata.Payment.App
{
    public class ProxyBinder : IProxyStub, IProxy
    {
        public static readonly string Tag = "ProxyBinder";
        //public override int Add(int value1, int value2)
        //{
        //    Log.Debug(Tag, "AdditionService.Add({0}, {1})", value1, value2);
        //    return value1 + value2;
        //}

        public override bool Print(string refNum)
        {
            throw new NotImplementedException();
        }
        

        public override bool PrintByString(string @string)
        {
            return PrintByString(@string);
        }

        public override int ReverseTransaction(int appId, string refNum)
        {
            throw new NotImplementedException();
        }

        public override int VerifyTransaction(int appId, string refNum)
        {
            throw new NotImplementedException();
        }
    }
}