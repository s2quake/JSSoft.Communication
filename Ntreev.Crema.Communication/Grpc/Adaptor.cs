// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Grpc/adaptor.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ntreev.Crema.Communication.Grpc {

  /// <summary>Holder for reflection information generated from Grpc/adaptor.proto</summary>
  internal static partial class AdaptorReflection {

    #region Descriptor
    /// <summary>File descriptor for Grpc/adaptor.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AdaptorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChJHcnBjL2FkYXB0b3IucHJvdG8SH250cmVldi5jcmVtYS5jb21tdW5pY2F0",
            "aW9uLmdycGMiUAoNSW52b2tlUmVxdWVzdBITCgtzZXJ2aWNlTmFtZRgBIAEo",
            "CRIMCgRuYW1lGAIgASgJEg0KBXR5cGVzGAMgAygJEg0KBWRhdGFzGAQgAygJ",
            "Ij4KC0ludm9rZVJlcGx5EhMKC3NlcnZpY2VOYW1lGAEgASgJEgwKBHR5cGUY",
            "AiABKAkSDAoEZGF0YRgDIAEoCSIuCgtQb2xsUmVxdWVzdBITCgtzZXJ2aWNl",
            "TmFtZRgBIAEoCRIKCgJpZBgCIAEoBSJHCg1Qb2xsUmVwbHlJdGVtEgoKAmlk",
            "GAEgASgFEgwKBG5hbWUYAiABKAkSDQoFdHlwZXMYAyADKAkSDQoFZGF0YXMY",
            "BCADKAkiXwoJUG9sbFJlcGx5EhMKC3NlcnZpY2VOYW1lGAEgASgJEj0KBWl0",
            "ZW1zGAIgAygLMi4ubnRyZWV2LmNyZW1hLmNvbW11bmljYXRpb24uZ3JwYy5Q",
            "b2xsUmVwbHlJdGVtMtsBCgdBZGFwdG9yEmgKBkludm9rZRIuLm50cmVldi5j",
            "cmVtYS5jb21tdW5pY2F0aW9uLmdycGMuSW52b2tlUmVxdWVzdBosLm50cmVl",
            "di5jcmVtYS5jb21tdW5pY2F0aW9uLmdycGMuSW52b2tlUmVwbHkiABJmCgRQ",
            "b2xsEiwubnRyZWV2LmNyZW1hLmNvbW11bmljYXRpb24uZ3JwYy5Qb2xsUmVx",
            "dWVzdBoqLm50cmVldi5jcmVtYS5jb21tdW5pY2F0aW9uLmdycGMuUG9sbFJl",
            "cGx5IgAoATABYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ntreev.Crema.Communication.Grpc.InvokeRequest), global::Ntreev.Crema.Communication.Grpc.InvokeRequest.Parser, new[]{ "ServiceName", "Name", "Types_", "Datas" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ntreev.Crema.Communication.Grpc.InvokeReply), global::Ntreev.Crema.Communication.Grpc.InvokeReply.Parser, new[]{ "ServiceName", "Type", "Data" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ntreev.Crema.Communication.Grpc.PollRequest), global::Ntreev.Crema.Communication.Grpc.PollRequest.Parser, new[]{ "ServiceName", "Id" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ntreev.Crema.Communication.Grpc.PollReplyItem), global::Ntreev.Crema.Communication.Grpc.PollReplyItem.Parser, new[]{ "Id", "Name", "Types_", "Datas" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ntreev.Crema.Communication.Grpc.PollReply), global::Ntreev.Crema.Communication.Grpc.PollReply.Parser, new[]{ "ServiceName", "Items" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  internal sealed partial class InvokeRequest : pb::IMessage<InvokeRequest> {
    private static readonly pb::MessageParser<InvokeRequest> _parser = new pb::MessageParser<InvokeRequest>(() => new InvokeRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InvokeRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ntreev.Crema.Communication.Grpc.AdaptorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeRequest(InvokeRequest other) : this() {
      serviceName_ = other.serviceName_;
      name_ = other.name_;
      types_ = other.types_.Clone();
      datas_ = other.datas_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeRequest Clone() {
      return new InvokeRequest(this);
    }

    /// <summary>Field number for the "serviceName" field.</summary>
    public const int ServiceNameFieldNumber = 1;
    private string serviceName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServiceName {
      get { return serviceName_; }
      set {
        serviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "types" field.</summary>
    public const int Types_FieldNumber = 3;
    private static readonly pb::FieldCodec<string> _repeated_types_codec
        = pb::FieldCodec.ForString(26);
    private readonly pbc::RepeatedField<string> types_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Types_ {
      get { return types_; }
    }

    /// <summary>Field number for the "datas" field.</summary>
    public const int DatasFieldNumber = 4;
    private static readonly pb::FieldCodec<string> _repeated_datas_codec
        = pb::FieldCodec.ForString(34);
    private readonly pbc::RepeatedField<string> datas_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Datas {
      get { return datas_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InvokeRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InvokeRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ServiceName != other.ServiceName) return false;
      if (Name != other.Name) return false;
      if(!types_.Equals(other.types_)) return false;
      if(!datas_.Equals(other.datas_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ServiceName.Length != 0) hash ^= ServiceName.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      hash ^= types_.GetHashCode();
      hash ^= datas_.GetHashCode();
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
      if (ServiceName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ServiceName);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      types_.WriteTo(output, _repeated_types_codec);
      datas_.WriteTo(output, _repeated_datas_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ServiceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServiceName);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      size += types_.CalculateSize(_repeated_types_codec);
      size += datas_.CalculateSize(_repeated_datas_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(InvokeRequest other) {
      if (other == null) {
        return;
      }
      if (other.ServiceName.Length != 0) {
        ServiceName = other.ServiceName;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      types_.Add(other.types_);
      datas_.Add(other.datas_);
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
          case 10: {
            ServiceName = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            types_.AddEntriesFrom(input, _repeated_types_codec);
            break;
          }
          case 34: {
            datas_.AddEntriesFrom(input, _repeated_datas_codec);
            break;
          }
        }
      }
    }

  }

  internal sealed partial class InvokeReply : pb::IMessage<InvokeReply> {
    private static readonly pb::MessageParser<InvokeReply> _parser = new pb::MessageParser<InvokeReply>(() => new InvokeReply());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InvokeReply> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ntreev.Crema.Communication.Grpc.AdaptorReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeReply() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeReply(InvokeReply other) : this() {
      serviceName_ = other.serviceName_;
      type_ = other.type_;
      data_ = other.data_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InvokeReply Clone() {
      return new InvokeReply(this);
    }

    /// <summary>Field number for the "serviceName" field.</summary>
    public const int ServiceNameFieldNumber = 1;
    private string serviceName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServiceName {
      get { return serviceName_; }
      set {
        serviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "type" field.</summary>
    public const int TypeFieldNumber = 2;
    private string type_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Type {
      get { return type_; }
      set {
        type_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "data" field.</summary>
    public const int DataFieldNumber = 3;
    private string data_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Data {
      get { return data_; }
      set {
        data_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InvokeReply);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InvokeReply other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ServiceName != other.ServiceName) return false;
      if (Type != other.Type) return false;
      if (Data != other.Data) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ServiceName.Length != 0) hash ^= ServiceName.GetHashCode();
      if (Type.Length != 0) hash ^= Type.GetHashCode();
      if (Data.Length != 0) hash ^= Data.GetHashCode();
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
      if (ServiceName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ServiceName);
      }
      if (Type.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Type);
      }
      if (Data.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Data);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ServiceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServiceName);
      }
      if (Type.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Type);
      }
      if (Data.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Data);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(InvokeReply other) {
      if (other == null) {
        return;
      }
      if (other.ServiceName.Length != 0) {
        ServiceName = other.ServiceName;
      }
      if (other.Type.Length != 0) {
        Type = other.Type;
      }
      if (other.Data.Length != 0) {
        Data = other.Data;
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
          case 10: {
            ServiceName = input.ReadString();
            break;
          }
          case 18: {
            Type = input.ReadString();
            break;
          }
          case 26: {
            Data = input.ReadString();
            break;
          }
        }
      }
    }

  }

  internal sealed partial class PollRequest : pb::IMessage<PollRequest> {
    private static readonly pb::MessageParser<PollRequest> _parser = new pb::MessageParser<PollRequest>(() => new PollRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PollRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ntreev.Crema.Communication.Grpc.AdaptorReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollRequest(PollRequest other) : this() {
      serviceName_ = other.serviceName_;
      id_ = other.id_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollRequest Clone() {
      return new PollRequest(this);
    }

    /// <summary>Field number for the "serviceName" field.</summary>
    public const int ServiceNameFieldNumber = 1;
    private string serviceName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServiceName {
      get { return serviceName_; }
      set {
        serviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 2;
    private int id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PollRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PollRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ServiceName != other.ServiceName) return false;
      if (Id != other.Id) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ServiceName.Length != 0) hash ^= ServiceName.GetHashCode();
      if (Id != 0) hash ^= Id.GetHashCode();
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
      if (ServiceName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ServiceName);
      }
      if (Id != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Id);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ServiceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServiceName);
      }
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Id);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PollRequest other) {
      if (other == null) {
        return;
      }
      if (other.ServiceName.Length != 0) {
        ServiceName = other.ServiceName;
      }
      if (other.Id != 0) {
        Id = other.Id;
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
          case 10: {
            ServiceName = input.ReadString();
            break;
          }
          case 16: {
            Id = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  internal sealed partial class PollReplyItem : pb::IMessage<PollReplyItem> {
    private static readonly pb::MessageParser<PollReplyItem> _parser = new pb::MessageParser<PollReplyItem>(() => new PollReplyItem());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PollReplyItem> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ntreev.Crema.Communication.Grpc.AdaptorReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReplyItem() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReplyItem(PollReplyItem other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      types_ = other.types_.Clone();
      datas_ = other.datas_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReplyItem Clone() {
      return new PollReplyItem(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private int id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "types" field.</summary>
    public const int Types_FieldNumber = 3;
    private static readonly pb::FieldCodec<string> _repeated_types_codec
        = pb::FieldCodec.ForString(26);
    private readonly pbc::RepeatedField<string> types_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Types_ {
      get { return types_; }
    }

    /// <summary>Field number for the "datas" field.</summary>
    public const int DatasFieldNumber = 4;
    private static readonly pb::FieldCodec<string> _repeated_datas_codec
        = pb::FieldCodec.ForString(34);
    private readonly pbc::RepeatedField<string> datas_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Datas {
      get { return datas_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PollReplyItem);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PollReplyItem other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if(!types_.Equals(other.types_)) return false;
      if(!datas_.Equals(other.datas_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      hash ^= types_.GetHashCode();
      hash ^= datas_.GetHashCode();
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
      if (Id != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      types_.WriteTo(output, _repeated_types_codec);
      datas_.WriteTo(output, _repeated_datas_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Id);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      size += types_.CalculateSize(_repeated_types_codec);
      size += datas_.CalculateSize(_repeated_datas_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PollReplyItem other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      types_.Add(other.types_);
      datas_.Add(other.datas_);
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
            Id = input.ReadInt32();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            types_.AddEntriesFrom(input, _repeated_types_codec);
            break;
          }
          case 34: {
            datas_.AddEntriesFrom(input, _repeated_datas_codec);
            break;
          }
        }
      }
    }

  }

  internal sealed partial class PollReply : pb::IMessage<PollReply> {
    private static readonly pb::MessageParser<PollReply> _parser = new pb::MessageParser<PollReply>(() => new PollReply());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PollReply> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ntreev.Crema.Communication.Grpc.AdaptorReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReply() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReply(PollReply other) : this() {
      serviceName_ = other.serviceName_;
      items_ = other.items_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PollReply Clone() {
      return new PollReply(this);
    }

    /// <summary>Field number for the "serviceName" field.</summary>
    public const int ServiceNameFieldNumber = 1;
    private string serviceName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServiceName {
      get { return serviceName_; }
      set {
        serviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "items" field.</summary>
    public const int ItemsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Ntreev.Crema.Communication.Grpc.PollReplyItem> _repeated_items_codec
        = pb::FieldCodec.ForMessage(18, global::Ntreev.Crema.Communication.Grpc.PollReplyItem.Parser);
    private readonly pbc::RepeatedField<global::Ntreev.Crema.Communication.Grpc.PollReplyItem> items_ = new pbc::RepeatedField<global::Ntreev.Crema.Communication.Grpc.PollReplyItem>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ntreev.Crema.Communication.Grpc.PollReplyItem> Items {
      get { return items_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PollReply);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PollReply other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ServiceName != other.ServiceName) return false;
      if(!items_.Equals(other.items_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ServiceName.Length != 0) hash ^= ServiceName.GetHashCode();
      hash ^= items_.GetHashCode();
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
      if (ServiceName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ServiceName);
      }
      items_.WriteTo(output, _repeated_items_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ServiceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServiceName);
      }
      size += items_.CalculateSize(_repeated_items_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PollReply other) {
      if (other == null) {
        return;
      }
      if (other.ServiceName.Length != 0) {
        ServiceName = other.ServiceName;
      }
      items_.Add(other.items_);
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
          case 10: {
            ServiceName = input.ReadString();
            break;
          }
          case 18: {
            items_.AddEntriesFrom(input, _repeated_items_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
