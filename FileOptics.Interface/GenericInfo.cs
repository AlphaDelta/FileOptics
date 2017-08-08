using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public class GenericInfo
    {
        public string Title;
        public string Body;

        public GenericInfo(string Title, string Body)
        {
            this.Title = Title;
            this.Body = Body;
        }
    }
}
