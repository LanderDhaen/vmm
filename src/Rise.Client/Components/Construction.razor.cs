using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class Construction : ComponentBase {
	void GoHome()
	{
		Navigation.NavigateTo("/");
	}
}