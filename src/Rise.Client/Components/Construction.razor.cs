using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class Construction : ComponentBase {
	private void GoHome() {
		Navigation.NavigateTo("/");
	}
}