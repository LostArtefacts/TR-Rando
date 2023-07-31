namespace TRRandomizerCore.Randomizers;

public interface ISecretRandomizer
{
    IEnumerable<string> GetAuthors();
}
