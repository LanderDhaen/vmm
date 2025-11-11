using MudBlazor;

namespace Rise.Client.Services;

public class BreadcrumbService {
	private readonly List<BreadcrumbItem> _items = new();

	public IReadOnlyList<BreadcrumbItem> Items => _items.AsReadOnly();
	public event Action? OnChanged;

	public void Set(params BreadcrumbItem[] items) {
		_items.Clear();
		_items.AddRange(items);
		OnChanged?.Invoke();
	}

	public void Clear() {
		_items.Clear();
		OnChanged?.Invoke();
	}
}