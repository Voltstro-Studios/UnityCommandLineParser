namespace Voltstro.CommandLineParser.TypeReaders
{
	public class StringReader : ITypeReader
	{
		public object ReadType(string input)
		{
			return input;
		}
	}
}