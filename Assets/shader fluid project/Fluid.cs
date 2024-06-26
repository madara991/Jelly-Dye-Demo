
using JetBrains.Annotations;
using System.Drawing;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

namespace StableFluids
{
	public class Fluid : MonoBehaviour
	{
		#region Editable attributes

		[SerializeField] int _resolution = 512;
		[SerializeField] float _viscosity = 1e-6f;
		[SerializeField] float _force = 300;
		[SerializeField] float _exponent = 200;
		[SerializeField] Texture2D _initial;

		#endregion

		#region Internal resources

		[SerializeField] ComputeShader _compute;
		[SerializeField] Shader _shader;

		#endregion

		#region Private members

		Material _shaderSheet;
		Vector2 _previousInput;

		public RenderTexture RenderTextureTarget;
		public GameObject TargetObject;
		public UnityEngine.Color fluidColor;
		static class Kernels
		{
			public const int Advect = 0;
			public const int Force = 1;
			public const int PSetup = 2;
			public const int PFinish = 3;
			public const int Jacobi1 = 4;
			public const int Jacobi2 = 5;
		}

		int ThreadCountX { get { return (_resolution + 7) / 8; } }
		int ThreadCountY { get { return (_resolution * RenderTextureTarget.height / RenderTextureTarget.width + 7) / 8; } }

		int ResolutionX { get { return ThreadCountX * 8; } }
		int ResolutionY { get { return ThreadCountY * 8; } }

		// Vector field buffers
		static class VFB
		{
			public static RenderTexture V1;
			public static RenderTexture V2;
			public static RenderTexture V3;
			public static RenderTexture P1;
			public static RenderTexture P2;
		}

		// Color buffers (for double buffering)
		RenderTexture _colorRT1;
		RenderTexture _colorRT2;

		RenderTexture AllocateBuffer(int componentCount, int width = 0, int height = 0)
		{
			var format = RenderTextureFormat.ARGBHalf;
			if (componentCount == 1) format = RenderTextureFormat.RHalf;
			if (componentCount == 2) format = RenderTextureFormat.RGHalf;

			if (width == 0) width = ResolutionX;
			if (height == 0) height = ResolutionY;

			var rt = new RenderTexture(width, height, 0, format);
			rt.enableRandomWrite = true;
			rt.Create();
			RenderTextureTarget = rt;
			return rt;
		}

		#endregion

		#region MonoBehaviour implementation
		void OnValidate()
		{
			_resolution = Mathf.Max(_resolution, 8);
		}
		
		void Start()
		{

			_shaderSheet = new Material(_shader);
			
			VFB.V1 = AllocateBuffer(2);
			VFB.V2 = AllocateBuffer(2);
			VFB.V3 = AllocateBuffer(2);
			VFB.P1 = AllocateBuffer(1);
			VFB.P2 = AllocateBuffer(1);

			_colorRT1 = AllocateBuffer(4, RenderTextureTarget.width, RenderTextureTarget.height);
			_colorRT2 = AllocateBuffer(4, RenderTextureTarget.width, RenderTextureTarget.height);

			


		}
		public void SetTextureInitlil()
		{
			Graphics.Blit(_initial, _colorRT1);
		}
		void OnDestroy()
		{
			Destroy(_shaderSheet);

			Destroy(VFB.V1);
			Destroy(VFB.V2);
			Destroy(VFB.V3);
			Destroy(VFB.P1);
			Destroy(VFB.P2);

			Destroy(_colorRT1);
			Destroy(_colorRT2);
		}
		
		void Update()
		{
			if (TargetObject.gameObject == null)
				return;
			

			var dt = Time.deltaTime;
			var dx = 1.0f / ResolutionY;

			

			Mesh mesh = TargetObject.GetComponent<MeshFilter>().sharedMesh;
			Vector3[] vertices = mesh.vertices;

			
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = TargetObject.transform.TransformPoint(vertices[i]);
			}

			
			Vector2 boundsMin = Camera.main.WorldToScreenPoint(vertices[0]);
			Vector2 boundsMax = boundsMin;

			
			foreach (Vector3 vertex in vertices)
			{
				Vector2 screenPoint = Camera.main.WorldToScreenPoint(vertex);

				boundsMin = Vector2.Min(boundsMin, screenPoint);
				boundsMax = Vector2.Max(boundsMax, screenPoint);
			}

			
			float objectWidth = Mathf.Abs(boundsMax.x - boundsMin.x);
			float objectHeight = Mathf.Abs(boundsMax.y - boundsMin.y);

			
			Vector2 objectCenter = (boundsMin + boundsMax) / 2;

			Vector3 HitedNeedle = GameData.Instance.GetNeedle().GetComponent<PlayerControler>().hitPositionAsScreenPoint;
			// Calculate _input based on the mouse position
			Vector2 _input = new Vector2(
				(HitedNeedle.x - objectCenter.x) / objectWidth,
				(HitedNeedle.y - objectCenter.y) / objectHeight
			);

