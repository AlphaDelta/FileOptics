using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public enum DataType
    {
        /// <summary>
        /// Critically necessary, cannot be marked for deletion.
        /// </summary>
        Critical,
        /// <summary>
        /// Provides data that compliments or changes the meaning of the file in some way; not critically necessary while still providing some sort of function that may complement critical data.
        /// </summary>
        Ancillary,
        /// <summary>
        /// Provides no function to the primary purpose of the file other than information.
        /// </summary>
        Metadata,
        /// <summary>
        /// Provides no function or information.
        /// </summary>
        Useless,
        /// <summary>
        /// Used only for information about an exception that occurred while attempting to read and parse the file, it should not necessarily reference any actual data in the file.
        /// </summary>
        Error
    }
}
