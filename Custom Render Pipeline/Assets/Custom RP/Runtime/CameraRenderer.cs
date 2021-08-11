using UnityEngine;
using UnityEngine.Rendering;

//class responsible for rendering a camera
public partial class CameraRenderer {

    ScriptableRenderContext context;

    Camera camera;

    CullingResults cullingResults;

    const string bufferName = "Render Camera";

    //this is to draw other geometry that cannot be drawn with commands
    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };

	static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    //This function is invoked each frame
    //responsible for drawing all geometry that the camera can see
    public void Render (ScriptableRenderContext context, Camera camera) {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull()) {
			return;
		}

        Setup();
        DrawVisibleGeometry();
		DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    //Sets the view projection matrix of the camera
    void Setup () {
        //setup camera context first so that we get efficient clear
        context.SetupCameraProperties(camera);

        //get the camera clear flags properties from the camera
        //values from 1 to 4 (Skybox, Color, Depth, Nothing)
        CameraClearFlags flags = camera.clearFlags;
        
        //need to guaruntee what we are rendering to. This could have been a render texture in a previous frame
        buffer.ClearRenderTarget(
            //Depth buffer has to be cleared in all cases but 'Nothing'
            flags <= CameraClearFlags.Depth,
            //We only need to clear the colour buffer when the flags are set to colour 
            flags == CameraClearFlags.Color,
            //When clearing we either use the cameras background colour (when clearing to a colour) or use the clear colour
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear); //clears depth and colour data (true) with Color.clear
        
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    //commands we issue to context are buffered, so we need to submit the queued work for execution
    void Submit () {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer () {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

    void DrawVisibleGeometry () {
        //camera provided is used to determine if orthographic or distanced based sorting applies
        var sortingSettings = new SortingSettings(camera){
            //sort transparent objects last
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings
        );

        //need to indicate which render queues are allowed
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        context.DrawSkybox(camera);

        //now draw the transparent geometry
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

    }

    //figuring out what is cullable
    bool Cull () {
        ScriptableCullingParameters p;

        //out is a keyword in c# that sets the variable p
        if(camera.TryGetCullingParameters(out p)) {
            //p passed as ref which is the same as out but not required to set a value
            //ref also passes a reference so this function runs faster as a result since CullingResults can be large
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
}