			#region Debugging render Screen
			//Debug.Log("Object Width in Screen Space: " + objectWidth);
			//Debug.Log("Object Height in Screen Space: " + objectHeight);
			//Debug.Log("Object Center in Screen Space: " + objectCenter);
			//Debug.Log("_input: " + _input);
			//Debug.Log("width and Height: " + objectWidth + " " + objectHeight);
			//Debug.Log("bounds normal" + bounds);
			//Debug.Log("bounds mesh : " + TargetObject.GetComponent<MeshFilter>().mesh.bounds);
			//Debug.Log("mouse pos: " + Input.mousePosition);
			//Debug.LogWarning("Mouse Position: "+"<color=green>" + Input.mousePosition + "</color>");
			//Debug.Log("CALCULATE: " + Input.mousePosition.x+"-" + screenCenter.x + "  /" + screenWidth);
			//Debug.Log("_INPUT:" + _input);
			//Debug.Log("Width,Height (Object):  " + (int)(screenWidth) + "," + (int)screenHeight);
			//Debug.Log("Min.Max: " + screenMin +","+ screenMax);
			//Debug.Log("Bounds Centar: " + Camera.main.WorldToScreenPoint(bounds.center));
			//Debug.Log("Bounds Size: " + Camera.main.WorldToScreenPoint(bounds.size));
			#endregion

			_shaderSheet.SetColor("_DyeColor", fluidColor);

			// Common variables
			_compute.SetFloat("Time", Time.time);
			_compute.SetFloat("DeltaTime", dt);

			// Advection
			_compute.SetTexture(Kernels.Advect, "U_in", VFB.V1);
			_compute.SetTexture(Kernels.Advect, "W_out", VFB.V2);
			_compute.Dispatch(Kernels.Advect, ThreadCountX, ThreadCountY, 1);

			// Diffuse setup
			var dif_alpha = dx * dx / (_viscosity * dt);
			_compute.SetFloat("Alpha", dif_alpha);
			_compute.SetFloat("Beta", 4 + dif_alpha);
			Graphics.CopyTexture(VFB.V2, VFB.V1);
			_compute.SetTexture(Kernels.Jacobi2, "B2_in", VFB.V1);

			// Jacobi iteration
			for (var i = 0; i < 20; i++)
			{
				_compute.SetTexture(Kernels.Jacobi2, "X2_in", VFB.V2);
				_compute.SetTexture(Kernels.Jacobi2, "X2_out", VFB.V3);
				_compute.Dispatch(Kernels.Jacobi2, ThreadCountX, ThreadCountY, 1);

				_compute.SetTexture(Kernels.Jacobi2, "X2_in", VFB.V3);
				_compute.SetTexture(Kernels.Jacobi2, "X2_out", VFB.V2);
				_compute.Dispatch(Kernels.Jacobi2, ThreadCountX, ThreadCountY, 1);
			}

			// Add external force
			_compute.SetVector("ForceOrigin", _input);
			_compute.SetFloat("ForceExponent", _exponent);
			_compute.SetTexture(Kernels.Force, "W_in", VFB.V2);
			_compute.SetTexture(Kernels.Force, "W_out", VFB.V3);

			if (Input.GetMouseButton(0))
				// Random push
				_compute.SetVector("ForceVector", Random.insideUnitCircle * _force * 0.025f);
			else if (Input.GetMouseButton(1))
				// Mouse drag
				_compute.SetVector("ForceVector", (_input - _previousInput) * _force);
			else
				_compute.SetVector("ForceVector", Vector4.zero);

			_compute.Dispatch(Kernels.Force, ThreadCountX, ThreadCountY, 1);

			// Projection setup
			_compute.SetTexture(Kernels.PSetup, "W_in", VFB.V3);
			_compute.SetTexture(Kernels.PSetup, "DivW_out", VFB.V2);
			_compute.SetTexture(Kernels.PSetup, "P_out", VFB.P1);
			_compute.Dispatch(Kernels.PSetup, ThreadCountX, ThreadCountY, 1);

			// Jacobi iteration
			_compute.SetFloat("Alpha", -dx * dx);
			_compute.SetFloat("Beta", 4);
			_compute.SetTexture(Kernels.Jacobi1, "B1_in", VFB.V2);

			for (var i = 0; i < 20; i++)
			{
				_compute.SetTexture(Kernels.Jacobi1, "X1_in", VFB.P1);
				_compute.SetTexture(Kernels.Jacobi1, "X1_out", VFB.P2);
				_compute.Dispatch(Kernels.Jacobi1, ThreadCountX, ThreadCountY, 1);

				_compute.SetTexture(Kernels.Jacobi1, "X1_in", VFB.P2);
				_compute.SetTexture(Kernels.Jacobi1, "X1_out", VFB.P1);
				_compute.Dispatch(Kernels.Jacobi1, ThreadCountX, ThreadCountY, 1);
			}

			// Projection finish
			_compute.SetTexture(Kernels.PFinish, "W_in", VFB.V3);
			_compute.SetTexture(Kernels.PFinish, "P_in", VFB.P1);
			_compute.SetTexture(Kernels.PFinish, "U_out", VFB.V1);
			_compute.Dispatch(Kernels.PFinish, ThreadCountX, ThreadCountY, 1);

			// Apply the velocity field to the color buffer.
			var offs = Vector2.one * (Input.GetMouseButton(0) ? 0 : 1e+7f);
			_shaderSheet.SetVector("_ForceOrigin", _input + offs);
			_shaderSheet.SetFloat("_ForceExponent", _exponent);
			_shaderSheet.SetTexture("_VelocityField", VFB.V1);
			Graphics.Blit(_colorRT1, _colorRT2, _shaderSheet, 0);

			// Swap the color buffers.
			var temp = _colorRT1;
			_colorRT1 = _colorRT2;
			_colorRT2 = temp;

			_previousInput = _input;
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(_colorRT1, destination, _shaderSheet, 1);
		}
		
		#endregion


	}
	
}