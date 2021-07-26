using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

//class responsible for rendering a camera
public partial class CameraRenderer {

    //added so build doesn't fail.
    partial void DrawUnsupportedShaders ();

    partial void DrawGizmos ();

    partial void PrepareForSceneWindow ();

    partial void PrepareBuffer ();

#if UNITY_EDITOR

    static ShaderTagId[] legacyShaderTagIds = {
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

    static Material errorMaterial;

    string SampleName { get; set; }

    partial void DrawGizmos () {
        //UnityEditor.Handles.ShouldRenderGizmos checks if gizmos should be drawn
        if (Handles.ShouldRenderGizmos()) {
            //Since we do not have image effects we will draw gizmos both times
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void DrawUnsupportedShaders () {
        if (errorMaterial == null) {
			errorMaterial =
				new Material(Shader.Find("Hidden/InternalErrorShader"));
		}

        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)) {
			overrideMaterial = errorMaterial
		};

        for (int i = 1; i < legacyShaderTagIds.Length; i++) {
			drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
		}
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    //this sets camera arguments on the scene view camera so that it can render UI elements
    partial void PrepareForSceneWindow () {
		if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
	}

    //Sets the camera buffer name to the gameobject name so that in the frame debug menu we can see the draw
    //calls grouped by each camera
    partial void PrepareBuffer () {
        //In the profiler we will wrap the allocation of camera name in a 'folder' called editor only so it is clear
        Profiler.BeginSample("Editor Only");
		buffer.name = camera.name;
        Profiler.EndSample();
	}

#else

    //when we aren't in the unity editor we set the name to the constant buffer name
    //this is to save on 100 bytes of allocation for the name 
    const string SampleName = bufferName;

#endif
}