namespace Rise.Domain.MaintObjects;

/// <summary>
///     MaintObj types for maintenance objects.
/// </summary>

//TODO: uitbreiden naar tabel in db + onderhoudsplan
public enum MaintObjType {
	Sonde,
	Logger,
	Modem,
	Sim,
	Voeding

	//TODO: onderhoudsplan: installedAt.AddDays(X) -> moet aanpasbaar zijn per maintobjtype
	// x -> cyclus 
}