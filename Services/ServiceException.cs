using System;
using System.Runtime.Serialization;

namespace Services
{
    [Serializable]
    public abstract class ServiceException : Exception, ISerializable
    {
        protected ServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        protected ServiceException(string message) : base(message)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Message", Message);
            info.AddValue("StackTrace", StackTrace);
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class ConflictException : ServiceException
    {
        public ConflictException(string message) : base(message)
        {

        }

        protected ConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class NotFoundException : ServiceException
    {
        public NotFoundException(string message) : base(message)
        {

        }

        protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class BadRequestException : ServiceException
    {
        public BadRequestException(string message) : base(message)
        {

        }

        protected BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
