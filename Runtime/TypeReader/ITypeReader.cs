namespace Voltstro.CommandLineParser.TypeReaders
{
	public interface ITypeReader
	{
		object ReadType(string input);
	}
}