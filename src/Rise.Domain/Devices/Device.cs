using Rise.Domain.Projects;

namespace Rise.Domain.Devices;

public class Device : Entity {
	private Device() { }

	public Device(string name, string serialNumber, DeviceType deviceType, string? brand = null,
		string? sonde = null, string? logger = null, string? modem = null, string? sim = null, string? voeding = null) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		DeviceType = deviceType;
		Brand = brand;
		Sonde = sonde;
		Logger = logger;
		Modem = modem;
		Sim = sim;
		Voeding = voeding;
	}

	public string Name { get; private set; } = default!;
	public string SerialNumber { get; private set; } = default!;
	public Status Status { get; } = Status.Actief;

	public DeviceType DeviceType { get; private set; }

	public string? Sonde { get; private set; }
	public string? Logger { get; private set; }
	public string? Modem { get; private set; }
	public string? Sim { get; private set; }
	public string? Voeding { get; private set; }
	public string? Brand { get; private set; }


	public void Edit(string name, string serialNumber, DeviceType deviceType, string? brand = null,
		string? sonde = null, string? logger = null, string? modem = null, string? sim = null, string? voeding = null) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Inactieve apparaten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		DeviceType = deviceType;
		Brand = brand;
		Sonde = sonde;
		Logger = logger;
		Modem = modem;
		Sim = sim;
		Voeding = voeding;
	}

	/// <summary>
	///     Fluent builder for device. Use Device.Builder.Create(type)
	///     and set required fields via WithX() and close with Build().
	///     Builder validates the fields and checks by DeviceType whether the associated
	///     component value is present.
	/// </summary>
	public sealed class Builder {
		private readonly DeviceType _deviceType;
		private string? _brand;
		private string? _logger;
		private string? _modem;
		private string? _name;
		private string? _serialNumber;
		private string? _sim;
		private string? _sonde;
		private string? _voeding;

		private Builder(DeviceType deviceType) {
			_deviceType = deviceType;
		}

		public static Builder Create(DeviceType deviceType) {
			return new Builder(deviceType);
		}

		public Builder WithName(string name) {
			_name = name;
			return this;
		}

		public Builder WithSerialNumber(string serialNumber) {
			_serialNumber = serialNumber;
			return this;
		}

		public Builder WithBrand(string? brand) {
			_brand = brand;
			return this;
		}

		public Builder WithSonde(string sonde) {
			_sonde = sonde;
			return this;
		}

		public Builder WithLogger(string logger) {
			_logger = logger;
			return this;
		}

		public Builder WithModem(string modem) {
			_modem = modem;
			return this;
		}

		public Builder WithSim(string sim) {
			_sim = sim;
			return this;
		}

		public Builder WithVoeding(string voeding) {
			_voeding = voeding;
			return this;
		}

		/// <summary>
		///     Valideer en bouw het Device-object.
		/// </summary>
		public Device Build() {
			// Basisvalidatie
			string name = Guard.Against.NullOrWhiteSpace(_name);
			string serial = Guard.Against.NullOrWhiteSpace(_serialNumber);

			// Per DeviceType extra validatie: zorg dat het corresponderende component is meegegeven
			switch (_deviceType) {
			case DeviceType.Sonde:
				Guard.Against.NullOrWhiteSpace(_sonde);
				break;
			case DeviceType.Logger:
				Guard.Against.NullOrWhiteSpace(_logger);
				break;
			case DeviceType.Modem:
				Guard.Against.NullOrWhiteSpace(_modem);
				break;
			case DeviceType.Sim:
				Guard.Against.NullOrWhiteSpace(_sim);
				break;
			case DeviceType.Voeding:
				Guard.Against.NullOrWhiteSpace(_voeding);
				break;
			}

			return new Device(name, serial, _deviceType, _brand, _sonde, _logger, _modem, _sim, _voeding);
		}
	}
}