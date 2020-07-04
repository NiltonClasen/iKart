using System.Collections;
using System.Collections.Generic;
using SWNetwork;
using UnityEngine;

public class Carro : MonoBehaviour
{
    public float engineForce = 900f;
    public float steerAngle = 30f;
    public float breakForce = 30000f;

    public WheelCollider frontRightCollider;
    public WheelCollider frontLeftCollider;
    public WheelCollider rearRightCollider;
    public WheelCollider rearLeftCollider;

    public Transform frontRightTransform;
    public Transform frontLeftTransform;
    public Transform rearRightTransform;
    public Transform rearLeftTransform;

    public Transform centerOfMass;

    public Quaternion frontWheelRot;
    public Quaternion rearWheelRot;

    GerenciadorAcoes gerenciadorAcoes;

    NetworkID networkId;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        //define o centro da massa como o local atual do componente
        rb.centerOfMass = centerOfMass.localPosition;
        gerenciadorAcoes = FindObjectOfType<GerenciadorAcoes>();

        // inicia o componente de conexão
        networkId = GetComponent<NetworkID>();

        //caso a ação seja do usuário, ele setará a camera para o seu carro
        if (networkId.IsMine)
        {
            
            Camera cam = Camera.main;
            CameraMove camFollow = cam.GetComponent<CameraMove>();
            camFollow.target = transform;
        }
        else
        {
            frontRightCollider.enabled = false;
            frontLeftCollider.enabled = false;
            rearRightCollider.enabled = false;
            rearLeftCollider.enabled = false;
        }
    }

    private void Update()
    {
        // método fica escutando para continuar atualizando os objetos
        if(gerenciadorAcoes.State == GerenciadorAcoes.GameState.started)
        {
            // atualiza o movimento das rodas caso o carro seja o do próprio usuároio
            if (networkId.IsMine)
            {
                MovimentoRodaUsuario();
            }

            MovimentoRodaEnemy();
        }
    }

    /// <summary>
    /// Atualiza a roda
    /// </summary>
    void MovimentoRodaEnemy()
    {
        if (networkId.IsMine)
        {
            Quaternion rotation;
            Vector3 position;

            frontLeftCollider.GetWorldPose(out position, out rotation);
            frontLeftTransform.position = position;
            frontLeftTransform.rotation = rotation;

            frontRightCollider.GetWorldPose(out position, out rotation);
            frontRightTransform.position = position;
            frontRightTransform.rotation = rotation;

            frontWheelRot = rotation;

            rearLeftCollider.GetWorldPose(out position, out rotation);
            rearLeftTransform.position = position;
            rearLeftTransform.rotation = rotation;

            rearRightCollider.GetWorldPose(out position, out rotation);
            rearRightTransform.position = position;
            rearRightTransform.rotation = rotation;

            rearWheelRot = rotation;
        }
        else
        {
            // aplica a atualização da roda para o oponente
            frontLeftTransform.rotation = frontWheelRot;
            frontRightTransform.rotation = frontWheelRot;
            rearLeftTransform.rotation = rearWheelRot;
            rearRightTransform.rotation = rearWheelRot;
        }
    }

    /// <summary>
    /// Atualiza a física da roda
    /// </summary>
    void MovimentoRodaUsuario()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        // Aceleração
        rearRightCollider.motorTorque = v * engineForce;
        rearLeftCollider.motorTorque = v * engineForce;
        frontRightCollider.motorTorque = v * engineForce;
        frontLeftCollider.motorTorque = v * engineForce;

        // Direção
        frontRightCollider.steerAngle = h * steerAngle;
        frontLeftCollider.steerAngle = h * steerAngle;

        // Freio
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Freio acionado");
            rearRightCollider.brakeTorque = breakForce;
            rearLeftCollider.brakeTorque = breakForce;
            frontRightCollider.brakeTorque = breakForce;
            frontLeftCollider.brakeTorque = breakForce;
        }

        // resetar o freio
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Freio solto");
            rearRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            frontLeftCollider.brakeTorque = 0;
        }
    }
}
