namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using OpenCvSharp;

	public class FaceTracker : WebCamera
{
	public TextAsset faces;
	public TextAsset eyes;
	public TextAsset shapes;
	public int rotation = 0;

	private FaceProcessorLive<WebCamTexture> processor;

	/// <summary>
	/// Default initializer for MonoBehavior sub-classes
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

		byte[] shapeDat = shapes.bytes;


		processor = new FaceProcessorLive<WebCamTexture>();
		processor.Initialize(faces.text, eyes.text, shapes.bytes);

		// data stabilizer - affects face rects, face landmarks etc.
		processor.DataStabilizer.Enabled = true;        // enable stabilizer
		processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
		processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

		// performance data - some tricks to make it work faster
		processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
		processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
	}

	/// <summary>
	/// Per-frame video capture processor
	/// </summary>


	protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
	{
		// detect everything we're interested in
		processor.ProcessTexture(input, TextureParameters);

		// mark detected objects
		processor.MarkDetected();

		// processor.Image now holds data we'd like to visualize
		output = Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

		// split the screen into  7 segments
		List<Double> segment = new List<Double>();
		List<int> degrees = new List<int>(){15, 10, 5, 0, -5, -10, -15};

		for (int i = 0; i < 8; i++)
		{
			//600 is currently the camera capture width, it's currently hardcoded which is bad.
			segment.Add(600 * i / 7);
		}
			 
		OpenCvSharp.Point center = new OpenCvSharp.Point(512f, 384f);
		OpenCvSharp.Point new_center = new OpenCvSharp.Point(((processor.firstFace.TopLeft.X + processor.firstFace.BottomRight.X) / 2), ((processor.firstFace.TopLeft.Y + processor.firstFace.BottomRight.Y) / 2));
		OpenCvSharp.Point old_center = new OpenCvSharp.Point(512f, 384f);

		//Debug.Log("Difference: " + Math.Abs(new_center.X - old_center.X));
		/*if (Math.Abs(new_center.X - old_center.X) < 300)
		{
				old_center = center;
				center = new_center;
		}*/
		center = new_center;

			
		//Debug.Log("Initial: " + processor.firstFace);
		//Debug.Log("Center: " + center.X);

			for (int x = 0; x < 7; x++)
		{
			if (center.X >= segment[x] && center.X < segment[x + 1])
				{
					//rotate object
					rotation = degrees[x];
					//Debug.Log("Rotate: " + degrees[x]);
				}

		}

		return true;
	}

}
}