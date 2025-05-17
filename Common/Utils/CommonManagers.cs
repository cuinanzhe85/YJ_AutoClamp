using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Utils
{

	public static class NumberUtil
	{
		public static bool AreSame(double lhs, double rhs)
		{
			const double tolerance = 1E-8;

			return Math.Abs((lhs - rhs) / lhs) < tolerance
				|| Math.Abs(lhs - rhs) < tolerance;
		}
	}


	#region Parser

	public enum SerializeFormats
	{
		/// <summary>
		/// Json
		/// </summary>
		Json,

		/// <summary>
		/// Xml
		/// </summary>
		Xml,

		/// <summary>
		/// Binary
		/// </summary>
		Binary
	}

	/// <summary>
	/// Encoding Set StringWriter
	/// </summary>
	public class EncodingStringWriter : StringWriter
	{
		private readonly Encoding _encoding = Encoding.UTF8;
		public EncodingStringWriter(Encoding encoding)
		{
			this._encoding = encoding;
		}
		public override Encoding Encoding { get { return this._encoding; } }
	}

	/// <summary>
	/// Serializer Util Class
	/// </summary>
	public static class Serializer
	{
		public static void SerializeEncryptExport<T>(string FullName, string EncryptName, T Data, SerializeFormats formats)
		{
			string data = Serialize<T>(Data, formats);

			// serialize JSON to a string and then write string to a file
#pragma warning disable SG0018 // Path traversal
			File.WriteAllText(FullName, data);
#pragma warning restore SG0018 // Path traversal

			switch (formats)
			{
				case SerializeFormats.Json:
					// serialize JSON directly to a file
					using (StreamWriter file = File.CreateText(FullName))
					{
						JsonSerializer serializer = new JsonSerializer();
						serializer.Serialize(file, Data);
					}
					break;
				case SerializeFormats.Xml:
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(data);
					xmlDocument.Save(FullName);
					break;
				case SerializeFormats.Binary:
					break;
				default:
					break;
			}

			// 파일을 암호화 한다.
			//FileManager.FileEncryptExport(FullName, EncryptName);
		}

		/// <summary>
		/// Serialize from T Type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static string Serialize<T>(T model, SerializeFormats format)
		{
			switch (format)
			{
				case SerializeFormats.Json:
					return SerializeByJson(model);

				case SerializeFormats.Xml:
					return SerializeByXml(model);

				case SerializeFormats.Binary:
					return SerializeByBinary(model);
			}

			return null;
		}

		/// <summary>
		/// Json Deserialize
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string SerializeByJson<T>(T model)
		{
			return JsonConvert.SerializeObject(model);
		}

		/// <summary>
		/// Xml Deserialize
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string SerializeByXml<T>(T model)
		{
			var serializer = new XmlSerializer(model.GetType());

			using (StringWriter sw = new EncodingStringWriter(Encoding.UTF8))
			{
				//Serialize
				serializer.Serialize(sw, model);
				return sw.ToString();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns></returns>
		private static string SerializeByBinary<T>(T model)
		{
			using (var ms = new MemoryStream())
			{
				var bf = new BinaryFormatter();
				bf.Serialize(ms, model);
				ms.Position = 0;
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		/// <summary>
		/// Deserialize to T Type (get stream)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static T Deserialize<T>(Stream stream, SerializeFormats format)
		{
			using (var reader = new StreamReader(stream))
			{
				var getString = reader.ReadToEnd();

				switch (format)
				{
					case SerializeFormats.Json:
						return DeserializeByJson<T>(getString);

					case SerializeFormats.Xml:
						return DeserializeByXml<T>(getString);

					case SerializeFormats.Binary:
						break;
				}
			}

			return default(T);
		}

		/// <summary>
		/// Deserialize to T Type (get string)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, SerializeFormats format)
		{
			try
			{
				switch (format)
				{
					case SerializeFormats.Json:
						return DeserializeByJson<T>(value);

					case SerializeFormats.Xml:
						return DeserializeByXml<T>(value);

					case SerializeFormats.Binary:
						return DeserializeByBinary<T>(value);
				}

				return default(T);
			}
			catch (Exception e)
			{
				LogManager.WriteExceptionLog("Deserialize Fail!!" + e.ToString());
				return default(T);
			}
		}

		/// <summary>
		/// Json Deserialize
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private static T DeserializeByJson<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value);
		}

		/// <summary>
		/// Xml Deserialize
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		private static T DeserializeByXml<T>(string value)
		{
			var serializer = new XmlSerializer(typeof(T));

			using (var stringReader = new StringReader(value))
			{
				using (var xmlReader = new XmlTextReader(stringReader))
				{
					return (T)serializer.Deserialize(xmlReader);
				}
			}
		}

		private static T DeserializeByBinary<T>(string value)
		{
			var bytes = Convert.FromBase64String(value);

			using (var ms = new MemoryStream(bytes))
			{
				return (T)new BinaryFormatter().Deserialize(ms);
			}
		}

		public static string EnumToXmlString<TEnum>(TEnum value) where TEnum : struct, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (!enumType.IsEnum)
				return null;

			MemberInfo member = enumType.GetMember(value.ToString()).FirstOrDefault();
			if (member == null)
				return null;

			XmlEnumAttribute attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
			if (attribute == null)
				return member.Name; // Fallback to the member name when there's no attribute

			return attribute.Name;
		}

		public static TEnum XmlStringToEnum<TEnum>(String value) where TEnum : struct, IConvertible
		{
			string name = string.Empty;
			Type enumType = typeof(TEnum);
			try
			{
				foreach (var item in enumType.GetEnumValues())
				{
					MemberInfo member = enumType.GetMember(item.ToString()).FirstOrDefault();
					XmlEnumAttribute attribute = (member as MemberInfo).GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
					if (value == attribute.Name)
					{
						name = item.ToString();
						break;
					}
				}
			}
			catch (Exception)
			{
				name = enumType.GetEnumValues().OfType<TEnum>().FirstOrDefault().ToString();
			}

			TEnum @enum;
			Enum.TryParse<TEnum>(name, out @enum);
			return @enum;
		}
	}

	#endregion //Parser
}
