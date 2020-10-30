// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: protos/expert_api.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Protos.Expert {
  public static partial class ExpertAPI
  {
    static readonly string __ServiceName = "protos.expert.ExpertAPI";

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

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

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

    static readonly grpc::Marshaller<global::Protos.Expert.CommandList> __Marshaller_protos_expert_CommandList = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.Expert.CommandList.Parser));
    static readonly grpc::Marshaller<global::Protos.Expert.CommandResultList> __Marshaller_protos_expert_CommandResultList = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.Expert.CommandResultList.Parser));
    static readonly grpc::Marshaller<global::Protos.Expert.ResolveConstRequest> __Marshaller_protos_expert_ResolveConstRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.Expert.ResolveConstRequest.Parser));
    static readonly grpc::Marshaller<global::Protos.Expert.ResolveConstResponse> __Marshaller_protos_expert_ResolveConstResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Protos.Expert.ResolveConstResponse.Parser));

    static readonly grpc::Method<global::Protos.Expert.CommandList, global::Protos.Expert.CommandResultList> __Method_ExecuteCommandList = new grpc::Method<global::Protos.Expert.CommandList, global::Protos.Expert.CommandResultList>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ExecuteCommandList",
        __Marshaller_protos_expert_CommandList,
        __Marshaller_protos_expert_CommandResultList);

    static readonly grpc::Method<global::Protos.Expert.ResolveConstRequest, global::Protos.Expert.ResolveConstResponse> __Method_ResolveConst = new grpc::Method<global::Protos.Expert.ResolveConstRequest, global::Protos.Expert.ResolveConstResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ResolveConst",
        __Marshaller_protos_expert_ResolveConstRequest,
        __Marshaller_protos_expert_ResolveConstResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Protos.Expert.ExpertApiReflection.Descriptor.Services[0]; }
    }

    /// <summary>Client for ExpertAPI</summary>
    public partial class ExpertAPIClient : grpc::ClientBase<ExpertAPIClient>
    {
      /// <summary>Creates a new client for ExpertAPI</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public ExpertAPIClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for ExpertAPI that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public ExpertAPIClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected ExpertAPIClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected ExpertAPIClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      ///*
      /// Send a list of expert actions and facts (commands) to the AI Module. The list will be processed during the next AI tick for the specified player. If that does not occur in 5 seconds, an error code will be returned.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::Protos.Expert.CommandResultList ExecuteCommandList(global::Protos.Expert.CommandList request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ExecuteCommandList(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Send a list of expert actions and facts (commands) to the AI Module. The list will be processed during the next AI tick for the specified player. If that does not occur in 5 seconds, an error code will be returned.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::Protos.Expert.CommandResultList ExecuteCommandList(global::Protos.Expert.CommandList request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ExecuteCommandList, null, options, request);
      }
      /// <summary>
      ///*
      /// Send a list of expert actions and facts (commands) to the AI Module. The list will be processed during the next AI tick for the specified player. If that does not occur in 5 seconds, an error code will be returned.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::Protos.Expert.CommandResultList> ExecuteCommandListAsync(global::Protos.Expert.CommandList request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ExecuteCommandListAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Send a list of expert actions and facts (commands) to the AI Module. The list will be processed during the next AI tick for the specified player. If that does not occur in 5 seconds, an error code will be returned.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::Protos.Expert.CommandResultList> ExecuteCommandListAsync(global::Protos.Expert.CommandList request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ExecuteCommandList, null, options, request);
      }
      /// <summary>
      ///*
      /// Experimental. Currently unsupported.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::Protos.Expert.ResolveConstResponse ResolveConst(global::Protos.Expert.ResolveConstRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ResolveConst(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Experimental. Currently unsupported.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      public virtual global::Protos.Expert.ResolveConstResponse ResolveConst(global::Protos.Expert.ResolveConstRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ResolveConst, null, options, request);
      }
      /// <summary>
      ///*
      /// Experimental. Currently unsupported.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::Protos.Expert.ResolveConstResponse> ResolveConstAsync(global::Protos.Expert.ResolveConstRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ResolveConstAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      ///*
      /// Experimental. Currently unsupported.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      public virtual grpc::AsyncUnaryCall<global::Protos.Expert.ResolveConstResponse> ResolveConstAsync(global::Protos.Expert.ResolveConstRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ResolveConst, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override ExpertAPIClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new ExpertAPIClient(configuration);
      }
    }

  }
}
#endregion
