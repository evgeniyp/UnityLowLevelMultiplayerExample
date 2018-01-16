using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxPlayer : MonoBehaviour
{
    private Vector2 UserInput;

    private void Update()
    {
        UserInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.Rotate(Vector3.back, UserInput.x * Time.deltaTime * Consts.ClientSpeed);
        transform.Translate(Vector3.up * UserInput.y * Time.deltaTime * Consts.ClientSpeed);
    }
}
