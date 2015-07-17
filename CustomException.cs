using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraConfigTool
{
    public enum CustomExceptionType
    {
        Xml,
        Argument,
        Code,
        System
    }

    public class CustomException : Exception
    {
        public CustomException(string message, CustomExceptionType type)
            : base(message)
        {
            this.type = type;
        }

        public CustomException(string message, Exception inner, CustomExceptionType type)
            : base(message, inner)
        {
            this.type = type;
        }

        private CustomExceptionType type;
        public CustomExceptionType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
    }

}