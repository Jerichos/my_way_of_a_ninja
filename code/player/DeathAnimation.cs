using System;
using SpriteTools;

namespace Sandbox.player;

public class DeathAnimation : Component
{
	[Property] private ColorAdjustments ColorAdjustment { get; set; }
	
	[Property] private float AnimationTime { get; set; } = 2.0f;
	[Property] private float FadeInTime { get; set; } = 0.5f;
	[Property] private float FadeOutTime { get; set; } = 0.5f;
	
	[Property] private float FadeInSaturation { get; set; } = 0.5f;
	[Property] private float FadeInHue { get; set; } = 0.5f;
	[Property] private float FadeInBrightness { get; set; } = 0.5f;
	[Property] private float FadeInContrast { get; set; } = 0.5f;
	
	public Action<bool> AnimationFadeFinishedEvent;
	
	bool _fadeIn; // if false then fade out

	private float _timer; // so we know what time it is
	private float _fadeT; // what time it is in the fade, finish fade here at 0 or 1
	
	private float _startSaturation;
	private float _startHue;
	private float _startBrightness;
	private float _startContrast;

	protected override void OnStart()
	{
		_startSaturation = ColorAdjustment.Saturation;
		_startHue = ColorAdjustment.HueRotate;
		_startBrightness = ColorAdjustment.Brightness;
		_startContrast = ColorAdjustment.Contrast;
	}

	public void StartFadeIn()
	{
		Enabled = true;
		_fadeIn = true;
		_timer = 0;
		_fadeT = 0;
	}
	
	public void StartFadeOut()
	{
		Enabled = true;
		_fadeIn = false;
		_timer = 0;
		_fadeT = 1;
	}

	protected override void OnFixedUpdate()
	{
		_timer += Time.Delta;
		
		if (_fadeIn)
		{
			_fadeT = (_timer / FadeInTime).Clamp(0, 1);
			
			
			ColorAdjustment.Saturation = MathX.Lerp(_startSaturation, FadeInSaturation, _fadeT);
			ColorAdjustment.HueRotate = MathX.Lerp(_startHue, FadeInHue, _fadeT);
			ColorAdjustment.Brightness = MathX.Lerp(_startBrightness, FadeInBrightness, _fadeT);
			ColorAdjustment.Contrast = MathX.Lerp(_startContrast, FadeInContrast, _fadeT);
		}
		else
		{
			_fadeT = (_timer / FadeInTime).Clamp(0, 1);
			ColorAdjustment.Saturation = MathX.Lerp(FadeInSaturation, _startSaturation, _fadeT);
			ColorAdjustment.HueRotate = MathX.Lerp(FadeInHue, _startHue, _fadeT);
			ColorAdjustment.Brightness = MathX.Lerp(FadeInBrightness, _startBrightness, _fadeT);
			ColorAdjustment.Contrast = MathX.Lerp(FadeInContrast, _startContrast, _fadeT);
		}
		
		if (_timer >= AnimationTime)
		{
			Enabled = false;
			AnimationFadeFinishedEvent?.Invoke(_fadeIn);
		}
	}
}
