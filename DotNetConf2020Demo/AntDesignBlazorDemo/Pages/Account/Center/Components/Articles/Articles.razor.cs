using System.Collections.Generic;
using AntDesignBlazorDemo.Models;
using Microsoft.AspNetCore.Components;

namespace AntDesignBlazorDemo.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}