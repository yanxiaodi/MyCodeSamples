using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesignBlazorDemo.Models;
using AntDesignBlazorDemo.Services;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace AntDesignBlazorDemo.Pages.List
{
    public partial class CardList
    {
        private readonly ListGridType _listGridType = new ListGridType
        {
            Gutter = 24,
            Column = 4
        };

        private ListItemDataType[] _data = { };

        [Inject] protected IProjectService ProjectService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var list = new List<ListItemDataType> {new ListItemDataType()};
            var data = await ProjectService.GetFakeListAsync(8);
            list.AddRange(data);
            _data = list.ToArray();
        }
    }
}