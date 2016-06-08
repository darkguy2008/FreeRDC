using System;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Text;

namespace FreeRDC.Common.Hardware
{
    // Ref http://forum.codecall.net/topic/78149-c-tutorial-generating-a-unique-hardware-id/
    // Ref http://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
    public class HWID
    {
        private static string Encode(string dataToEncode)
        {
            string rv = "";
            byte[] buf = Encoding.UTF8.GetBytes(dataToEncode);
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress, true))
                    gz.Write(buf, 0, buf.Length);
                ms.Position = 0;

                byte[] data = new byte[ms.Length];
                ms.Read(data, 0, data.Length);

                byte[] gzBuf = new byte[data.Length + 4];
                Buffer.BlockCopy(data, 0, gzBuf, 4, data.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buf.Length), 0, gzBuf, 0, 4);
                rv = Convert.ToBase64String(gzBuf);
            }
            return rv;
        }

        private static string Decode(string b64string)
        {
            string rv = "";
            byte[] byteData = Convert.FromBase64String(b64string);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(byteData, 4, byteData.Length - 4);
                byte[] buf = new byte[BitConverter.ToInt32(byteData, 0)];
                ms.Position = 0;
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                    gz.Read(buf, 0, buf.Length);
                rv = Encoding.UTF8.GetString(buf);
            }
            return rv;
        }

        public static string GenerateFingerprint()
        {
            string data = "CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId() + "\nDISK >> " + DiskId() + "\nVIDEO >> " + VideoId() + "\nMAC >> " + MacId();
            return Encode(data);
        }

        public static string DecodeFingerprint(string b64string)
        {
            return Decode(b64string);
        }

        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue = "")
        {
            string result = "";
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementBaseObject mo in moc)
            {
                if(!string.IsNullOrEmpty(wmiMustBeTrue))
                    if (mo[wmiMustBeTrue].ToString() != "True") continue;
                if (result != "") continue;
                try
                {
                    result = mo[wmiProperty].ToString();
                    break;
                }
                catch
                {
                }
            }
            return result;
        }

        private static string CpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "ProcessorId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "Name");
            if (retVal == "") //If no Name, use Manufacturer
            {
                retVal = Identifier("Win32_Processor", "Manufacturer");
            }
            //Add clock speed for extra security
            retVal += Identifier("Win32_Processor", "MaxClockSpeed");
            return retVal;
        }

        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") + Identifier("Win32_BIOS", "IdentificationCode") + Identifier("Win32_BIOS", "SerialNumber") + Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
        }

        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model") + Identifier("Win32_DiskDrive", "Manufacturer") + Identifier("Win32_DiskDrive", "Signature") + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model") + Identifier("Win32_BaseBoard", "Manufacturer") + Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
        }

        private static string VideoId()
        {
            return Identifier("Win32_VideoController", "DriverVersion") + Identifier("Win32_VideoController", "Name");
        }

        private static string MacId()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
    }
}
