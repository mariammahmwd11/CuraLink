using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Exceptions
{
   
        

        public class NotFoundException : Exception
        {
            public NotFoundException(string name, object key)
                : base($"Entity \"{name}\" ({key}) was not found.")
            {
            }

            public NotFoundException(string message)
                : base(message)
            {
            }
        }
    }



