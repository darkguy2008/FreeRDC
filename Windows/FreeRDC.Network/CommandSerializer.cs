﻿using System;
using System.IO;
using System.Text;

namespace FreeRDC.Network
{
    public class CommandSerializer
    { 
        public enum EType
        {
            Byte = 1,
            Int = 2,
            String = 3,
            ByteArray = 4,
            Struct = 5,
            Null = 6,
            Short = 7,
            Bool = 8
        }

        public byte[] Serialize(object cmd)
        {
            MemoryStream ms = new MemoryStream();
            using(BinaryWriter bw = new BinaryWriter(ms))
            {
                if (cmd.GetType().IsArray)
                {
                    if (cmd.GetType() == typeof(byte[]))
                        bw.Write((byte[])cmd);
                }
                else
                {
                    foreach (var prop in cmd.GetType().GetProperties())
                    {
                        Type propType = prop.PropertyType;
                        if (prop.GetValue(cmd, null) == null)
                        {
                            bw.Write((byte)EType.Null);
                            bw.Write(prop.Name);
                            continue;
                        }
                        if (propType == typeof(byte))
                        {
                            bw.Write((byte)EType.Byte);
                            bw.Write(prop.Name);
                            bw.Write((byte)prop.GetValue(cmd, null));
                            continue;
                        }
                        if (propType == typeof(bool))
                        {
                            bw.Write((byte)EType.Bool);
                            bw.Write(prop.Name);
                            bw.Write((bool)prop.GetValue(cmd, null));
                            continue;
                        }
                        if (propType == typeof(short))
                        {
                            bw.Write((byte)EType.Short);
                            bw.Write(prop.Name);
                            bw.Write((short)prop.GetValue(cmd, null));
                            continue;
                        }
                        if (propType == typeof(int))
                        {
                            bw.Write((byte)EType.Int);
                            bw.Write(prop.Name);
                            bw.Write((int)prop.GetValue(cmd, null));
                            continue;
                        }
                        if (propType == typeof(string))
                        {
                            string v = (string)prop.GetValue(cmd, null);
                            bw.Write((byte)EType.String);
                            bw.Write(prop.Name);
                            bw.Write(v.Length);
                            bw.Write(v);
                            continue;
                        }
                        if (propType.IsNested || propType == typeof(byte[]))
                        {                            
                            object ov = prop.GetValue(cmd, null);
                            byte[] ndata = Serialize(ov);
                            bw.Write(propType.IsNested ? (byte)EType.Struct : (byte)EType.ByteArray);
                            bw.Write(prop.Name);
                            bw.Write(ndata.Length);
                            bw.Write(ndata);
                            continue;
                        }
                    }
                }
            }
            return ms.ToArray();
        }

        public T DeserializeAs<T>(byte[] data) where T : new()
        {
            T obj = new T();
            MemoryStream ms = new MemoryStream(data);
            using (BinaryReader br = new BinaryReader(ms))
            {
                while (br.PeekChar() > 0)
                {
                    Type t = obj.GetType();
                    string propName = string.Empty;
                    EType dataType = (EType)br.ReadByte();
                    switch (dataType)
                    {
                        case EType.Null:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, null, null);
                            break;
                        case EType.Byte:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, br.ReadByte(), null);
                            break;
                        case EType.Bool:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, br.ReadBoolean(), null);
                            break;
                        case EType.Short:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, br.ReadInt16(), null);
                            break;
                        case EType.Int:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, br.ReadInt32(), null);
                            break;
                        case EType.String:
                            propName = br.ReadString();
                            int len = br.ReadInt32();
                            br.ReadByte();
                            string vv = Encoding.ASCII.GetString(br.ReadBytes(len));
                            t.GetProperty(propName).SetValue(obj, vv, null);
                            break;
                        case EType.ByteArray:
                            propName = br.ReadString();
                            t.GetProperty(propName).SetValue(obj, br.ReadBytes(br.ReadInt32()), null);
                            break;
                        case EType.Struct:
                            propName = br.ReadString();
                            Type nt = t.GetProperty(propName).PropertyType;
                            int nlen = br.ReadInt32();
                            var nestedObject = GetType().GetMethod("DeserializeAs").MakeGenericMethod(nt).Invoke(this, new object[] { br.ReadBytes(nlen) });
                            t.GetProperty(propName).SetValue(obj, nestedObject, null);
                            break;
                    }
                }
            }
            return obj;
        }
    }
}
