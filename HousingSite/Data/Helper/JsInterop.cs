using HousingSite.Pages;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Data.Helper//HousingSite.Pages
{
    public class JsInterop
    {
        protected string resultString;
        public string sessionKey { get; private set; }
        protected IJSRuntime JSRuntime { get; set; }
        protected DotNetObjectReference<JsInterop> dotNetObjectRef;
        protected HouseMapClass houseMapClass;

        public JsInterop(IJSRuntime IJSRuntime)
        {
            JSRuntime = IJSRuntime;
            dotNetObjectRef = DotNetObjectReference.Create(this);
            JSRuntime.InvokeAsync<Task>("SetDotNetHelper", dotNetObjectRef);
        }

        public JsInterop(IJSRuntime IJSRuntime, HouseMapClass mapClass)
        {
            JSRuntime = IJSRuntime;
            houseMapClass = mapClass;
            dotNetObjectRef = DotNetObjectReference.Create(this);
            JSRuntime.InvokeAsync<Task>("SetDotNetHelper", dotNetObjectRef);
        }

        [JSInvokable]
        public void FromJS(string s)
        {
            Console.WriteLine("Received " + s);
            resultString = s;
        }

        [JSInvokable]
        public void SetSessionKey(string s)
        {
            Console.WriteLine("Received " + s);
            sessionKey = s;
            if(houseMapClass != null)
            {
                houseMapClass.SetSessionKey(sessionKey);
            }            
        }

        public async Task<string> CallFunction1()
        {
            await JSRuntime.InvokeAsync<Task>("FromNet", "Hello");
            //await JSRuntime.InvokeAsync<Task>("FromNet", dotNetObjectRef, "Hello");
            return resultString;
        }
        public async Task<string> CallFunction2()
        {
            await JSRuntime.InvokeAsync<Task>("FromNet2", dotNetObjectRef, "Hello again");
            return resultString;
        }

        public async Task<string> GetSessionKey()
        {
            await JSRuntime.InvokeAsync<string>("GetSessionKey");
            return sessionKey;
        }
    }
}
