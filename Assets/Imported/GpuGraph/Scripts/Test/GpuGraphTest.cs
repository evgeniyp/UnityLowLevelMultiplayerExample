using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuGraphTest : MonoBehaviour
{
    private Rect GraphRect;
    private int _count;

    void Start()
    {
        var xSize = 0.5f * Screen.width;
        var ySize = 0.2f * Screen.height;
        var xPos = 0.2f * Screen.width;
        var yPos = 0.0f * Screen.height + 10;

        GraphRect = new Rect(xPos, yPos, xSize, ySize);
    }

    void Update()
    {
        _count++;


        if(GraphManager.Graph != null)
        {
            //GraphManager.Graph.Plot("Test_WorldSpace", currentDeltaTime, Color.green, new GraphManager.Matrix4x4Wrapper(transform.position, transform.rotation, transform.localScale));
            GraphManager.Graph.Plot("Test_ScreenSpace", Time.deltaTime, Color.green, GraphRect);
            GraphManager.Graph.Plot("Count", _count, Color.red, GraphRect);
        }
    }
}
