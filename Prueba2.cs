using UnityEngine;
using System.Collections;
using System;
using System.IO.Ports;


public class Prueba2 : MonoBehaviour
{

    SerialPort stream;

    public GameObject target; // is the gameobject to 


    float acc_normalizer_factor = 0.00025f;
    float gyro_normalizer_factor = 1.0f / 32768.0f;   // 32768 is max value captured during test on imu

    float curr_angle_x = 0;
    float curr_angle_y = 0;
    float curr_angle_z = 0;

    float curr_offset_x = 0;
    float curr_offset_y = 0;
    float curr_offset_z = 0;

    float RAD_A_DEG = 57.295779f;
    float A_R = 16384.0f;
    float G_R = 131.0f;

    float ax;
    float ay;
    float az;
    float gx;
    float gy;
    float gz;

    public float ax2;
    public float ay2;
    float ax21;
    float ay21;
    float az2;
    float gz2;


    float ro = 0;
    float phi = 0;

    // Increase the speed/influence rotation
    public float factor = 7;
    public float speed = 4.0f;


    public bool enableRotation;
    public bool enableTranslation;

    // SELECT YOUR COM PORT AND BAUDRATE
    string port = "COM8";
    int baudrate = 9600;
    int readTimeout = 10;

    void Start()
    {
        // open port. Be shure in unity edit > project settings > player is NET2.0 and not NET2.0Subset
        stream = new SerialPort("\\\\.\\" + port, baudrate);

        try
        {
            stream.ReadTimeout = readTimeout;
        }
        catch (System.IO.IOException ioe)
        {
            Debug.Log("IOException: " + ioe.Message);
        }

        stream.Open();
    }

    void Update()
    {
        string dataString = "null received";

        if (stream.IsOpen)
        {
            try
            {
                dataString = stream.ReadLine();
                //Debug.Log("RCV_ : " + dataString);
            }
            catch (System.IO.IOException ioe)
            {
                Debug.Log("IOException: " + ioe.Message);
            }

        }
        else
            dataString = "NOT OPEN";
        //Debug.Log("RCV_ : " + dataString);

        if (!dataString.Equals("NOT OPEN"))
        {
            // recived string is  like  "accx;accy;accz;gyrox;gyroy;gyroz"
            char splitChar = ';';
            string[] dataRaw = dataString.Split(splitChar);

            // normalized accelerometer values
            ax = Int32.Parse(dataRaw[0]) / A_R; //* acc_normalizer_factor;
            ay = Int32.Parse(dataRaw[1]) / A_R; //* acc_normalizer_factor;
            az = Int32.Parse(dataRaw[2]) / A_R; //* acc_normalizer_factor;

            // normalized gyrocope values
            gx = Int32.Parse(dataRaw[3]) / G_R; //* gyro_normalizer_factor;
            gy = Int32.Parse(dataRaw[4]) / G_R; //* gyro_normalizer_factor;
            gz = Int32.Parse(dataRaw[5]) / G_R; //* gyro_normalizer_factor;

            ro = (float)Math.Atan((ax) / (Math.Sqrt(Math.Pow(ay, 2) + Math.Pow(az, 2)))) * RAD_A_DEG;//Calcular grados con aceleraciones se calculan en rad se cambian a Dregez
            phi = (float)Math.Atan((ay) / (Math.Sqrt(Math.Pow(ax, 2) + Math.Pow(az, 2)))) * RAD_A_DEG;

        }

    }


    private void FixedUpdate()
    {
        az2 = az * A_R * acc_normalizer_factor;
        ax2 = ax * A_R * acc_normalizer_factor;
        ay2 = ay * A_R * acc_normalizer_factor;
        gz2 = gz * G_R * gyro_normalizer_factor;

        if (ax2 < 0.5f && ax2 > -0.5f) ax2 = 0.0f;
        if (ay2 < 0.5f && ay2 > -0.5f) ay2 = 0.0f;

        ax21 += ax2 ;
        ay21 += ay2 ;

        if (ax21 < 0.5f && ax21 > -0.5f) ax21 = 0.0f;
        if (ay21 < 0.5f && ay21 > -0.5f) ay21 = 0.0f;

        // prevent 
        if (Mathf.Abs(az2) - 1 < 0) az2 = 0;

        curr_offset_z += 0; // The IMU module have value of z axis of 16600 caused by gravity

        // prevent little noise effect
        //if (Mathf.Abs(gz2) < 0.025f) gz2 = 0f;

        //curr_angle_z += gz2;

        //Filtro complementario
        curr_angle_x = 0.96f * (curr_angle_x + gx * 0.005f) + 0.04f * ro * -1.0f;//*-1 YA QUE LOS VALORES CON RESPECTO A MI CARA DE REFERENCIA SON ESOS
        curr_angle_y = 0.96f * (curr_angle_y + gy * 0.005f) + 0.04f * phi * -1.0f;

        //Vector3 angles = new Vector3(curr_angle_x, curr_angle_z * factor * -1.0f, curr_angle_y);
        Vector3 angles = new Vector3(curr_angle_x, 0, curr_angle_y);
        Vector3 acelerations = new Vector3(ay21, 0, ax21);

        if (enableTranslation) target.transform.position = acelerations * speed * Time.deltaTime;
        if (enableRotation) target.transform.localRotation = Quaternion.Euler(angles);
    }

}