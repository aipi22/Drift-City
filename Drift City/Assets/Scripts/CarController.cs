using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public AnimationCurve torqueCurve;

    [Header("Wheel Colliders")]
    public WheelCollider fl;
    public WheelCollider fr;
    public WheelCollider bl;
    public WheelCollider br;

    [Header("Wheel Meshes")]
    public Transform flm;
    public Transform frm;
    public Transform blm;
    public Transform brm;

    [Header("Drive Mode")]
    public bool RearWheelDrive;
    public bool FrontWheelDrive;
    public bool FourWheelDrive;

    [Header("Gear Stuff")]
    public float[] gearRatios = { 3.2f, 2.1f, 1.4f, 1.0f, 0.8f, 0.7f };
    public float diffRatio = 3.9f;
    public float driveTrainEfficiency = 0.85f;

    [Header("Car Settings")]
    public float maxTorque = 450f;
    public float torqueExponent = 0.8f;
    public float horsepower = 600f;
    public float horsepowerToRPMFactor = 0.1f;
    public float responsiveness = 0.5f;
    public float idleRPM = 900f;
    public float redline = 8000f;
    public float RPMdegrade = 0.5f;
    public float brakeForce = 1500f;
    public float maxTurn = 20f;

    [HideInInspector]
    public float throttleInput;
    [HideInInspector]
    public float RPM;
    [HideInInspector]
    public int currentGear;
    [HideInInspector]
    public bool isBraking;

    void Start()
    {
        RPM = idleRPM;
        currentGear = 1;

        if (torqueCurve != null)
        {
            torqueCurve.keys = new Keyframe[0];
        }
        else
        {
            torqueCurve = new AnimationCurve();
        }
        torqueCurve.AddKey(new Keyframe(1000f, 0.2f));
        torqueCurve.AddKey(new Keyframe(2500f, 0.5f));
        torqueCurve.AddKey(new Keyframe(4000f, 0.8f));
        torqueCurve.AddKey(new Keyframe(5500f, 1f));
        torqueCurve.AddKey(new Keyframe(6500f, 0.95f));
        torqueCurve.AddKey(new Keyframe(7500f, 0.85f));
        torqueCurve.AddKey(new Keyframe(8500f, 0.7f));
        torqueCurve.AddKey(new Keyframe(9000f, 0.4f));
    }

    void Update()
    {
        steer();
        brake();
        applyThrottle();
        changeGears();

        updateMesh(fl, flm);
        updateMesh(fr, frm);
        updateMesh(bl, blm);
        updateMesh(br, brm);
    }

    void applyThrottle()
    {
        if (Input.GetKey(KeyCode.W))
        {
            throttleInput = 1;
        }
        else
        {
            throttleInput = 0;
        }

        float avgWheelRPM = Mathf.Abs(bl.rpm + Mathf.Abs(br.rpm)) / 2f;
        RPM = avgWheelRPM * gearRatios[currentGear - 1] * diffRatio;
        RPM = Mathf.Clamp(RPM, idleRPM, redline);
        float engineTorque = torqueCurve.Evaluate(RPM);
        float wheelTorque = engineTorque * gearRatios[currentGear - 1] * diffRatio;

        if (throttleInput > 0)
        {
            if (RearWheelDrive)
            {
                bl.motorTorque = wheelTorque;
                br.motorTorque = wheelTorque;
            }
            else if (FrontWheelDrive)
            {
                fl.motorTorque = wheelTorque;
                fr.motorTorque = wheelTorque;
            }
            else
            {
                fl.motorTorque = wheelTorque;
                fr.motorTorque = wheelTorque;
                bl.motorTorque = wheelTorque;
                br.motorTorque = wheelTorque;
            }
        }
        else
        {
            RPM -= RPMdegrade;

            fl.motorTorque = 0f;
            fr.motorTorque = 0f;
            bl.motorTorque = 0f;
            br.motorTorque = 0f;
        }
    }
    void changeGears()
    {
        if (Input.GetMouseButtonDown(0) && currentGear < gearRatios.Length)
        {
            currentGear++;
            RPM = RPM * (gearRatios[currentGear - 1] / gearRatios[currentGear - 2]);
        }
        else if (Input.GetMouseButtonDown(1) && currentGear > 1)
        {
            currentGear--;
            RPM = RPM * (gearRatios[currentGear - 2] / gearRatios[currentGear - 1]);
        }
    }

    void steer()
    {
        float turnAmount = Input.GetAxis("Horizontal");
        fl.steerAngle = turnAmount * maxTurn;
        fr.steerAngle = turnAmount * maxTurn;
    }

    void brake()
    {
        if (Input.GetKey(KeyCode.DownArrow) == true || Input.GetKey(KeyCode.S) == true)
        {
            fl.brakeTorque = brakeForce;
            fr.brakeTorque = brakeForce;
            bl.brakeTorque = brakeForce;
            br.brakeTorque = brakeForce;
            isBraking = true;
        }
        else if (Input.GetKey(KeyCode.Space) == true)
        {
            bl.brakeTorque = brakeForce * 2;
            br.brakeTorque = brakeForce * 2;
            isBraking = true;
        }
        else
        {
            fl.brakeTorque = 0f;
            fr.brakeTorque = 0f;
            bl.brakeTorque = 0f;
            br.brakeTorque = 0f;
            isBraking = false;
        }
    }

    void updateMesh(WheelCollider wheel, Transform wheelMesh)
    {
        Vector3 position;
        Quaternion rotation;

        wheel.GetWorldPose(out position, out rotation);

        wheelMesh.position = position;
        wheelMesh.rotation = rotation * Quaternion.Euler(0, 0, 90);
    }
}
