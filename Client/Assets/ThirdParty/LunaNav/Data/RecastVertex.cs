using System;

namespace LunaNav
{
    public class RecastVertex
    {
        protected float[] Verts;
        public float X { get { return Verts[0]; } set { Verts[0] = value; } }
        public float Y { get { return Verts[1]; } set { Verts[1] = value; } }
        public float Z { get { return Verts[2]; } set { Verts[2] = value; } }

        public RecastVertex()
        {
            Verts = new float[3];
        }

        public RecastVertex(RecastVertex copy) : this()
        {
            X = copy.X;
            Y = copy.Y;
            Z = copy.Z;
        }

        public RecastVertex(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public RecastVertex(float[] inArray) : this()
        {
            if (inArray.Length < 3)
                throw new ArgumentOutOfRangeException();
            X = inArray[0];
            Y = inArray[1];
            Z = inArray[2];
        }

        public float[] ToArray()
        {
            return Verts;
        }

        public float this[int i]
        {
            get { return Verts[i]; }
            set { Verts[i] = value; }
        }

        #region Equality Code
        public bool Equals(RecastVertex other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(RecastVertex)) return false;
            return Equals((RecastVertex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X.GetHashCode();
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        #endregion

        public static RecastVertex Add(RecastVertex left, RecastVertex right)
        {
            return new RecastVertex
                       {
                           X = left.X + right.X,
                           Y = left.Y + right.Y,
                           Z = left.Z + right.Z
                       };
        }

        public static RecastVertex Sub(RecastVertex left, RecastVertex right)
        {
            return new RecastVertex
                       {
                           X = left.X - right.X,
                           Y = left.Y - right.Y,
                           Z = left.Z - right.Z
                       };
        }

        public static RecastVertex Mult(RecastVertex left, int scale)
        {
            return new RecastVertex
                       {
                           X = left.X * scale,
                           Y = left.Y * scale,
                           Z = left.Z * scale
                       };
        }

        public static RecastVertex Mult(RecastVertex left, RecastVertex right, int scale)
        {
            return new RecastVertex
                       {
                           X = left.X + (right.X * scale),
                           Y = left.Y + (right.Y * scale),
                           Z = left.Z + (right.Z * scale)
                       };
        }

        public static RecastVertex Cross(RecastVertex left, RecastVertex right)
        {
            return new RecastVertex
                       {
                           X = left.Y * right.Z - left.Z * right.Y,
                           Y = left.Z * right.X - left.X * right.Z,
                           Z = left.X * right.Y - left.Y * right.X
                       };
        }

        public static float Dot(RecastVertex left, RecastVertex right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static RecastVertex Min(RecastVertex left, RecastVertex right)
        {
            return new RecastVertex
                       {
                           X = Math.Min(left.X, right.X),
                           Y = Math.Min(left.Y, right.Y),
                           Z = Math.Min(left.Z, right.Z)
                       };
        }

        public static RecastVertex Max(RecastVertex left, RecastVertex right)
        {
            return new RecastVertex
                       {
                           X = Math.Max(left.X, right.X),
                           Y = Math.Max(left.Y, right.Y),
                           Z = Math.Max(left.Z, right.Z)
                       };
        }

        public static float Distance(RecastVertex left, RecastVertex right)
        {
            float dx = right.X - left.X;
            float dy = right.Y - left.Y;
            float dz = right.Z - left.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static float SquareDistance(RecastVertex left, RecastVertex right)
        {
            float dx = right.X - left.X;
            float dy = right.Y - left.Y;
            float dz = right.Z - left.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public void Normalize()
        {
            float d = (float)(1.0f/Math.Sqrt(X * X + Y * Y + Z * Z));
            X *= d;
            Y *= d;
            Z *= d;
        }
    }
}