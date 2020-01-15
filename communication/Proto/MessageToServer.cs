// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: MessageToServer.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Communication.Proto {

  /// <summary>Holder for reflection information generated from MessageToServer.proto</summary>
  public static partial class MessageToServerReflection {

    #region Descriptor
    /// <summary>File descriptor for MessageToServer.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MessageToServerReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVNZXNzYWdlVG9TZXJ2ZXIucHJvdG8SD2dvb2dsZS5wcm90b2J1ZiJOCg9N",
            "ZXNzYWdlVG9TZXJ2ZXISEwoLQ29tbWFuZFR5cGUYASABKAUSEgoKUGFyYW1l",
            "dGVyMRgCIAEoBRISCgpQYXJhbWV0ZXIyGAMgASgFQhaqAhNDb21tdW5pY2F0",
            "aW9uLlByb3RvYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Communication.Proto.MessageToServer), global::Communication.Proto.MessageToServer.Parser, new[]{ "CommandType", "Parameter1", "Parameter2" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class MessageToServer : pb::IMessage<MessageToServer> {
    private static readonly pb::MessageParser<MessageToServer> _parser = new pb::MessageParser<MessageToServer>(() => new MessageToServer());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MessageToServer> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Communication.Proto.MessageToServerReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageToServer() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageToServer(MessageToServer other) : this() {
      commandType_ = other.commandType_;
      parameter1_ = other.parameter1_;
      parameter2_ = other.parameter2_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageToServer Clone() {
      return new MessageToServer(this);
    }

    /// <summary>Field number for the "CommandType" field.</summary>
    public const int CommandTypeFieldNumber = 1;
    private int commandType_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CommandType {
      get { return commandType_; }
      set {
        commandType_ = value;
      }
    }

    /// <summary>Field number for the "Parameter1" field.</summary>
    public const int Parameter1FieldNumber = 2;
    private int parameter1_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Parameter1 {
      get { return parameter1_; }
      set {
        parameter1_ = value;
      }
    }

    /// <summary>Field number for the "Parameter2" field.</summary>
    public const int Parameter2FieldNumber = 3;
    private int parameter2_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Parameter2 {
      get { return parameter2_; }
      set {
        parameter2_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MessageToServer);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MessageToServer other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (CommandType != other.CommandType) return false;
      if (Parameter1 != other.Parameter1) return false;
      if (Parameter2 != other.Parameter2) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (CommandType != 0) hash ^= CommandType.GetHashCode();
      if (Parameter1 != 0) hash ^= Parameter1.GetHashCode();
      if (Parameter2 != 0) hash ^= Parameter2.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (CommandType != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(CommandType);
      }
      if (Parameter1 != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Parameter1);
      }
      if (Parameter2 != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Parameter2);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (CommandType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(CommandType);
      }
      if (Parameter1 != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Parameter1);
      }
      if (Parameter2 != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Parameter2);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MessageToServer other) {
      if (other == null) {
        return;
      }
      if (other.CommandType != 0) {
        CommandType = other.CommandType;
      }
      if (other.Parameter1 != 0) {
        Parameter1 = other.Parameter1;
      }
      if (other.Parameter2 != 0) {
        Parameter2 = other.Parameter2;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            CommandType = input.ReadInt32();
            break;
          }
          case 16: {
            Parameter1 = input.ReadInt32();
            break;
          }
          case 24: {
            Parameter2 = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
