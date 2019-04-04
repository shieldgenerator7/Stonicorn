using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_MemoryObject : ES2Type
{
    public override void Write(object obj, ES2Writer writer)
    {
        MemoryObject data = (MemoryObject)obj;
        // Add your writer.Write calls here.
        writer.Write(data.found);
    }

    public override object Read(ES2Reader reader)
    {
        MemoryObject data = new MemoryObject();
        Read(reader, data);
        return data;
    }

    public override void Read(ES2Reader reader, object c)
    {
        MemoryObject data = (MemoryObject)c;
        // Add your reader.Read calls here to read the data into the object.
        data.found = reader.Read<System.Boolean>();
    }

    /* ! Don't modify anything below this line ! */
    public ES2UserType_MemoryObject() : base(typeof(MemoryObject)) { }
}