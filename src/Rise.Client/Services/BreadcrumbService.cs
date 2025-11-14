using MudBlazor;

namespace Rise.Client.Services;

public class BreadcrumbService {
	private List<BreadcrumbItem> _items = new();

	public IReadOnlyList<BreadcrumbItem> Items => _items.AsReadOnly();

	public event Action? OnChange;

	public void SetBreadcrumbs(params BreadcrumbItem[] items) {
		_items = new List<BreadcrumbItem>(items);
		NotifyStateChanged();
	}

	public void SetBreadcrumbs(IEnumerable<BreadcrumbItem> items) {
		_items = new List<BreadcrumbItem>(items);
		NotifyStateChanged();
	}

	public void Clear() {
		_items.Clear();
		NotifyStateChanged();
	}

	private void NotifyStateChanged() {
		OnChange?.Invoke();
	}
}