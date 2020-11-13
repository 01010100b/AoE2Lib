// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Bots/protos/expert_api.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Protos.Expert {

  /// <summary>Holder for reflection information generated from Bots/protos/expert_api.proto</summary>
  public static partial class ExpertApiReflection {

    #region Descriptor
    /// <summary>File descriptor for Bots/protos/expert_api.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ExpertApiReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChxCb3RzL3Byb3Rvcy9leHBlcnRfYXBpLnByb3RvEg1wcm90b3MuZXhwZXJ0",
            "Ghlnb29nbGUvcHJvdG9idWYvYW55LnByb3RvIksKC0NvbW1hbmRMaXN0EhQK",
            "DHBsYXllck51bWJlchgBIAEoBRImCghjb21tYW5kcxgCIAMoCzIULmdvb2ds",
            "ZS5wcm90b2J1Zi5BbnkiUAoRQ29tbWFuZFJlc3VsdExpc3QSFAoMcGxheWVy",
            "TnVtYmVyGAEgASgFEiUKB3Jlc3VsdHMYAiADKAsyFC5nb29nbGUucHJvdG9i",
            "dWYuQW55Ii0KE1Jlc29sdmVDb25zdFJlcXVlc3QSFgoOY29uc3RUb1Jlc29s",
            "dmUYASABKAkiKgoUUmVzb2x2ZUNvbnN0UmVzcG9uc2USEgoKY29uc3RWYWx1",
            "ZRgBIAEoBTK8AQoJRXhwZXJ0QVBJElQKEkV4ZWN1dGVDb21tYW5kTGlzdBIa",
            "LnByb3Rvcy5leHBlcnQuQ29tbWFuZExpc3QaIC5wcm90b3MuZXhwZXJ0LkNv",
            "bW1hbmRSZXN1bHRMaXN0IgASWQoMUmVzb2x2ZUNvbnN0EiIucHJvdG9zLmV4",
            "cGVydC5SZXNvbHZlQ29uc3RSZXF1ZXN0GiMucHJvdG9zLmV4cGVydC5SZXNv",
            "bHZlQ29uc3RSZXNwb25zZSIAYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Google.Protobuf.WellKnownTypes.AnyReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Protos.Expert.CommandList), global::Protos.Expert.CommandList.Parser, new[]{ "PlayerNumber", "Commands" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protos.Expert.CommandResultList), global::Protos.Expert.CommandResultList.Parser, new[]{ "PlayerNumber", "Results" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protos.Expert.ResolveConstRequest), global::Protos.Expert.ResolveConstRequest.Parser, new[]{ "ConstToResolve" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protos.Expert.ResolveConstResponse), global::Protos.Expert.ResolveConstResponse.Parser, new[]{ "ConstValue" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  ///*
  /// `ExecuteCommandList` call parameters.
  /// </summary>
  public sealed partial class CommandList : pb::IMessage<CommandList>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<CommandList> _parser = new pb::MessageParser<CommandList>(() => new CommandList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CommandList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protos.Expert.ExpertApiReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandList(CommandList other) : this() {
      playerNumber_ = other.playerNumber_;
      commands_ = other.commands_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandList Clone() {
      return new CommandList(this);
    }

    /// <summary>Field number for the "playerNumber" field.</summary>
    public const int PlayerNumberFieldNumber = 1;
    private int playerNumber_;
    /// <summary>
    /// player number from 1-8 on which to execute the commands, must be an AI player
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int PlayerNumber {
      get { return playerNumber_; }
      set {
        playerNumber_ = value;
      }
    }

    /// <summary>Field number for the "commands" field.</summary>
    public const int CommandsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Google.Protobuf.WellKnownTypes.Any> _repeated_commands_codec
        = pb::FieldCodec.ForMessage(18, global::Google.Protobuf.WellKnownTypes.Any.Parser);
    private readonly pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any> commands_ = new pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any>();
    /// <summary>
    /// list of commands to execute on behalf of that player
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any> Commands {
      get { return commands_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CommandList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CommandList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PlayerNumber != other.PlayerNumber) return false;
      if(!commands_.Equals(other.commands_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (PlayerNumber != 0) hash ^= PlayerNumber.GetHashCode();
      hash ^= commands_.GetHashCode();
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (PlayerNumber != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(PlayerNumber);
      }
      commands_.WriteTo(output, _repeated_commands_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (PlayerNumber != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(PlayerNumber);
      }
      commands_.WriteTo(ref output, _repeated_commands_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (PlayerNumber != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(PlayerNumber);
      }
      size += commands_.CalculateSize(_repeated_commands_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CommandList other) {
      if (other == null) {
        return;
      }
      if (other.PlayerNumber != 0) {
        PlayerNumber = other.PlayerNumber;
      }
      commands_.Add(other.commands_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            PlayerNumber = input.ReadInt32();
            break;
          }
          case 18: {
            commands_.AddEntriesFrom(input, _repeated_commands_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            PlayerNumber = input.ReadInt32();
            break;
          }
          case 18: {
            commands_.AddEntriesFrom(ref input, _repeated_commands_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  ///*
  /// `ExecuteCommandList` call reply.
  /// </summary>
  public sealed partial class CommandResultList : pb::IMessage<CommandResultList>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<CommandResultList> _parser = new pb::MessageParser<CommandResultList>(() => new CommandResultList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CommandResultList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protos.Expert.ExpertApiReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandResultList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandResultList(CommandResultList other) : this() {
      playerNumber_ = other.playerNumber_;
      results_ = other.results_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CommandResultList Clone() {
      return new CommandResultList(this);
    }

    /// <summary>Field number for the "playerNumber" field.</summary>
    public const int PlayerNumberFieldNumber = 1;
    private int playerNumber_;
    /// <summary>
    /// player number from 1-8 on which the commands were executed on
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int PlayerNumber {
      get { return playerNumber_; }
      set {
        playerNumber_ = value;
      }
    }

    /// <summary>Field number for the "results" field.</summary>
    public const int ResultsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Google.Protobuf.WellKnownTypes.Any> _repeated_results_codec
        = pb::FieldCodec.ForMessage(18, global::Google.Protobuf.WellKnownTypes.Any.Parser);
    private readonly pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any> results_ = new pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any>();
    /// <summary>
    /// list of results for each command in the same order as the originally sent in the command list
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Google.Protobuf.WellKnownTypes.Any> Results {
      get { return results_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CommandResultList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CommandResultList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PlayerNumber != other.PlayerNumber) return false;
      if(!results_.Equals(other.results_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (PlayerNumber != 0) hash ^= PlayerNumber.GetHashCode();
      hash ^= results_.GetHashCode();
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (PlayerNumber != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(PlayerNumber);
      }
      results_.WriteTo(output, _repeated_results_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (PlayerNumber != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(PlayerNumber);
      }
      results_.WriteTo(ref output, _repeated_results_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (PlayerNumber != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(PlayerNumber);
      }
      size += results_.CalculateSize(_repeated_results_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CommandResultList other) {
      if (other == null) {
        return;
      }
      if (other.PlayerNumber != 0) {
        PlayerNumber = other.PlayerNumber;
      }
      results_.Add(other.results_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            PlayerNumber = input.ReadInt32();
            break;
          }
          case 18: {
            results_.AddEntriesFrom(input, _repeated_results_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            PlayerNumber = input.ReadInt32();
            break;
          }
          case 18: {
            results_.AddEntriesFrom(ref input, _repeated_results_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  ///*
  /// Experimental. Currently unsupported.
  /// </summary>
  public sealed partial class ResolveConstRequest : pb::IMessage<ResolveConstRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ResolveConstRequest> _parser = new pb::MessageParser<ResolveConstRequest>(() => new ResolveConstRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ResolveConstRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protos.Expert.ExpertApiReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstRequest(ResolveConstRequest other) : this() {
      constToResolve_ = other.constToResolve_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstRequest Clone() {
      return new ResolveConstRequest(this);
    }

    /// <summary>Field number for the "constToResolve" field.</summary>
    public const int ConstToResolveFieldNumber = 1;
    private string constToResolve_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ConstToResolve {
      get { return constToResolve_; }
      set {
        constToResolve_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ResolveConstRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ResolveConstRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ConstToResolve != other.ConstToResolve) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ConstToResolve.Length != 0) hash ^= ConstToResolve.GetHashCode();
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ConstToResolve.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ConstToResolve);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ConstToResolve.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ConstToResolve);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ConstToResolve.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ConstToResolve);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ResolveConstRequest other) {
      if (other == null) {
        return;
      }
      if (other.ConstToResolve.Length != 0) {
        ConstToResolve = other.ConstToResolve;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ConstToResolve = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ConstToResolve = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  ///*
  /// Experimental. Currently unsupported.
  /// </summary>
  public sealed partial class ResolveConstResponse : pb::IMessage<ResolveConstResponse>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ResolveConstResponse> _parser = new pb::MessageParser<ResolveConstResponse>(() => new ResolveConstResponse());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ResolveConstResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protos.Expert.ExpertApiReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstResponse(ResolveConstResponse other) : this() {
      constValue_ = other.constValue_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ResolveConstResponse Clone() {
      return new ResolveConstResponse(this);
    }

    /// <summary>Field number for the "constValue" field.</summary>
    public const int ConstValueFieldNumber = 1;
    private int constValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ConstValue {
      get { return constValue_; }
      set {
        constValue_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ResolveConstResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ResolveConstResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ConstValue != other.ConstValue) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ConstValue != 0) hash ^= ConstValue.GetHashCode();
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ConstValue != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ConstValue);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ConstValue != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ConstValue);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ConstValue != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ConstValue);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ResolveConstResponse other) {
      if (other == null) {
        return;
      }
      if (other.ConstValue != 0) {
        ConstValue = other.ConstValue;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ConstValue = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ConstValue = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
