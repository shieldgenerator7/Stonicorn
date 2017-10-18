using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_SceneLoader : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		SceneLoader data = (SceneLoader)obj;
		// Add your writer.Write calls here.
		writer.Write(data.sceneName);
		writer.Write(data.lastOpenGameStateId);

	}
	
	public override object Read(ES2Reader reader)
	{
		SceneLoader data = GetOrCreate<SceneLoader>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		SceneLoader data = (SceneLoader)c;
		// Add your reader.Read calls here to read the data into the object.
		data.sceneName = reader.Read<System.String>();
		data.lastOpenGameStateId = reader.Read<System.Int32>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_SceneLoader():base(typeof(SceneLoader)){}
}