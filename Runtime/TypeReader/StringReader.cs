namespace Voltstro.CommandLineParser.TypeReaders
{
	/// <summary>
	/// A default reader for <see cref="string"/>
	/// </summary>
	public class StringReader : ITypeReader
	{
		public object ReadType(string input)
		{
			return input;
		}
	}
}