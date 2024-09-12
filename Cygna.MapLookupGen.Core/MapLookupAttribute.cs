using System;

namespace Cygna.MapLookupGen;

public class MapLookupAttribute : Attribute
{
    public HashMethod HashMethod { get; }
    
    public MapLookupAttribute(HashMethod hashMethod = HashMethod.XxHash64)
    {
        HashMethod = hashMethod;
    }
}