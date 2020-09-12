using System.Globalization;

namespace Voltstro.CommandLineParser.TypeReaders
{
	public class FloatReader : ITypeReader
	{
		public object ReadType(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return 0f;

			return float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)
				? result
				: 0f;
		}
	}
}