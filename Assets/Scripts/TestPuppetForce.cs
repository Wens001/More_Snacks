using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPuppetForce : MonoBehaviour
{
    public PuppetMaster puppet;
    public Rigidbody rg;
    private Rigidbody[] rgs;
    public float force;
    // Start is called before the first frame update
    void Start()
    {
        rgs = puppet.GetComponentsInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            puppet.state = PuppetMaster.State.Dead;
            rg.AddForce(transform.forward * -force , ForceMode.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            puppet.state = PuppetMaster.State.Alive;

        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            puppet.state = PuppetMaster.State.Dead;
            for (int i = 0; i < rgs.Length; i++) {
                rgs[i].AddForce(transform.forward * -force, ForceMode.Impulse);
            }
        }
    }
}
