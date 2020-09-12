using System.Globalization;

public class IntReader : ITypeReader
{
	public object ReadType(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return 0;

		return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : 0;
	}
}