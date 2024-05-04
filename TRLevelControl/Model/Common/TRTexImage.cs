namespace TRLevelControl.Model;

public class TRTexImage<T>
{
    public T[] Pixels { get; set; }
}

public class TRTexImage8 : TRTexImage<byte> { }
public class TRTexImage16 : TRTexImage<ushort> { }
public class TRTexImage32 : TRTexImage<uint> { }
