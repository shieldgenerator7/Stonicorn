using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_SavableObject : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		SavableObject data = (SavableObject)obj;
		// Add your writer.Write calls here.
		writer.Write(data.data);
		writer.Write(data.isSpawnedObject);
		writer.Write(data.isSpawnedScript);
		writer.Write(data.scriptType);
		writer.Write(data.prefabName);
		writer.Write(data.spawnTag);

	}
	
	public override object Read(ES2Reader reader)
	{
		SavableObject data = new SavableObject();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		SavableObject data = (SavableObject)c;
		// Add your reader.Read calls here to read the data into the object.
		data.data = reader.ReadDictionary<System.String,System.Object>();
		data.isSpawnedObject = reader.Read<System.Boolean>();
		data.isSpawnedScript = reader.Read<System.Boolean>();
		data.scriptType = reader.Read<System.String>();
		data.prefabName = reader.Read<System.String>();
		data.spawnTag = reader.Read<System.String>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_SavableObject():base(typeof(SavableObject)){}
}