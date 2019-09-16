using System;
using System.IO;
using JetBrains.Annotations;

namespace Pomelo.WoW.SRP6
{
	/// <summary>
	/// Provide helper method for <see cref="MemoryStream"/>. This class is static.
	/// </summary>
	public static class MemoryStreamHelper
	{
		/// <summary>
		/// Write an entire byte sequence into the memory stream.
		/// </summary>
		/// <param name="stream">The memory stream.</param>
		/// <param name="buffer">The buffer to be written.</param>
		public static void Write([NotNull] this MemoryStream stream, [NotNull] byte[] buffer)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			stream.Write(buffer, 0, buffer.Length);
		}
	}
}