using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsMe;
    public Client Client;
    public float Speed = 500f;
    public Vector3 TargetPosition;

    void Start()
    {
        TargetPosition = transform.position;
    }

    void Update()
    {
        if (!IsMe)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * Consts.ClientTension);
        }
        else
        {
            var movementVector = new Vector3()
            {
                x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed,
                y = Input.GetAxis("Vertical") * Time.deltaTime * Speed
            };
            transform.position += movementVector;
        }
    }

    private void FixedUpdate()
    {
        if (!IsMe) return;

        Client.SendPosition(transform.position);
    }
}
