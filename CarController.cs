using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform CenterOfMas;
    private InputAction steringInput;
    private InputAction torqueInput;

    [Serializable] private class CarSettings
    {
        public int carHp = 120;
        public int carBrakeTorque = 400;
        public int carBrakeActualForce = 0;
        public float actualSpeed = 0;
        public int carMaxSpeed = 220;
        public int carWheelAngle = 45;
        public int carWeight = 1400;
    }

    [Serializable] private class CarWheelCollider
    {
        public WheelCollider FrontLeftCollider;
        public WheelCollider FrontRightCollider;
        public WheelCollider BackLeftCollider;
        public WheelCollider BackRightCollider;
    }

    [Serializable] private class CarWheelModel
    {
        public GameObject FrontLeftModel;
        public GameObject FrontRightModel;
        public GameObject BackLeftModel;
        public GameObject BackRightModel;
    }

    [Serializable] private class SpecialEffect
    {
        public ParticleSystem dustEffect;
    }

    [Serializable] private class CarAxis
    {
        public float horizontal;
        public float vertical;
    }

    [SerializeField] private CarSettings carSettings;
    [SerializeField] private CarWheelCollider carWheelCollider;
    [SerializeField] private CarWheelModel carWheelModel;
    [SerializeField] private SpecialEffect specialEffect;
    [SerializeField] private CarAxis carAxis;


    [SerializeField] private TextMeshProUGUI speedText;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody.centerOfMass = CenterOfMas.localPosition;

        if (SceneManager.GetActiveScene().name == "Main")
        {
            carRigidbody.constraints = RigidbodyConstraints.None;
        }
        else
        {
            carRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void Awake()
    {
        steringInput = new InputAction("steringWheel", binding: "< Gamepad >/ leftStick / x");
        steringInput.performed += OnSteringInput;
        steringInput.canceled += OnSteringInput;

        // do zrobienia torqueInput = new InputAction("steringWheel", binding: "< Gamepad >/ leftStick / x");
    }

    private void OnSteringInput(InputAction.CallbackContext context)
    {
        float steringValue = context.ReadValue<float>();

        UpdateWheelAngle(carWheelCollider.FrontLeftCollider, steringValue);
        UpdateWheelAngle(carWheelCollider.FrontRightCollider, steringValue);
    }

    // Update is called once per frame
    void Update()
    {
        carSettings.actualSpeed = carRigidbody.velocity.magnitude * 3.6f;
        speedText.text = carSettings.actualSpeed.ToString("F0") + "Km/h";

        if (carSettings.actualSpeed < carSettings.carMaxSpeed)
        {
            UpdateWheelsTorque(carWheelCollider.FrontLeftCollider);
            UpdateWheelsTorque(carWheelCollider.FrontRightCollider);
            UpdateWheelsTorque(carWheelCollider.BackLeftCollider);
            UpdateWheelsTorque(carWheelCollider.BackRightCollider);
        }

        if (Input.GetKey(KeyCode.S) && carSettings.actualSpeed > 0.1)
        {
            carSettings.carBrakeActualForce = carSettings.carBrakeTorque;

            UpdateBrakeTorque(carWheelCollider.FrontLeftCollider);
            UpdateBrakeTorque(carWheelCollider.FrontRightCollider);
            UpdateBrakeTorque(carWheelCollider.BackLeftCollider);
            UpdateBrakeTorque(carWheelCollider.BackRightCollider);
        }

        else
        {
            carSettings.carBrakeActualForce = 0;
            carWheelCollider.FrontLeftCollider.motorTorque = 1;
            carWheelCollider.FrontRightCollider.motorTorque = 1;
        }

        UpdateWheelAngle(carWheelCollider.FrontLeftCollider, carAxis.horizontal);
        UpdateWheelAngle(carWheelCollider.FrontRightCollider, carAxis.horizontal);

        AnimateWheels(carWheelCollider.FrontLeftCollider, carWheelModel.FrontLeftModel);
        AnimateWheels(carWheelCollider.FrontRightCollider, carWheelModel.FrontRightModel);
        AnimateWheels(carWheelCollider.BackLeftCollider, carWheelModel.BackLeftModel);
        AnimateWheels(carWheelCollider.BackRightCollider, carWheelModel.BackRightModel);


        UpdateDustEffect();
        UpdateWeight();
        GetWheelAxis();
    }

    //Add wheel torque to selected wheel
    void UpdateWheelsTorque(WheelCollider wheel)
    {
        wheel.motorTorque = carAxis.vertical * carSettings.carHp;
    }


    //Add wheel brake torque to selected wheel
    void UpdateBrakeTorque(WheelCollider wheel)
    {
        wheel.brakeTorque = carSettings.carBrakeTorque * -carAxis.vertical;
        wheel.brakeTorque = 0;
    }


    //rotate car using wheel
    void UpdateWheelAngle(WheelCollider wheel, float angle)
    {
        wheel.steerAngle = carSettings.carWheelAngle * angle;
    }


    //set wheel mode wheelCollider position and rotation
    void AnimateWheels(WheelCollider wheel, GameObject wheelModel)
    {
        Vector3 WheelPos = wheelModel.transform.position;
        Quaternion WheelRot = wheelModel.transform.rotation;

        wheel.GetWorldPose(out WheelPos, out WheelRot);

        WheelRot = WheelRot * Quaternion.Euler(new Vector3(0, 0, 90));

        wheelModel.transform.position = WheelPos;
        wheelModel.transform.rotation = WheelRot;
    }

    void UpdateDustEffect()
    {
        if (specialEffect.dustEffect != null)
        {
            if (carSettings.actualSpeed >= 50)
            {
                specialEffect.dustEffect.gameObject.SetActive(true);
            }
            else
            {
                specialEffect.dustEffect.gameObject.SetActive(false);
            }
        }
    }


    //update car weight
    void UpdateWeight()
    {
        carRigidbody.mass = carSettings.carWeight;
    }

    //get user input value
    void GetWheelAxis()
    {
        carAxis.horizontal = Input.GetAxis("Horizontal");
        carAxis.vertical = Input.GetAxis("Vertical");
    }
}
