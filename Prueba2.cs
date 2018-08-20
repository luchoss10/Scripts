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

    // Increase the speed/influence rotation
    public float factor = 7;


    public bool enableRotation;
    public bool enableTranslation;

    // SELECT YOUR COM PORT AND BAUDRATE
    string port = "COM8";
    int baudrate = 9600;
    int readTimeout = 25;

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
            float ax = Int32.Parse(dataRaw[0]) / A_R; //* acc_normalizer_factor;
            float ay = Int32.Parse(dataRaw[1]) / A_R; //* acc_normalizer_factor;
            float az = Int32.Parse(dataRaw[2]) / A_R; //* acc_normalizer_factor;

            //Debug.Log(az);
            // normalized gyrocope values
            float gx = Int32.Parse(dataRaw[3]) / G_R; //* gyro_normalizer_factor;
            float gy = Int32.Parse(dataRaw[4]) / G_R; //* gyro_normalizer_factor;
            float gz = Int32.Parse(dataRaw[5]) / G_R; //* gyro_normalizer_factor;

            float az2 = az * A_R * acc_normalizer_factor;
            float gz2 = gz * G_R * gyro_normalizer_factor;

            // prevent 
            //if (Mathf.Abs(ax) - 1 < 0) ax = 0;
            //if (Mathf.Abs(ay) - 1 < 0) ay = 0;
            if (Mathf.Abs(az2) - 1 < 0) az2 = 0;



            //curr_offset_x += ax;
            //curr_offset_y += ay;
            curr_offset_z += 0; // The IMU module have value of z axis of 16600 caused by gravity


            // prevent little noise effect
            //if (Mathf.Abs(gx) < 0.025f) gx = 0f;
            //if (Mathf.Abs(gy) < 0.025f) gy = 0f;
            if (Mathf.Abs(gz2) < 0.025f) gz2 = 0f;

            //curr_angle_x += gx;
            //curr_angle_y += gy;
            curr_angle_z += gz2;

            //Debug.Log(ax+","+ay+","+az);


            float ro = (float) Math.Atan((ax)/(Math.Sqrt(Math.Pow(ay,2) + Math.Pow(az,2)))) * RAD_A_DEG;//Calcular grados con aceleraciones se calculan en rad se cambian a Dregez
            float phi = (float)Math.Atan((ay)/(Math.Sqrt(Math.Pow(ax, 2) + Math.Pow(az, 2)))) * RAD_A_DEG;
            //float teta = (float)Math.Atan((Math.Sqrt(Math.Pow(ax, 2) + Math.Pow(ay, 2)))/(az)) * RAD_A_DEG;

            //Debug.Log(ro + "," + phi + "," + teta);

            curr_angle_x = 0.96f * (curr_angle_x + gx * 0.025f) + 0.04f * ro;
            //Debug.Log(curr_angle_x);
            curr_angle_y = 0.96f * (curr_angle_y + gy * 0.025f) + 0.04f * phi;
            //curr_angle_z = 0.99f * (curr_angle_z + gz * Time.deltaTime) + 0.01f * teta;


            Debug.Log(curr_angle_x + "," + curr_angle_y * factor + "," + curr_angle_z);

            Vector3 angles = new Vector3(curr_angle_x, curr_angle_z * factor, -1 * curr_angle_y);

            if (enableTranslation) target.transform.position = new Vector3(curr_offset_x, curr_offset_z, curr_offset_y);
            if (enableRotation) target.transform.localRotation = Quaternion.Euler(angles);
            /*if (enableRotation)
            {
                target.transform.Rotate(Vector3.up, curr_angle_z * Time.deltaTime);
                target.transform.Rotate(Vector3.right, curr_angle_x * Time.deltaTime);
                target.transform.Rotate(Vector3.back, curr_angle_z * Time.deltaTime);
            }*/
            }
    }

}