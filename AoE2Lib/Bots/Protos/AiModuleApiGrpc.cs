// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Bots/Protos/ai_module_api.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Protos {
  public static partial class AIModuleAPI
  {
    static readonly string __ServiceName = "protos.AIModuleAPI";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.GetMatchStatusRequest> __Marshaller_protos_GetMatchStatusRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.GetMatchStatusRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.GetMatchStatusReply> __Marshaller_protos_GetMatchStatusReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.GetMatchStatusReply.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.GetGameDataFilePathRequest> __Marshaller_protos_GetGameDataFilePathRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.GetGameDataFilePathRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.GetGameDataFilePathReply> __Marshaller_protos_GetGameDataFilePathReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.GetGameDataFilePathReply.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.UnloadRequest> __Marshaller_protos_UnloadRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.UnloadRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Protos.UnloadReply> __Marshaller_protos_UnloadReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.UnloadReply.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Protos.GetMatchStatusRequest, global::Protos.GetMatchStatusReply> __Method_GetMatchStatus = new grpc::Method<global::Protos.GetMatchStatusRequest, global::Protos.GetMatchStatusReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetMatchStatus",
        __Marshaller_protos_GetMatchStatusRequest,
        __Marshaller_protos_GetMatchStatusReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Protos.GetGameDataFilePathRequest, global::Protos.GetGameDataFilePathReply> __Method_GetGameDataFilePath = new grpc::Method<global::Protos.GetGameDataFilePathRequest, global::Protos.GetGameDataFilePathReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetGameDataFilePath",
        __Marshaller_protos_GetGameDataFilePathRequest,
        __Marshaller_protos_GetGameDataFilePathReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Protos.UnloadRequest, global::Protos.UnloadReply> __Method_Unload = new grpc::Method<global::Protos.UnloadRequest, global::Protos.UnloadReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Unload",
        __Marshaller_protos_UnloadRequest,
        __Marshaller_protos_UnloadReply);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Protos.AiModuleApiReflection.Descriptor.Services[0]; }
    }

    /// <summary>Client for AIModuleAPI</summary>
    public partial class AIModuleAPIClient : grpc::ClientBase<AIModuleAPIClient>
    {
      /// <summary>Creates a new client for AIModuleAPI</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public AIModuleAPIClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for AIModuleAPI that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public AIModuleAPIClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected AIModuleAPIClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected AIModuleAPIClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      ///*
      /// Get the current match status to determine if match has been started or finished.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.GetMatchStatusReply GetMatchStatus(global::Protos.GetMatchStatusRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetMatchStatus(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Get the current match status to determine if match has been started or finished.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.GetMatchStatusReply GetMatchStatus(global::Protos.GetMatchStatusRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetMatchStatus, null, options, request);
      }
      /// <summary>
      ///*
      /// Get the current match status to determine if match has been started or finished.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.GetMatchStatusReply> GetMatchStatusAsync(global::Protos.GetMatchStatusRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetMatchStatusAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Get the current match status to determine if match has been started or finished.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.GetMatchStatusReply> GetMatchStatusAsync(global::Protos.GetMatchStatusRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetMatchStatus, null, options, request);
      }
      /// <summary>
      ///*
      /// Get the path to the game data (.dat) file that is being used by the game. Only available on AoC.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.GetGameDataFilePathReply GetGameDataFilePath(global::Protos.GetGameDataFilePathRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetGameDataFilePath(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Get the path to the game data (.dat) file that is being used by the game. Only available on AoC.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.GetGameDataFilePathReply GetGameDataFilePath(global::Protos.GetGameDataFilePathRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetGameDataFilePath, null, options, request);
      }
      /// <summary>
      ///*
      /// Get the path to the game data (.dat) file that is being used by the game. Only available on AoC.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.GetGameDataFilePathReply> GetGameDataFilePathAsync(global::Protos.GetGameDataFilePathRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetGameDataFilePathAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Get the path to the game data (.dat) file that is being used by the game. Only available on AoC.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.GetGameDataFilePathReply> GetGameDataFilePathAsync(global::Protos.GetGameDataFilePathRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetGameDataFilePath, null, options, request);
      }
      /// <summary>
      ///*
      /// Cancel any pending RPC calls, close the RPC server, remove detours and detach the AI Module from the game process.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.UnloadReply Unload(global::Protos.UnloadRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Unload(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Cancel any pending RPC calls, close the RPC server, remove detours and detach the AI Module from the game process.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Protos.UnloadReply Unload(global::Protos.UnloadRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Unload, null, options, request);
      }
      /// <summary>
      ///*
      /// Cancel any pending RPC calls, close the RPC server, remove detours and detach the AI Module from the game process.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.UnloadReply> UnloadAsync(global::Protos.UnloadRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return UnloadAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Cancel any pending RPC calls, close the RPC server, remove detours and detach the AI Module from the game process.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Protos.UnloadReply> UnloadAsync(global::Protos.UnloadRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Unload, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override AIModuleAPIClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new AIModuleAPIClient(configuration);
      }
    }

  }
}
#endregion
