namespace Rise.Domain.Products;

public class Product : Entity {
	private string _description = string.Empty;
	private string _name = string.Empty;

	public required string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	public string Description {
		get => _description;
		set => _description = Guard.Against.NullOrWhiteSpace(value);
	}
}