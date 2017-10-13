using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_GameState : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		GameState data = (GameState)obj;
		// Add your writer.Write calls here.
		writer.Write(data.id);
		writer.Write(data.states);
		writer.Write(data.merky);
		writer.Write(data.openScenes);

	}
	
	public override object Read(ES2Reader reader)
	{
		GameState data = new GameState();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		GameState data = (GameState)c;
		// Add your reader.Read calls here to read the data into the object.
		data.id = reader.Read<System.Int32>();
		data.states = reader.ReadList<ObjectState>();
		data.merky = reader.Read<ObjectState>();
		data.openScenes = reader.ReadList<System.String>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_GameState():base(typeof(GameState)){}
}