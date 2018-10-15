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

namespace Tarzan.Nfx.FlowTracker
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class PacketStream : TBase
  {
    private string _FlowUid;
    private List<Frame> _FrameList;

    public string FlowUid
    {
      get
      {
        return _FlowUid;
      }
      set
      {
        __isset.FlowUid = true;
        this._FlowUid = value;
      }
    }

    public List<Frame> FrameList
    {
      get
      {
        return _FrameList;
      }
      set
      {
        __isset.FrameList = true;
        this._FrameList = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool FlowUid;
      public bool FrameList;
    }

    public PacketStream() {
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
            case 6:
              if (field.Type == TType.String) {
                FlowUid = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 20:
              if (field.Type == TType.List) {
                {
                  FrameList = new List<Frame>();
                  TList _list4 = iprot.ReadListBegin();
                  for( int _i5 = 0; _i5 < _list4.Count; ++_i5)
                  {
                    Frame _elem6;
                    _elem6 = new Frame();
                    _elem6.Read(iprot);
                    FrameList.Add(_elem6);
                  }
                  iprot.ReadListEnd();
                }
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
        TStruct struc = new TStruct("PacketStream");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (FlowUid != null && __isset.FlowUid) {
          field.Name = "FlowUid";
          field.Type = TType.String;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(FlowUid);
          oprot.WriteFieldEnd();
        }
        if (FrameList != null && __isset.FrameList) {
          field.Name = "FrameList";
          field.Type = TType.List;
          field.ID = 20;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, FrameList.Count));
            foreach (Frame _iter7 in FrameList)
            {
              _iter7.Write(oprot);
            }
            oprot.WriteListEnd();
          }
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
      StringBuilder __sb = new StringBuilder("PacketStream(");
      bool __first = true;
      if (FlowUid != null && __isset.FlowUid) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("FlowUid: ");
        __sb.Append(FlowUid);
      }
      if (FrameList != null && __isset.FrameList) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("FrameList: ");
        __sb.Append(FrameList);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}