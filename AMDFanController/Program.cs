using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
//using Microsoft.VisualBasic.FileIO;

namespace AMDFanController
{
    class Program
    {
        enum FanStates {Maximum,Manual,Automatic};
        enum SensorTypes {input,label,crit,crit_hyst,emergency};
        private static string CardLocation = "";
        private static int menu=0;
        static void Main(string[] args)
        {
            Console.Clear();
            CardLocation = GetGPULocation();
            SetFanMode(FanStates.Manual);
            Console.Clear();
            //byte speed = GPUTemp();
            while (true)
            {
                Thread.Sleep(1000);
                //Console.Clear();
                byte DestinationSpeed = SpeedTemp(GPUTemp(1));
                Console.Clear();
                for (int i = 1; i < 4; i++)
                {
                   Console.WriteLine(GPUTempString(i)); 
                }
                Console.WriteLine($"speed\n\tcurrent speed is:\t{DestinationSpeed}  \t maximum speed is:\t255");
                SetFanSpeed(DestinationSpeed);
            }
        }

        private static string GetGPULocation()
        {
            return "/sys/class/drm/card0/device/hwmon/hwmon1/";
            //If you can figure out how to generalize this for other GPU's, make it so and
            //Make it a PR!
        }

        static byte SpeedTemp(byte temperature)
        {
            var FanSpeed = (byte) Math.Min(Math.Max(0, temperature - 60) * 255 / 30, 255);
            return FanSpeed; //Failsafe
        }
        static void SetFanSpeed(byte speed)
        {
            string p = speed.ToString();
            File.WriteAllText(CardLocation + "pwm1", p);
        }
        static void SetFanMode(FanStates mode)
        {
            Console.WriteLine("Setting GPU Control");
            string PWM1_Enable = CardLocation + "pwm1_enable";
            File.WriteAllText(PWM1_Enable, ((int)mode).ToString());
        }

        static string GetGPUData(int SensorNumber, SensorTypes Type){
            return File.ReadAllText($"{CardLocation}temp{SensorNumber}_{Type.ToString()}");
        }

        static byte GPUTemp(int SensorNumber, SensorTypes Type=SensorTypes.input){
            string temp = GetGPUData(SensorNumber,Type);
            int temperature = int.Parse(temp) / 1000;
            byte final = Convert.ToByte(temperature);
            return final;
        }
        static string RemoveEndEnter(string input){
            char[] arr=input.ToCharArray();
            input="";
            for (int i = 0; i < arr.Length-1; i++)
            {
                input+=arr[i].ToString();
            }
            return input;
        }
        static string GPUTempString(int SensorNumber){
            string temp = "";
            temp+=$"{RemoveEndEnter(GetGPUData(SensorNumber,SensorTypes.label))}\n\t current temperature is:\t{GPUTemp(SensorNumber)}°C  \t critical temperature is:\t{GPUTemp(SensorNumber,SensorTypes.emergency)}°C";
            return temp;
        }
    }
}