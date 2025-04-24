using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataExtractor.DataStructures {
	public class IgnorePropertiesResolver : DefaultContractResolver {
		private readonly HashSet<Type> _typesToIgnore = new();
		private readonly HashSet<string> _generalFieldsToIgnore = new();
		private readonly Dictionary<Type, HashSet<string>> _typeFieldsToIgnore = new();

		public void Ignore<T>() {
			_typesToIgnore.Add(typeof(T));
		}

		public void IgnoreProperty(params string[] propertyNames) {
			foreach (var prop in propertyNames)
				_generalFieldsToIgnore.Add(prop);
		}

		public void IgnoreProperty<T>(params string[] propertyNames) {
			if (!_typeFieldsToIgnore.ContainsKey(typeof(T)))
				_typeFieldsToIgnore[typeof(T)] = new HashSet<string>();

			foreach (var prop in propertyNames)
				_typeFieldsToIgnore[typeof(T)].Add(prop);
		}

		private bool IsIgnored(Type type) {
			return _typesToIgnore.Contains(type);
		}

		private bool IsFieldIgnored(string propertyName) {
			return _generalFieldsToIgnore.Contains(propertyName);
		}

		private bool IsFieldIgnored(Type type, string propertyName) {
			if (!_typeFieldsToIgnore.ContainsKey(type))
				return false;

			return _typeFieldsToIgnore[type].Contains(propertyName);
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
			var property = base.CreateProperty(member, memberSerialization);

			if (!property.Writable || IsIgnored(property.DeclaringType) || IsFieldIgnored(property.PropertyName) ||
			    IsFieldIgnored(property.DeclaringType, property.PropertyName)) {
				property.ShouldSerialize = i => false;
				property.Ignored = true;
			}

			return property;
		}
	}
}