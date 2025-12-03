
	public class FilterSpec {
		public string? Operator { get; }
		public string? Value { get; }

		public FilterSpec(string? op, string? val) {
			Operator = op;
			Value = val;
		}
	}