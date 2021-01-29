using System;
using System.Collections.Generic;
using System.Text;

namespace BlenderAWS.Exceptions
{
    public class RemoteConfigNotFoundException : Exception
    {
        public RemoteConfigNotFoundException(string message) : base(message)
        {

        }
    }
}
