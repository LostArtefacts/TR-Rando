namespace TRLevelControl;

public abstract class TRMapControlBase<TKey, TAlias>
    where TKey : Enum
    where TAlias : Enum
{
    private const char _separator = '=';

    public Dictionary<TKey, TAlias> Read(string filePath)
    {
        using StreamReader reader = new(File.OpenRead(filePath));
        return Read(reader);
    }

    public Dictionary<TKey, TAlias> Read(StreamReader reader)
    {
        Dictionary<TKey, TAlias> map = new();
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] parts = line.Split(_separator);
            if (parts.Length < 2)
            {
                continue;
            }

            map[ReadKey(parts[0])] = (TAlias)Enum.Parse(typeof(TAlias), parts[1], true);
        }

        return map;
    }

    public void Write(Dictionary<TKey, TAlias> data, string filePath)
    {
        using StreamWriter writer = new(File.Create(filePath));
        Write(data, writer);
    }

    public void Write(Dictionary<TKey, TAlias> data, StreamWriter writer)
    {
        foreach (var (key, value) in data)
        {
            writer.Write(ConvertKey(key).ToUpper());
            writer.Write(_separator);
            writer.WriteLine(value.ToString().ToUpper());
        }

        writer.Flush();
    }

    protected abstract TKey ReadKey(string key);
    protected abstract string ConvertKey(TKey key);
}
