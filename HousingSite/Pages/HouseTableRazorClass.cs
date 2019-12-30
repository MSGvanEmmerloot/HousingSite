using HousingSite.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Pages
{
    public class HouseTableRazorClass : ComponentBase
    {
        [Inject]
        protected HouseTableService HouseService { get; set; }

        protected bool init = false;

        protected override async Task OnInitializedAsync()
        {
            await HouseService.RunAsync(0);
            init = true;
        }
    }
}
