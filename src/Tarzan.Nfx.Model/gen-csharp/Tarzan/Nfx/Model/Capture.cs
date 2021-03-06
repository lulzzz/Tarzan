/**
 * Autogenerated by Thrift Compiler (0.11.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace Tarzan.Nfx.Model
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class Capture : TBase
  {
    private string _Uid;
    private string _Name;
    private long _CreationTime;
    private long _Length;
    private string _Hash;

    public string Uid
    {
      get
      {
        return _Uid;
      }
      set
      {
        __isset.Uid = true;
        this._Uid = value;
      }
    }

    public string Name
    {
      get
      {
        return _Name;
      }
      set
      {
        __isset.Name = true;
        this._Name = value;
      }
    }

    public long CreationTime
    {
      get
      {
        return _CreationTime;
      }
      set
      {
        __isset.CreationTime = true;
        this._CreationTime = value;
      }
    }

    public long Length
    {
      get
      {
        return _Length;
      }
      set
      {
        __isset.Length = true;
        this._Length = value;
      }
    }

    public string Hash
    {
      get
      {
        return _Hash;
      }
      set
      {
        __isset.Hash = true;
        this._Hash = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool Uid;
      public bool Name;
      public bool CreationTime;
      public bool Length;
      public bool Hash;
    }

    public Capture() {
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.String) {
                Uid = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Name = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.I64) {
                CreationTime = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.I64) {
                Length = iprot.ReadI64();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.String) {
                Hash = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("Capture");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (Uid != null && __isset.Uid) {
          field.Name = "Uid";
          field.Type = TType.String;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Uid);
          oprot.WriteFieldEnd();
        }
        if (Name != null && __isset.Name) {
          field.Name = "Name";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Name);
          oprot.WriteFieldEnd();
        }
        if (__isset.CreationTime) {
          field.Name = "CreationTime";
          field.Type = TType.I64;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(CreationTime);
          oprot.WriteFieldEnd();
        }
        if (__isset.Length) {
          field.Name = "Length";
          field.Type = TType.I64;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteI64(Length);
          oprot.WriteFieldEnd();
        }
        if (Hash != null && __isset.Hash) {
          field.Name = "Hash";
          field.Type = TType.String;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Hash);
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("Capture(");
      bool __first = true;
      if (Uid != null && __isset.Uid) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Uid: ");
        __sb.Append(Uid);
      }
      if (Name != null && __isset.Name) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Name: ");
        __sb.Append(Name);
      }
      if (__isset.CreationTime) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("CreationTime: ");
        __sb.Append(CreationTime);
      }
      if (__isset.Length) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Length: ");
        __sb.Append(Length);
      }
      if (Hash != null && __isset.Hash) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Hash: ");
        __sb.Append(Hash);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
