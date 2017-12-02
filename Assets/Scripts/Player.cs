using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsMe;
    public Client Client;
    public float Speed = 500f;

    void Start()
    {

    }

    void Update()
    {
        if (!IsMe) return;

        var movementVector = new Vector3()
        {
            x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed,
            y = Input.GetAxis("Vertical") * Time.deltaTime * Speed
        };

        transform.position += movementVector;
    }

    private void FixedUpdate()
    {
        if (!IsMe) return;

        Client.SendPosition(transform.position);
    }
}
