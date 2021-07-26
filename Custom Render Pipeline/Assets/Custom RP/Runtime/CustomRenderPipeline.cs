using UnityEngine;
using UnityEngine.Rendering;

//type used for the RP instance that our render pipeline asset returns
public class CustomRenderPipeline : RenderPipeline {

    CameraRenderer renderer = new CameraRenderer();

    //RenderPipeline defines a protected abstract Render method that we have to override to create our own pipeline
    protected override void Render (
        ScriptableRenderContext context, Camera[] cameras
    ) {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context,camera);
        }
    }
}