using System;

namespace Sandbox.sprite;

public class SpriteSizer : Component
{
	[Property] private int PixelPerUnit { get; set; } = 32;
	[Property] private int _width = 32;
	[Property] private int _height = 32;

	protected override void OnValidate()
	{
		Transform.Scale = new Vector3(_width / (float)PixelPerUnit, _height / (float)PixelPerUnit, 1);
	}
}
