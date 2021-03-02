﻿using MsgPack.Rpc.Core.Client.Protocols;
using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace MsgPack.Rpc.Core.Client {
	// This file generated from RpcClientConfiguration.tt T4Template.
	// Do not modify this file. Edit RpcClientConfiguration.tt instead.

	partial class RpcClientConfiguration {
		bool _preferIPv4 = false;

		/// <summary>
		/// 	Gets or sets whether use IP v4 even when IP v6 is supported.
		/// </summary>
		/// <value>
		/// 	<c>true</c>, use IP v4 anyway; otherwise, <c>false</c>. The default is <c>false</c>.
		/// </value>
		public bool PreferIPv4 {
			get {
				return _preferIPv4;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoercePreferIPv4Value(ref coerced);
				_preferIPv4 = coerced;
			}
		}

		/// <summary>
		/// 	Resets the PreferIPv4 property value.
		/// </summary>
		public void ResetPreferIPv4() {
			_preferIPv4 = false;
		}

		static partial void CoercePreferIPv4Value(ref bool value);

		int _minimumConcurrentRequest = 2;

		/// <summary>
		/// 	Gets or sets the minimum concurrency for the each clients.
		/// </summary>
		/// <value>
		/// 	The minimum concurrency for the each clients. The default is 2.
		/// </value>
		public int MinimumConcurrentRequest {
			get {
				Contract.Ensures(Contract.Result<int>() >= default(int));

				return _minimumConcurrentRequest;
			}
			set {
				if (!(value >= default(int))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument cannot be negative number.");
				}

				Contract.Ensures(Contract.Result<int>() >= default(int));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceMinimumConcurrentRequestValue(ref coerced);
				_minimumConcurrentRequest = coerced;
			}
		}

		/// <summary>
		/// 	Resets the MinimumConcurrentRequest property value.
		/// </summary>
		public void ResetMinimumConcurrentRequest() {
			_minimumConcurrentRequest = 2;
		}

		static partial void CoerceMinimumConcurrentRequestValue(ref int value);

		int _maximumConcurrentRequest = 10;

		/// <summary>
		/// 	Gets or sets the maximum concurrency for the each clients.
		/// </summary>
		/// <value>
		/// 	The maximum concurrency for the each clients. The default is 10.
		/// </value>
		public int MaximumConcurrentRequest {
			get {
				Contract.Ensures(Contract.Result<int>() > default(int));

				return _maximumConcurrentRequest;
			}
			set {
				if (!(value > default(int))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument must be positive number.");
				}

				Contract.Ensures(Contract.Result<int>() > default(int));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceMaximumConcurrentRequestValue(ref coerced);
				_maximumConcurrentRequest = coerced;
			}
		}

		/// <summary>
		/// 	Resets the MaximumConcurrentRequest property value.
		/// </summary>
		public void ResetMaximumConcurrentRequest() {
			_maximumConcurrentRequest = 10;
		}

		static partial void CoerceMaximumConcurrentRequestValue(ref int value);

		TimeSpan? _connectTimeout = TimeSpan.FromSeconds(120);

		/// <summary>
		/// 	Gets or sets the timeout value to connect.
		/// </summary>
		/// <value>
		/// 	The timeout value to connect. The default is 120 seconds. <c>null</c> means inifinite timeout.
		/// </value>
		public TimeSpan? ConnectTimeout {
			get {
				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				return _connectTimeout;
			}
			set {
				if (!(value == null || value.Value > default(TimeSpan))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument must be positive number.");
				}

				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceConnectTimeoutValue(ref coerced);
				_connectTimeout = coerced;
			}
		}

		/// <summary>
		/// 	Resets the ConnectTimeout property value.
		/// </summary>
		public void ResetConnectTimeout() {
			_connectTimeout = TimeSpan.FromSeconds(120);
		}

		static partial void CoerceConnectTimeoutValue(ref TimeSpan? value);

		TimeSpan? _waitTimeout = TimeSpan.FromSeconds(120);

		/// <summary>
		/// 	Gets or sets the timeout value to wait response.
		/// </summary>
		/// <value>
		/// 	The timeout value to wait response. The default is 120 seconds. <c>null</c> means inifinite timeout.
		/// </value>
		public TimeSpan? WaitTimeout {
			get {
				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				return _waitTimeout;
			}
			set {
				if (!(value == null || value.Value > default(TimeSpan))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument must be positive number.");
				}

				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceWaitTimeoutValue(ref coerced);
				_waitTimeout = coerced;
			}
		}

		/// <summary>
		/// 	Resets the WaitTimeout property value.
		/// </summary>
		public void ResetWaitTimeout() {
			_waitTimeout = TimeSpan.FromSeconds(120);
		}

		static partial void CoerceWaitTimeoutValue(ref TimeSpan? value);

		Func<RpcClientConfiguration, ClientTransportManager> _transportManagerProvider = (configuration) => new TcpClientTransportManager(configuration);

		/// <summary>
		/// 	Gets or sets the factory function which creates new <see cref="ClientTransportManager" />.
		/// </summary>
		/// <value>
		/// 	The factory function which creates new <see cref="ClientTransportManager" />. The default is the delegate which creates <see cref="TcpClientTransportManager" /> instance.
		/// </value>
		public Func<RpcClientConfiguration, ClientTransportManager> TransportManagerProvider {
			get {
				Contract.Ensures(Contract.Result<Func<RpcClientConfiguration, ClientTransportManager>>() != null);

				return _transportManagerProvider;
			}
			set {
				if (!(value != null)) {
					throw new ArgumentNullException(nameof(value));
				}

				Contract.Ensures(Contract.Result<Func<RpcClientConfiguration, ClientTransportManager>>() != null);

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceTransportManagerProviderValue(ref coerced);
				_transportManagerProvider = coerced;
			}
		}

		/// <summary>
		/// 	Resets the TransportManagerProvider property value.
		/// </summary>
		public void ResetTransportManagerProvider() {
			_transportManagerProvider = (configuration) => new TcpClientTransportManager(configuration);
		}

		static partial void CoerceTransportManagerProviderValue(ref Func<RpcClientConfiguration, ClientTransportManager> value);

		int _initialMethodNameBufferLength = 256;

		/// <summary>
		/// 	Gets or sets the initial buffer size to pack method name.
		/// </summary>
		/// <value>
		/// 	The initial buffer size to pack method name. The default is <c>256</c>.
		/// </value>
		public int InitialMethodNameBufferLength {
			get {
				return _initialMethodNameBufferLength;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceInitialMethodNameBufferLengthValue(ref coerced);
				_initialMethodNameBufferLength = coerced;
			}
		}

		/// <summary>
		/// 	Resets the InitialMethodNameBufferLength property value.
		/// </summary>
		public void ResetInitialMethodNameBufferLength() {
			_initialMethodNameBufferLength = 256;
		}

		static partial void CoerceInitialMethodNameBufferLengthValue(ref int value);

		int _initialArgumentsBufferLength = 65536;

		/// <summary>
		/// 	Gets or sets the initial buffer size to pack arguments.
		/// </summary>
		/// <value>
		/// 	The initial buffer size to pack arguments. The default is <c>65536</c>.
		/// </value>
		public int InitialArgumentsBufferLength {
			get {
				return _initialArgumentsBufferLength;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceInitialArgumentsBufferLengthValue(ref coerced);
				_initialArgumentsBufferLength = coerced;
			}
		}

		/// <summary>
		/// 	Resets the InitialArgumentsBufferLength property value.
		/// </summary>
		public void ResetInitialArgumentsBufferLength() {
			_initialArgumentsBufferLength = 65536;
		}

		static partial void CoerceInitialArgumentsBufferLengthValue(ref int value);

		int _initialReceiveBufferLength = 65536;

		/// <summary>
		/// 	Gets or sets the initial buffer size to receive packets.
		/// </summary>
		/// <value>
		/// 	The initial buffer size to receive packets. The default is <c>65536</c>.
		/// </value>
		public int InitialReceiveBufferLength {
			get {
				return _initialReceiveBufferLength;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceInitialReceiveBufferLengthValue(ref coerced);
				_initialReceiveBufferLength = coerced;
			}
		}

		/// <summary>
		/// 	Resets the InitialReceiveBufferLength property value.
		/// </summary>
		public void ResetInitialReceiveBufferLength() {
			_initialReceiveBufferLength = 65536;
		}

		static partial void CoerceInitialReceiveBufferLengthValue(ref int value);

		bool _dumpCorruptResponse = false;

		/// <summary>
		/// 	Gets or sets the value whether transport should dump invalid response stream for debugging purposes.
		/// </summary>
		/// <value>
		/// 	<c>true</c>, if the corrupt response dumping is enabled; <c>false</c>, otherwise.
		/// </value>
		public bool DumpCorruptResponse {
			get {
				return _dumpCorruptResponse;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceDumpCorruptResponseValue(ref coerced);
				_dumpCorruptResponse = coerced;
			}
		}

		/// <summary>
		/// 	Resets the DumpCorruptResponse property value.
		/// </summary>
		public void ResetDumpCorruptResponse() {
			_dumpCorruptResponse = false;
		}

		static partial void CoerceDumpCorruptResponseValue(ref bool value);

		string _corruptResponseDumpOutputDirectory = null;

		/// <summary>
		/// 	Gets or sets the directory path the corrupt response dump file is created.
		/// </summary>
		/// <value>
		/// 	The directory path the corrupt response dump file is created. To use default location, specify <c>null</c>. The default is <c>null</c>. The enviroment variable can be embeded.
		/// </value>
		/// <remarks>
		/// 	The default location is <c>MsgPack\v&lt;MsgPack.Rpc.Core.Client AssemblyVersion&lt;\Client\Dump</c> under the <c>LocalApplicationDataDirectory</c>.
		/// 	The <c>LocalApplicaitonDataDirectory</c> can be gotten using <see cref="Environment.GetFolderPath(Environment.SpecialFolder)" /> with <see cref="Environment.SpecialFolder.LocalApplicationData" />, which is platform dependent.
		/// 	For example, it is <c>%UserProfile%\Application Data\Local</c> in Windows prior to Windows Vista/Server 2008, <c>%LocalAppData%</c> on recent Windows.
		/// </remarks>
		public string CorruptResponseDumpOutputDirectory {
			get {
				return _corruptResponseDumpOutputDirectory;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceCorruptResponseDumpOutputDirectoryValue(ref coerced);
				_corruptResponseDumpOutputDirectory = coerced;
			}
		}

		/// <summary>
		/// 	Resets the CorruptResponseDumpOutputDirectory property value.
		/// </summary>
		public void ResetCorruptResponseDumpOutputDirectory() {
			_corruptResponseDumpOutputDirectory = null;
		}

		static partial void CoerceCorruptResponseDumpOutputDirectoryValue(ref string value);

		Func<Func<ClientRequestContext>, ObjectPoolConfiguration, ObjectPool<ClientRequestContext>> _requestContextPoolProvider = (factory, configuration) => new StandardObjectPool<ClientRequestContext>(factory, configuration);

		/// <summary>
		/// 	Gets or sets the factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="ClientRequestContext" />.
		/// </summary>
		/// <value>
		/// 	The factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="ClientRequestContext" />. The default is the delegate which creates <see cref="StandardObjectPool{T}" /> instance with <c>null</c> configuration.
		/// </value>
		public Func<Func<ClientRequestContext>, ObjectPoolConfiguration, ObjectPool<ClientRequestContext>> RequestContextPoolProvider {
			get {
				Contract.Ensures(Contract.Result<Func<Func<ClientRequestContext>, ObjectPoolConfiguration, ObjectPool<ClientRequestContext>>>() != null);

				return _requestContextPoolProvider;
			}
			set {
				if (!(value != null)) {
					throw new ArgumentNullException(nameof(value));
				}

				Contract.Ensures(Contract.Result<Func<Func<ClientRequestContext>, ObjectPoolConfiguration, ObjectPool<ClientRequestContext>>>() != null);

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceRequestContextPoolProviderValue(ref coerced);
				_requestContextPoolProvider = coerced;
			}
		}

		/// <summary>
		/// 	Resets the RequestContextPoolProvider property value.
		/// </summary>
		public void ResetRequestContextPoolProvider() {
			_requestContextPoolProvider = (factory, configuration) => new StandardObjectPool<ClientRequestContext>(factory, configuration);
		}

		static partial void CoerceRequestContextPoolProviderValue(ref Func<Func<ClientRequestContext>, ObjectPoolConfiguration, ObjectPool<ClientRequestContext>> value);

		Func<Func<ClientResponseContext>, ObjectPoolConfiguration, ObjectPool<ClientResponseContext>> _responseContextPoolProvider = (factory, configuration) => new StandardObjectPool<ClientResponseContext>(factory, configuration);

		/// <summary>
		/// 	Gets or sets the factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="ClientResponseContext" />.
		/// </summary>
		/// <value>
		/// 	The factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="ClientResponseContext" />. The default is the delegate which creates <see cref="StandardObjectPool{T}" /> instance with <c>null</c> configuration.
		/// </value>
		public Func<Func<ClientResponseContext>, ObjectPoolConfiguration, ObjectPool<ClientResponseContext>> ResponseContextPoolProvider {
			get {
				Contract.Ensures(Contract.Result<Func<Func<ClientResponseContext>, ObjectPoolConfiguration, ObjectPool<ClientResponseContext>>>() != null);

				return _responseContextPoolProvider;
			}
			set {
				if (!(value != null)) {
					throw new ArgumentNullException(nameof(value));
				}

				Contract.Ensures(Contract.Result<Func<Func<ClientResponseContext>, ObjectPoolConfiguration, ObjectPool<ClientResponseContext>>>() != null);

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceResponseContextPoolProviderValue(ref coerced);
				_responseContextPoolProvider = coerced;
			}
		}

		/// <summary>
		/// 	Resets the ResponseContextPoolProvider property value.
		/// </summary>
		public void ResetResponseContextPoolProvider() {
			_responseContextPoolProvider = (factory, configuration) => new StandardObjectPool<ClientResponseContext>(factory, configuration);
		}

		static partial void CoerceResponseContextPoolProviderValue(ref Func<Func<ClientResponseContext>, ObjectPoolConfiguration, ObjectPool<ClientResponseContext>> value);

		Func<Func<TcpClientTransport>, ObjectPoolConfiguration, ObjectPool<TcpClientTransport>> _tcpTransportPoolProvider = (factory, configuration) => new StandardObjectPool<TcpClientTransport>(factory, configuration);

		/// <summary>
		/// 	Gets or sets the factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="TcpClientTransport" />.
		/// </summary>
		/// <value>
		/// 	The factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="TcpClientTransport" />. The default is the delegate which creates <see cref="StandardObjectPool{T}" /> instance with <c>null</c> configuration.
		/// </value>
		public Func<Func<TcpClientTransport>, ObjectPoolConfiguration, ObjectPool<TcpClientTransport>> TcpTransportPoolProvider {
			get {
				Contract.Ensures(Contract.Result<Func<Func<TcpClientTransport>, ObjectPoolConfiguration, ObjectPool<TcpClientTransport>>>() != null);

				return _tcpTransportPoolProvider;
			}
			set {
				if (!(value != null)) {
					throw new ArgumentNullException(nameof(value));
				}

				Contract.Ensures(Contract.Result<Func<Func<TcpClientTransport>, ObjectPoolConfiguration, ObjectPool<TcpClientTransport>>>() != null);

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceTcpTransportPoolProviderValue(ref coerced);
				_tcpTransportPoolProvider = coerced;
			}
		}

		/// <summary>
		/// 	Resets the TcpTransportPoolProvider property value.
		/// </summary>
		public void ResetTcpTransportPoolProvider() {
			_tcpTransportPoolProvider = (factory, configuration) => new StandardObjectPool<TcpClientTransport>(factory, configuration);
		}

		static partial void CoerceTcpTransportPoolProviderValue(ref Func<Func<TcpClientTransport>, ObjectPoolConfiguration, ObjectPool<TcpClientTransport>> value);

		Func<Func<UdpClientTransport>, ObjectPoolConfiguration, ObjectPool<UdpClientTransport>> _udpTransportPoolProvider = (factory, configuration) => new StandardObjectPool<UdpClientTransport>(factory, configuration);

		/// <summary>
		/// 	Gets or sets the factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="UdpClientTransport" />.
		/// </summary>
		/// <value>
		/// 	The factory function which creates new <see cref="ObjectPool{T}" /> of <see cref="UdpClientTransport" />. The default is the delegate which creates <see cref="StandardObjectPool{T}" /> instance with <c>null</c> configuration.
		/// </value>
		public Func<Func<UdpClientTransport>, ObjectPoolConfiguration, ObjectPool<UdpClientTransport>> UdpTransportPoolProvider {
			get {
				Contract.Ensures(Contract.Result<Func<Func<UdpClientTransport>, ObjectPoolConfiguration, ObjectPool<UdpClientTransport>>>() != null);

				return _udpTransportPoolProvider;
			}
			set {
				if (!(value != null)) {
					throw new ArgumentNullException(nameof(value));
				}

				Contract.Ensures(Contract.Result<Func<Func<UdpClientTransport>, ObjectPoolConfiguration, ObjectPool<UdpClientTransport>>>() != null);

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceUdpTransportPoolProviderValue(ref coerced);
				_udpTransportPoolProvider = coerced;
			}
		}

		/// <summary>
		/// 	Resets the UdpTransportPoolProvider property value.
		/// </summary>
		public void ResetUdpTransportPoolProvider() {
			_udpTransportPoolProvider = (factory, configuration) => new StandardObjectPool<UdpClientTransport>(factory, configuration);
		}

		static partial void CoerceUdpTransportPoolProviderValue(ref Func<Func<UdpClientTransport>, ObjectPoolConfiguration, ObjectPool<UdpClientTransport>> value);

		/// <summary>
		/// 	Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// 	A string that represents the current object.
		/// </returns>
		public sealed override string ToString() {
			var buffer = new StringBuilder(4096);
			buffer.Append("{ ");
			buffer.Append("\"PreferIPv4\" : ");
			ToString(PreferIPv4, buffer);
			buffer.Append(", ");
			buffer.Append("\"MinimumConcurrentRequest\" : ");
			ToString(MinimumConcurrentRequest, buffer);
			buffer.Append(", ");
			buffer.Append("\"MaximumConcurrentRequest\" : ");
			ToString(MaximumConcurrentRequest, buffer);
			buffer.Append(", ");
			buffer.Append("\"ConnectTimeout\" : ");
			ToString(ConnectTimeout, buffer);
			buffer.Append(", ");
			buffer.Append("\"WaitTimeout\" : ");
			ToString(WaitTimeout, buffer);
			buffer.Append(", ");
			buffer.Append("\"TransportManagerProvider\" : ");
			ToString(TransportManagerProvider, buffer);
			buffer.Append(", ");
			buffer.Append("\"InitialMethodNameBufferLength\" : ");
			ToString(InitialMethodNameBufferLength, buffer);
			buffer.Append(", ");
			buffer.Append("\"InitialArgumentsBufferLength\" : ");
			ToString(InitialArgumentsBufferLength, buffer);
			buffer.Append(", ");
			buffer.Append("\"InitialReceiveBufferLength\" : ");
			ToString(InitialReceiveBufferLength, buffer);
			buffer.Append(", ");
			buffer.Append("\"DumpCorruptResponse\" : ");
			ToString(DumpCorruptResponse, buffer);
			buffer.Append(", ");
			buffer.Append("\"CorruptResponseDumpOutputDirectory\" : ");
			ToString(CorruptResponseDumpOutputDirectory, buffer);
			buffer.Append(", ");
			buffer.Append("\"RequestContextPoolProvider\" : ");
			ToString(RequestContextPoolProvider, buffer);
			buffer.Append(", ");
			buffer.Append("\"ResponseContextPoolProvider\" : ");
			ToString(ResponseContextPoolProvider, buffer);
			buffer.Append(", ");
			buffer.Append("\"TcpTransportPoolProvider\" : ");
			ToString(TcpTransportPoolProvider, buffer);
			buffer.Append(", ");
			buffer.Append("\"UdpTransportPoolProvider\" : ");
			ToString(UdpTransportPoolProvider, buffer);
			buffer.Append(" }");
			return buffer.ToString();
		}

		static void ToString<T>(T value, StringBuilder buffer) {
			if (value == null) {
				buffer.Append("null");
			}

			if (typeof(Delegate).IsAssignableFrom(typeof(T))) {
				var asDelegate = (Delegate)(object)value;
				buffer.Append("\"Type='").Append(asDelegate.Method.DeclaringType);

				if (asDelegate.Target != null) {
					buffer.Append("', Instance='").Append(asDelegate.Target);
				}

				buffer.Append("', Method='").Append(asDelegate.Method).Append("'\"");
				return;
			}

			switch (Type.GetTypeCode(typeof(T))) {
				case TypeCode.Boolean: {
					buffer.Append(value.ToString().ToLowerInvariant());
					break;
				}
				case TypeCode.Byte:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: {
					buffer.Append(value.ToString());
					break;
				}
				default: {
					buffer.Append('"').Append(value.ToString()).Append('"');
					break;
				}
			}
		}
	}
}
