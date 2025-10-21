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
    
    private string _code = string.Empty;

    public string Code
    {
        get => _code;
        set => _code = Guard.Against.NullOrEmpty(value);
    }
    
    private DateTime _startDate;

    public DateTime StartDate
    {
        get => _startDate;
        set => _startDate = Guard.Against.Null(value);
    }
    
    private  DateTime _endDate;

    public DateTime EndDate
    {
        get => _endDate;
        set => _endDate = Guard.Against.Null(value);    
    }
    
    /// <summary>
    /// The address is immutable and owned by the <see cref="Project"/>. If you want to change the address, create a new Address and link it to the project.
    /// <see cref="ProjectConfiguration"/>
    /// </summary>
    public Address Location { get; init; } = default!;

    /// <summary>
    /// Entity Framework Core Constructor
    /// </summary>
    private Project()
    {
        
    }

    public Project(string name, string code, DateTime startDate, DateTime endDate, Address location)
    {
        Name = name;
        Code = code;
        StartDate = startDate;
        EndDate = endDate;
        Location = Guard.Against.Null(location);
    }

    public void Edit(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }
}