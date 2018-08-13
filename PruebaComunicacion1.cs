using System.Collections;
using System.Collections.Generic;
using System;
using System.IO.Ports;//iNCLUSION DE LA LIBRERIA PORTS PARA LA COMUNICACION SERIAL 
using UnityEngine;

public class PruebaComunicacion1 : MonoBehaviour {

    
    SerialPort serialPort = new SerialPort("COM8", 9600);//Incia la comunicacion del puerto serial

	// Use this for initialization
	void Start () {
        serialPort.ReadTimeout = 20;//Establecemos el tiempo de esperacuando una operacion de lectura no finaliza
        serialPort.Open();//Aabrimos una nueva conexión del puerto serie
	}
	
	// Update is called once per frame
	void Update () {
        if (serialPort.IsOpen)//Comprobamos que el puerto esta abierto
        {
            try//Utilizamos el bloque try/catch para detertac una posible excepción
            {
                string value = serialPort.ReadLine();//leeemos una linea del puerto serie y la almacenamos en un strng
                print(value);//printeamos la linea leida para verificar que leemos el dato que manda nuestro arduino
                string[] vec6 = value.Split(',');//Separamos el string leido valiendonos de las comas y almacenamos los valores en un array
            }
            catch(System.Exception e)
            {
                print(e);//Ignoramos Error o excepción
            }
        }
	}
}

