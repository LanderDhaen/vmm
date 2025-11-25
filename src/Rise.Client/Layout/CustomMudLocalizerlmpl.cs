using Microsoft.Extensions.Localization;

using MudBlazor;

namespace Rise.Client.Layout;

internal class DictionaryMudLocalizer : MudLocalizer {
	private readonly Dictionary<string, string> _localization;

	public DictionaryMudLocalizer() {
		_localization = new()
		{

		{ "MudDataGrid_Contains", "Bevat" },
			{ "MudDataGrid_NotContains", "Bevat niet" },
			{ "MudDataGrid_StartsWith", "Begint met" },
			{ "MudDataGrid_EndsWith", "Eindigt op" },
			{ "MudDataGrid_IsEmpty", "Is leeg" },
			{ "MudDataGrid_IsNotEmpty", "Is niet leeg" },
			{ "MudDataGrid_Equals", "Is gelijk aan" },
			{ "MudDataGrid_NotEquals", "Is niet gelijk aan" },
			{ "MudDataGrid_GreaterThan", "Groter dan" },
			{ "MudDataGrid_GreaterThanOrEqual", "Groter dan of gelijk aan" },
			{ "MudDataGrid_LessThan", "Kleiner dan" },
			{ "LessThanOrEqual", "Kleiner dan of gelijk aan" },
		};
	}

	public override LocalizedString this[string key] {
		get {
			var currentCulture = Thread.CurrentThread.CurrentUICulture.Parent.TwoLetterISOLanguageName;
			if (currentCulture.Equals("be", StringComparison.InvariantCultureIgnoreCase)
				&& _localization.TryGetValue(key, out var res)) {
				return new(key, res);
			} else {
				return new(key, key, true);
			}
		}
	}
}