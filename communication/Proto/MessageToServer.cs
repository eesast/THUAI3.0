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
            "ChVNZXNzYWdlVG9TZXJ2ZXIucHJvdG8SD2dvb2dsZS5wcm90b2J1ZhoVTWVz",
            "c2FnZVRvQ2xpZW50LnByb3RvIuQBCg9NZXNzYWdlVG9TZXJ2ZXISCgoCSUQY",
            "ASABKAMSOAoLQ29tbWFuZFR5cGUYAiABKA4yIy5nb29nbGUucHJvdG9idWYu",
            "Q29tbWFuZFR5cGVNZXNzYWdlEjgKDU1vdmVEaXJlY3Rpb24YAyABKA4yIS5n",
            "b29nbGUucHJvdG9idWYuRGlyZWN0aW9uTWVzc2FnZRIUCgxNb3ZlRHVyYXRp",
            "b24YBCABKAUSFQoNVGhyb3dEaXN0YW5jZRgFIAEoARITCgtJc1Rocm93RGlz",
            "aBgGIAEoCBIPCgdVc2VUeXBlGAcgASgFKlkKEkNvbW1hbmRUeXBlTWVzc2Fn",
            "ZRIICgRNb3ZlEAASCAoEUGljaxABEgcKA1B1dBACEgcKA1VzZRADEggKBFN0",
            "b3AQBBITCg9Db21tYW5kVHlwZVNpemUQBUIWqgITQ29tbXVuaWNhdGlvbi5Q",
            "cm90b2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Communication.Proto.MessageToClientReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Communication.Proto.CommandTypeMessage), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Communication.Proto.MessageToServer), global::Communication.Proto.MessageToServer.Parser, new[]{ "ID", "CommandType", "MoveDirection", "MoveDuration", "ThrowDistance", "IsThrowDish", "UseType" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum CommandTypeMessage {
    [pbr::OriginalName("Move")] Move = 0,
    [pbr::OriginalName("Pick")] Pick = 1,
    [pbr::OriginalName("Put")] Put = 2,
    [pbr::OriginalName("Use")] Use = 3,
    [pbr::OriginalName("Stop")] Stop = 4,
    [pbr::OriginalName("CommandTypeSize")] CommandTypeSize = 5,
  }

  #endregion

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
      iD_ = other.iD_;
      commandType_ = other.commandType_;
      moveDirection_ = other.moveDirection_;
      moveDuration_ = other.moveDuration_;
      throwDistance_ = other.throwDistance_;
      isThrowDish_ = other.isThrowDish_;
      useType_ = other.useType_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageToServer Clone() {
      return new MessageToServer(this);
    }

    /// <summary>Field number for the "ID" field.</summary>
    public const int IDFieldNumber = 1;
    private long iD_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long ID {
      get { return iD_; }
      set {
        iD_ = value;
      }
    }

    /// <summary>Field number for the "CommandType" field.</summary>
    public const int CommandTypeFieldNumber = 2;
    private global::Communication.Proto.CommandTypeMessage commandType_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Communication.Proto.CommandTypeMessage CommandType {
      get { return commandType_; }
      set {
        commandType_ = value;
      }
    }

    /// <summary>Field number for the "MoveDirection" field.</summary>
    public const int MoveDirectionFieldNumber = 3;
    private global::Communication.Proto.DirectionMessage moveDirection_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Communication.Proto.DirectionMessage MoveDirection {
      get { return moveDirection_; }
      set {
        moveDirection_ = value;
      }
    }

    /// <summary>Field number for the "MoveDuration" field.</summary>
    public const int MoveDurationFieldNumber = 4;
    private int moveDuration_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int MoveDuration {
      get { return moveDuration_; }
      set {
        moveDuration_ = value;
      }
    }

    /// <summary>Field number for the "ThrowDistance" field.</summary>
    public const int ThrowDistanceFieldNumber = 5;
    private double throwDistance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double ThrowDistance {
      get { return throwDistance_; }
      set {
        throwDistance_ = value;
      }
    }

    /// <summary>Field number for the "IsThrowDish" field.</summary>
    public const int IsThrowDishFieldNumber = 6;
    private bool isThrowDish_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool IsThrowDish {
      get { return isThrowDish_; }
      set {
        isThrowDish_ = value;
      }
    }

    /// <summary>Field number for the "UseType" field.</summary>
    public const int UseTypeFieldNumber = 7;
    private int useType_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int UseType {
      get { return useType_; }
      set {
        useType_ = value;
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
      if (ID != other.ID) return false;
      if (CommandType != other.CommandType) return false;
      if (MoveDirection != other.MoveDirection) return false;
      if (MoveDuration != other.MoveDuration) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(ThrowDistance, other.ThrowDistance)) return false;
      if (IsThrowDish != other.IsThrowDish) return false;
      if (UseType != other.UseType) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ID != 0L) hash ^= ID.GetHashCode();
      if (CommandType != 0) hash ^= CommandType.GetHashCode();
      if (MoveDirection != 0) hash ^= MoveDirection.GetHashCode();
      if (MoveDuration != 0) hash ^= MoveDuration.GetHashCode();
      if (ThrowDistance != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(ThrowDistance);
      if (IsThrowDish != false) hash ^= IsThrowDish.GetHashCode();
      if (UseType != 0) hash ^= UseType.GetHashCode();
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
      if (ID != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(ID);
      }
      if (CommandType != 0) {
        output.WriteRawTag(16);
        output.WriteEnum((int) CommandType);
      }
      if (MoveDirection != 0) {
        output.WriteRawTag(24);
        output.WriteEnum((int) MoveDirection);
      }
      if (MoveDuration != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(MoveDuration);
      }
      if (ThrowDistance != 0D) {
        output.WriteRawTag(41);
        output.WriteDouble(ThrowDistance);
      }
      if (IsThrowDish != false) {
        output.WriteRawTag(48);
        output.WriteBool(IsThrowDish);
      }
      if (UseType != 0) {
        output.WriteRawTag(56);
        output.WriteInt32(UseType);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ID != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ID);
      }
      if (CommandType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) CommandType);
      }
      if (MoveDirection != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) MoveDirection);
      }
      if (MoveDuration != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(MoveDuration);
      }
      if (ThrowDistance != 0D) {
        size += 1 + 8;
      }
      if (IsThrowDish != false) {
        size += 1 + 1;
      }
      if (UseType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(UseType);
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
      if (other.ID != 0L) {
        ID = other.ID;
      }
      if (other.CommandType != 0) {
        CommandType = other.CommandType;
      }
      if (other.MoveDirection != 0) {
        MoveDirection = other.MoveDirection;
      }
      if (other.MoveDuration != 0) {
        MoveDuration = other.MoveDuration;
      }
      if (other.ThrowDistance != 0D) {
        ThrowDistance = other.ThrowDistance;
      }
      if (other.IsThrowDish != false) {
        IsThrowDish = other.IsThrowDish;
      }
      if (other.UseType != 0) {
        UseType = other.UseType;
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
            ID = input.ReadInt64();
            break;
          }
          case 16: {
            CommandType = (global::Communication.Proto.CommandTypeMessage) input.ReadEnum();
            break;
          }
          case 24: {
            MoveDirection = (global::Communication.Proto.DirectionMessage) input.ReadEnum();
            break;
          }
          case 32: {
            MoveDuration = input.ReadInt32();
            break;
          }
          case 41: {
            ThrowDistance = input.ReadDouble();
            break;
          }
          case 48: {
            IsThrowDish = input.ReadBool();
            break;
          }
          case 56: {
            UseType = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
