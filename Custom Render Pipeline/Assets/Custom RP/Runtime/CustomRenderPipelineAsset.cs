using UnityEngine;
using UnityEngine.Rendering;

//this puts an entry in the asset/create menu
[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]

//This asset is used to gibe unity a way to get hold of a pipeline object instance that is responsible for rendering
public class CustomRenderPipelineAsset : RenderPipelineAsset{
    //To get a pipeline object instance we need to override the abstract CreatePipeline to return a RenderPipeline instance
    //CreatePipeline is defined with the protected access modifier which means that only the class that defined the method (and who extend it) can access it
    protected override RenderPipeline CreatePipeline () {
        return null;
    }
}