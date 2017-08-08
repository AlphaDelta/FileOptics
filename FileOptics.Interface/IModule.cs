using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.Interface
{
    public interface IModule
    {
        /// <summary>
        /// Returns true if the module is able to parse the data in the specified stream.
        /// </summary>
        /// <param name="stream">Stream that contains the data to test against</param>
        bool CanRead(Stream stream);

        /// <summary>
        /// Reads and parses data from the specified stream into the FileOptics tree view
        /// </summary>
        /// <param name="stream">Stream that contains data to read and parse</param>
        /// <returns>True if the contents of the whole stream were successfully read and parsed, false if not all of the data could not be accounted for</returns>
        bool Read(RootInfoNode root, Stream stream);

        /// <summary>
        /// Writes all data not marked for deletion from the specified RootInfoNode to the specified Stream.
        /// <para>Some files contain a ToC (Table of Contents) or hard-written data lengths that need to be rewritten for the files to be vaild.</para>
        /// </summary>
        void Write(RootInfoNode root, Stream sout);
    }
}
