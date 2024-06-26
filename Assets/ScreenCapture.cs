using System.Collections;
using UnityEngine;

public class ScreenCapture : MonoBehaviour
{
	public Camera camerCapture;
	public Vector2 offset;
	public Sprite CaptureScreenshot()
	{
		
		 
		int width = Screen.width;
		int height = Screen.height;

		Texture2D screenTexture = new Texture2D(width  , height, TextureFormat.RGB24, false);

		RenderTexture renderTexture = new RenderTexture(width + (int)offset.x, height+(int)offset.y, 24);
		camerCapture.targetTexture = renderTexture;
		camerCapture.Render();

		RenderTexture.active = renderTexture;
		screenTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		screenTexture.Apply();

		camerCapture.targetTexture = null;
		RenderTexture.active = null;
		Destroy(renderTexture);

		Sprite newSprite = Sprite.Create(screenTexture, new Rect(0, 0, screenTexture.width , screenTexture.height ), new Vector2(0.5f, 0.5f));

		return newSprite;
	}
}
