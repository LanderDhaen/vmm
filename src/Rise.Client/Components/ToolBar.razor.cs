using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Layout {
	public partial class ToolBar : ComponentBase {
		private bool _open;
		private Anchor _anchor;
		private string _width, _height;
		private void OpenActionsDrawer(Anchor anchor) {
			_open = true;
			_anchor = anchor;
			_width = "100%";
			_height = "350px";


		}
	}
}