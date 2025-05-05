using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Common.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        
        protected Entity(Guid id)
        {
            Id = id;
        }
        
        protected Entity() { } // For EF Core
        
        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;
                
            if (ReferenceEquals(this, other))
                return true;
                
            if (GetType() != other.GetType())
                return false;
                
            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;
                
            return Id == other.Id;
        }
        
        public static bool operator ==(Entity? left, Entity? right)
        {
            if (left is null && right is null)
                return true;
                
            if (left is null || right is null)
                return false;
                
            return left.Equals(right);
        }
        
        public static bool operator !=(Entity? left, Entity? right)
        {
            return !(left == right);
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}