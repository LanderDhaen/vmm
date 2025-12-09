using System.Data;

using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	
	// History

	private readonly List<MaintObjHistory> _maintObjHistories = [];
	
	// Constructors
	
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, MaintObjPlan maintObjPlan, string? brand, Location location) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = Guard.Against.Null(maintObjType);
		MaintObjPlan = Guard.Against.Null(maintObjPlan);
		Brand = brand;
		Location = Guard.Against.Null(location);
	}
	
	// Properties

	public string Name { get; private set; }
	public string SerialNumber { get; private set; }
	public string? Brand { get; private set; }

	public MaintObjType MaintObjType { get; private set; }
	public MaintObjStatus MaintObjStatus { get; private set; } = MaintObjStatus.Inactive;
	public MaintObjPlan MaintObjPlan { get; private set; }
	public Installation? Installation { get; private set; }

	public MeasurementStation? Station { get; private set; }
	public Location Location { get; private set; }

	public Technician? LastScannedBy { get; private set; }
	
	public IReadOnlyList<MaintObjHistory> MaintObjHistories => _maintObjHistories.AsReadOnly();
	
	// Functions

	public void AssignToStation(MeasurementStation station) {
		
		// Check if the values are the same as the old values

		if (station.Equals(Station)) {
			return;
		}
		
		// Check if the values are allowed

		Guard.Against.Null(station);
		
		// Create history object using the old values
		
		AddHistoryEntry();
		
		// Assign the new values
		
		Station = station;
		Installation = new Installation(Station.Installation.InstalledAt, Station.Installation.RemovedAt);
		Location = station.Location;
		SetStatus(MaintObjStatus.Active);
	}
	
	public void MoveToStockLocation(Location location) {
		
		// Check if the values are the same as the old values
		
		if (Location.Equals(location)) {
			return;
		}
		
		// Check if the values are allowed
		
		Guard.Against.Null(location);

		if (!location.IsStockLocation) {
			throw new ArgumentException("Dit is geen stocklocatie");
		}
		
		// Create history object using old values
		
		AddHistoryEntry();
		
		// Assign the new values
		
		Location = location;
		Installation = null;
		Station = null;
		SetStatus(MaintObjStatus.Inactive);
	}

	public void ScanBy(Technician technician) {
		
		// Check if the values are the same as the old values

		if (technician.Equals(LastScannedBy)) {
			return;
		}
		
		// Check if the values are allowed
		
		Guard.Against.Null(technician);
		
		// Create history object using old values
		
		AddHistoryEntry();
		
		// Assign the new values
		
		LastScannedBy = technician;
	}

	public void MarkAsBroken() {
		if (MaintObjStatus == MaintObjStatus.Broken) {
			return;
		}
		
		AddHistoryEntry();
		
		SetStatus(MaintObjStatus.Broken);
	}

	public void MarkAsFixed() {
		if (MaintObjStatus != MaintObjStatus.Broken) {
			return;
		}
		
		AddHistoryEntry();
		SetStatus(MaintObjStatus.Inactive);
	}

	private void SetStatus(MaintObjStatus status) {
		MaintObjStatus = Guard.Against.Null(status);
	}

	public void Edit(string name, string serialNumber, MaintObjPlan maintObjPlan, string? brand) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjPlan = maintObjPlan;
		Brand = brand;
	}
	
	// History

	private void AddHistoryEntry() {
		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		Installation? installation = Installation is null ? null : new Installation(Installation.InstalledAt, Installation.RemovedAt);
		
		MaintObjHistory maintObjHistory = new(MaintObjStatus, 
			MaintObjPlan,
			installation,
			Station,  
			Location, 
			LastScannedBy,  
			validityRange); 
		
		_maintObjHistories.Add(maintObjHistory);
	}

	public List<MaintObjModification> GetModifications() {
		List<MaintObjModification> modifications = [];
		
		_maintObjHistories.Insert(0, new MaintObjHistory(MaintObjStatus, MaintObjPlan, Installation, Station, Location, LastScannedBy, new ValidityRange(UpdatedAt, DateTime.UtcNow)));

		for (int i = 0; i < _maintObjHistories.Count - 1; i++) {
			var current =  _maintObjHistories[i];
			var previous = _maintObjHistories[i + 1];
			
			List<MaintObjModification> modificationsPerPair = MaintObjModificationFactory.Create(current, previous);
			modifications.AddRange(modificationsPerPair);
		}
		
		return modifications;
	}
	
}

/// <summary>
///     A maintenance object history is something that holds a past state of a maintenance object.
/// </summary>
public class MaintObjHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MaintObjHistory() { }

	public MaintObjHistory(MaintObjStatus maintObjStatus, 
		MaintObjPlan maintObjPlan, 
		Installation? installation, 
		MeasurementStation? station, 
		Location location, 
		Technician? lastScannedBy,
		ValidityRange validityRange) {
		
		MaintObjStatus = Guard.Against.Null(maintObjStatus);
		MaintObjPlan = Guard.Against.Null(maintObjPlan);
		Installation = installation;
		Station = station;
		Location = location;
		LastScannedBy = lastScannedBy;
		ValidityRange = validityRange;

	}
	
	public MaintObjStatus MaintObjStatus { get; init; }
	public MaintObjPlan MaintObjPlan { get; init; }
	public Installation? Installation { get; init; }
	public MeasurementStation? Station { get; init; }
	public Location Location { get; init; }
	public Technician? LastScannedBy { get; init; }
}