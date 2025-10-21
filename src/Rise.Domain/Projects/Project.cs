namespace Rise.Domain.Projects;

/// <summary>
/// A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity
{
    
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = Guard.Against.NullOrEmpty(value);
    }
    public Address Location { get; init; } = default!;

    /// <summary>
    /// Entity Framework Core Constructor
    /// </summary>
    private Project()
    {
        
    }

    public Project(string name, Address location)
    {
        Name = name;
        Location = Guard.Against.Null(location);
    }

    public void Edit(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);       
    }
}