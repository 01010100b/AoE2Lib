using System;

namespace MsgPack.Rpc.Core {
	internal static class MessagePackObjectExtension {
		public static string GetString(this MessagePackObject source, MessagePackObject key) {
			if (source.IsDictionary) {
				if (source.AsDictionary().TryGetValue(key, out var value) && value.IsTypeOf<string>().GetValueOrDefault()) {
					return value.AsString();
				}
			}

			return null;
		}

		public static TimeSpan? GetTimeSpan(this MessagePackObject source, MessagePackObject key) {
			if (source.IsDictionary) {
				if (source.AsDictionary().TryGetValue(key, out var value) && value.IsTypeOf<long>().GetValueOrDefault()) {
					return new TimeSpan(value.AsInt64());
				}
			}

			return null;
		}
	}
}
