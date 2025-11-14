namespace Rise.Client.Services;

public sealed class HeaderService {
	public HeaderState State { get; private set; } = new("");

	public event Action? OnChange;

	public void Set(string title, string? icon = null, Func<Task>? onBack = null) {
		State = new HeaderState(title, icon, onBack);
		OnChange?.Invoke();
	}

	public sealed record HeaderState(string Title, string? Icon = null, Func<Task>? OnBack = null);
}