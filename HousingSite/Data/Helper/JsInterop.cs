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
        protected IJSRuntime JSRuntime { get; set; }
        protected DotNetObjectReference<JsInterop> dotNetObjectRef;

        public JsInterop(IJSRuntime IJSRuntime)
        {
            JSRuntime = IJSRuntime;
            dotNetObjectRef = DotNetObjectReference.Create(this);
            JSRuntime.InvokeAsync<Task>("SetDotNetHelper", dotNetObjectRef);
        }

        [JSInvokable]
        public void FromJS(string s)
        {
            Console.WriteLine("Received " + s);
            resultString = s;
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
    }
}
